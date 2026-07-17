using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Контроллер координации светофоров на перекрёстке для Hour-peak.
/// Управляет несколькими светофорами для предотвращения заторов.
/// </summary>
public class SignalIntersectionController : MonoBehaviour
{
    [Header("Светофоры перекрёстка")]
    [Tooltip("Светофор для основного направления (север-юг)")]
    [SerializeField] private LightTraficSignal northSouthSignal;
    [Tooltip("Светофор для бокового направления (запад-восток)")]
    [SerializeField] private LightTraficSignal eastWestSignal;
    [Tooltip("Дополнительные светофоры перекрёстка")]
    [SerializeField] private List<LightTraficSignal> additionalSignals = new List<LightTraficSignal>();

    [Header("Настройки координации")]
    [Tooltip("Длительность зелёного света для основного направления")]
    [SerializeField] private float mainDirectionGreenTime = 15f;
    [Tooltip("Длительность зелёного света для бокового направления")]
    [SerializeField] private float crossDirectionGreenTime = 10f;
    [Tooltip("Длительность жёлтого света перед переключением")]
    [SerializeField] private float yellowTime = 3f;
    [Tooltip("Время всекрасного (безопасный интервал)")]
    [SerializeField] private float allRedTime = 2f;

    [Header("Режим работы")]
    [SerializeField] private IntersectionMode mode = IntersectionMode.Timed;
    [Tooltip("Активный светофор (управление вручную)")]
    [SerializeField] private LightTraficSignal activeSignal;

    [Header("Адаптивное управление")]
    [Tooltip("Включить адаптивное управление на основе трафика")]
    [SerializeField] private bool adaptiveMode = false;
    [Tooltip("Минимальная длительность зелёного")]
    [SerializeField] private float minGreenTime = 8f;
    [Tooltip("Максимальная длительность зелёного")]
    [SerializeField] private float maxGreenTime = 25f;
    [Tooltip("Порог количества машин для продления зелёного")]
    [SerializeField] private int vehicleThreshold = 3;

    [Header("События")]
    public UnityEvent<LightTraficSignal> OnSignalActivated;
    public UnityEvent<LightTraficSignal> OnSignalDeactivated;
    public UnityEvent OnPhaseChanged;

    public enum IntersectionMode
    {
        Timed,           // Таймерное переключение
        Manual,          // Ручное управление
        Adaptive         // Адаптивное на основе трафика
    }

    private LightTraficSignal[] allSignals;
    private LightTraficSignal currentGreenSignal;
    private LightTraficSignal currentYellowSignal;
    private float phaseTimer;
    private bool isTransitioning = false;
    private int currentPhaseIndex = 0;

    // Для адаптивного режима
    private int waitingVehiclesMain = 0;
    private int waitingVehiclesCross = 0;
    private float extendedGreenTimer = 0f;

    private void Awake()
    {
        CollectAllSignals();
        LinkCoordinatedSignals();
    }

    private void Start()
    {
        InitializeIntersection();
    }

    private void Update()
    {
        if (isTransitioning)
        {
            HandleTransitionPhase();
        }
        else
        {
            HandleActivePhase();
        }

        if (adaptiveMode)
        {
            UpdateAdaptiveMetrics();
        }
    }

    /// <summary>
    /// Сбор всех светофоров перекрёстка
    /// </summary>
    private void CollectAllSignals()
    {
        List<LightTraficSignal> signals = new List<LightTraficSignal>();
        
        if (northSouthSignal != null) signals.Add(northSouthSignal);
        if (eastWestSignal != null) signals.Add(eastWestSignal);
        
        foreach (var signal in additionalSignals)
        {
            if (signal != null && !signals.Contains(signal))
                signals.Add(signal);
        }

        allSignals = signals.ToArray();
    }

    /// <summary>
    /// Связывание светофоров для координации
    /// </summary>
    private void LinkCoordinatedSignals()
    {
        foreach (var signal in allSignals)
        {
            if (signal != null)
            {
                // Добавляем все светофоры перекрёстка в группу координации
                foreach (var otherSignal in allSignals)
                {
                    if (otherSignal != null && otherSignal != signal && !signal.coordinatedSignals.Contains(otherSignal))
                    {
                        signal.coordinatedSignals.Add(otherSignal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Инициализация перекрёстка
    /// </summary>
    private void InitializeIntersection()
    {
        if (allSignals.Length == 0)
        {
            Debug.LogWarning("[SignalIntersectionController] Нет светофоров для управления!", this);
            return;
        }

        // Устанавливаем начальные значения длительностей
        UpdateSignalDurations();

        // Начинаем с основного направления
        currentGreenSignal = (mode == IntersectionMode.Manual && activeSignal != null) 
            ? activeSignal 
            : northSouthSignal ?? allSignals[0];

        StartGreenPhase(currentGreenSignal);
    }

    /// <summary>
    /// Обновление длительностей фаз для светофоров
    /// </summary>
    private void UpdateSignalDurations()
    {
        foreach (var signal in allSignals)
        {
            if (signal == null) continue;

            signal.SetPhaseDuration(LightTraficSignal.SignalState.Green, 
                signal == northSouthSignal ? mainDirectionGreenTime : crossDirectionGreenTime);
            signal.SetPhaseDuration(LightTraficSignal.SignalState.Yellow, yellowTime);
        }
    }

    /// <summary>
    /// Запуск зелёной фазы для светофора
    /// </summary>
    private void StartGreenPhase(LightTraficSignal signal)
    {
        if (signal == null) return;

        if (currentGreenSignal != null && currentGreenSignal != signal)
        {
            OnSignalDeactivated?.Invoke(currentGreenSignal);
        }

        currentGreenSignal = signal;
        phaseTimer = 0f;
        isTransitioning = false;

        signal.ForceState(LightTraficSignal.SignalState.Green);
        signal.ResumeCycle();
        
        OnSignalActivated?.Invoke(signal);
        OnPhaseChanged?.Invoke();

        Debug.Log($"[Перекрёсток] Зелёный для: {signal.gameObject.name}");
    }

    /// <summary>
    /// Обработка активной фазы (зелёный свет)
    /// </summary>
    private void HandleActivePhase()
    {
        if (currentGreenSignal == null) return;

        phaseTimer += Time.deltaTime;

        float greenTime = GetGreenTimeForSignal(currentGreenSignal);

        // Проверка на переход
        if (adaptiveMode)
        {
            HandleAdaptiveTransition();
        }
        else if (phaseTimer >= greenTime)
        {
            StartTransitionPhase();
        }
    }

    /// <summary>
    /// Получение длительности зелёного света с учётом адаптивного режима
    /// </summary>
    private float GetGreenTimeForSignal(LightTraficSignal signal)
    {
        if (!adaptiveMode)
        {
            return signal == northSouthSignal ? mainDirectionGreenTime : crossDirectionGreenTime;
        }

        // Адаптивная длительность
        int waitingVehicles = GetWaitingVehiclesForSignal(signal);
        float adaptiveTime = Mathf.Lerp(minGreenTime, maxGreenTime, 
            Mathf.InverseLerp(0, vehicleThreshold * 2, waitingVehicles));

        return Mathf.Clamp(adaptiveTime, minGreenTime, maxGreenTime);
    }

    /// <summary>
    /// Адаптивное управление переходом
    /// </summary>
    private void HandleAdaptiveTransition()
    {
        int waitingVehicles = GetWaitingVehiclesForSignal(currentGreenSignal);
        float currentGreenTime = GetGreenTimeForSignal(currentGreenSignal);

        // Если машин мало и прошло минимальное время - можно переключать
        if (waitingVehicles < vehicleThreshold && phaseTimer >= minGreenTime)
        {
            StartTransitionPhase();
        }
        // Если машин много и достигнут максимум - переключать обязательно
        else if (phaseTimer >= maxGreenTime)
        {
            StartTransitionPhase();
        }
    }

    /// <summary>
    /// Подсчёт ожидающих машин для светофора
    /// </summary>
    private int GetWaitingVehiclesForSignal(LightTraficSignal signal)
    {
        if (signal == null) return 0;
        return signal.WaitingVehicleCount;
    }

    /// <summary>
    /// Обновление метрик для адаптивного режима
    /// </summary>
    private void UpdateAdaptiveMetrics()
    {
        waitingVehiclesMain = northSouthSignal != null ? northSouthSignal.WaitingVehicleCount : 0;
        waitingVehiclesCross = eastWestSignal != null ? eastWestSignal.WaitingVehicleCount : 0;

        // Логика продления зелёного
        if (currentGreenSignal != null)
        {
            int waiting = GetWaitingVehiclesForSignal(currentGreenSignal);
            if (waiting >= vehicleThreshold && phaseTimer < maxGreenTime)
            {
                extendedGreenTimer += Time.deltaTime;
            }
            else
            {
                extendedGreenTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Запуск переходной фазы (жёлтый -> все красный)
    /// </summary>
    private void StartTransitionPhase()
    {
        isTransitioning = true;
        phaseTimer = 0f;

        // Включаем жёлтый для текущего зелёного
        if (currentGreenSignal != null)
        {
            currentYellowSignal = currentGreenSignal;
            currentGreenSignal.ForceState(LightTraficSignal.SignalState.Yellow);
            OnPhaseChanged?.Invoke();
        }
    }

    /// <summary>
    /// Обработка переходной фазы
    /// </summary>
    private void HandleTransitionPhase()
    {
        phaseTimer += Time.deltaTime;

        // Фаза жёлтого
        if (phaseTimer < yellowTime)
        {
            return;
        }

        // Фаза всекрасного
        phaseTimer -= yellowTime;
        
        if (currentYellowSignal != null)
        {
            currentYellowSignal.ForceState(LightTraficSignal.SignalState.Red);
            currentYellowSignal = null;
        }

        if (phaseTimer < allRedTime)
        {
            return;
        }

        // Переключение на следующий светофор
        phaseTimer -= allRedTime;
        SwitchToNextSignal();
    }

    /// <summary>
    /// Переключение на следующий светофор
    /// </summary>
    private void SwitchToNextSignal()
    {
        isTransitioning = false;
        currentPhaseIndex = (currentPhaseIndex + 1) % allSignals.Length;
        
        LightTraficSignal nextSignal = allSignals[currentPhaseIndex];
        StartGreenPhase(nextSignal);
    }

    /// <summary>
    /// Ручное переключение на конкретный светофор
    /// </summary>
    public void SetActiveSignal(LightTraficSignal signal)
    {
        if (signal == null || !allSignals.Contains(signal)) return;

        mode = IntersectionMode.Manual;
        activeSignal = signal;
        
        if (!isTransitioning)
        {
            StartGreenPhase(signal);
        }
    }

    /// <summary>
    /// Переключение на следующее направление (ручное)
    /// </summary>
    public void ManualNextPhase()
    {
        if (mode != IntersectionMode.Manual)
        {
            mode = IntersectionMode.Manual;
            activeSignal = currentGreenSignal;
        }

        if (!isTransitioning)
        {
            SwitchToNextSignal();
        }
    }

    /// <summary>
    /// Включение режима all-red (безопасный режим)
    /// </summary>
    public void EmergencyAllRed()
    {
        StopAllCoroutines();
        
        foreach (var signal in allSignals)
        {
            if (signal != null)
            {
                signal.ForceState(LightTraficSignal.SignalState.Red);
                signal.PauseCycle();
            }
        }

        currentGreenSignal = null;
        currentYellowSignal = null;
        isTransitioning = false;
    }

    /// <summary>
    /// Возобновление нормальной работы
    /// </summary>
    public void ResumeNormalOperation()
    {
        InitializeIntersection();
    }

    /// <summary>
    /// Добавление дополнительного светофора в перекрёсток
    /// </summary>
    public void AddSignal(LightTraficSignal signal)
    {
        if (signal != null && !additionalSignals.Contains(signal))
        {
            additionalSignals.Add(signal);
            CollectAllSignals();
            LinkCoordinatedSignals();
        }
    }

    /// <summary>
    /// Удаление светофора из перекрёстка
    /// </summary>
    public void RemoveSignal(LightTraficSignal signal)
    {
        if (signal != null && additionalSignals.Contains(signal))
        {
            additionalSignals.Remove(signal);
            CollectAllSignals();
        }
    }

    /// <summary>
    /// Получение текущего активного светофора
    /// </summary>
    public LightTraficSignal GetCurrentActiveSignal() => currentGreenSignal;

    /// <summary>
    /// Получение текущего режима работы
    /// </summary>
    public IntersectionMode GetMode() => mode;

    #region Editor Helpers

    private void OnDrawGizmos()
    {
        // Связи между светофорами
        Gizmos.color = Color.magenta;
        foreach (var signal in allSignals)
        {
            if (signal != null)
            {
                foreach (var other in allSignals)
                {
                    if (other != null && other != signal)
                    {
                        Gizmos.DrawLine(signal.transform.position, other.transform.position);
                    }
                }
            }
        }

        // Текущий активный светофор
        if (currentGreenSignal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentGreenSignal.transform.position, 3f);
        }
    }

    private void OnGUI()
    {
        #if UNITY_EDITOR
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Режим: {mode}");
        GUILayout.Label($"Активный: {currentGreenSignal?.gameObject.name ?? "Нет"}");
        GUILayout.Label($"Фаза: {(isTransitioning ? "Переход" : "Зелёный")}");
        GUILayout.Label($"Таймер фазы: {phaseTimer:F1}s");
        
        if (adaptiveMode)
        {
            GUILayout.Label($"Ожидающие (основн): {waitingVehiclesMain}");
            GUILayout.Label($"Ожидающие (боков): {waitingVehiclesCross}");
        }
        
        GUILayout.EndArea();
        #endif
    }

    #endregion
}