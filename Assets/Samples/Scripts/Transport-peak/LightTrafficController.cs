using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HourPeak.Transport
{
    /// <summary>
    /// Тип транспортного сигнала.
    /// </summary>
    public enum TrafficSignalType
    {
        Red,
        Yellow,
        Green
    }

    /// <summary>
    /// Направления движения для транспортных светофоров.
    /// </summary>
    [Flags]
    public enum TrafficDirection
    {
        None = 0,
        North = 1,
        South = 2,
        East = 4,
        West = 8,
        All = North | South | East | West
    }

    /// <summary>
    /// Конфигурация одного транспортного сигнала.
    /// </summary>
    [Serializable]
    public class TrafficLightConfig
    {
        [Header("References")]
        [Tooltip("GameObject красного света")]
        public GameObject redLight;
        [Tooltip("GameObject жёлтого света")]
        public GameObject yellowLight;
        [Tooltip("GameObject зелёного света")]
        public GameObject greenLight;

        [Header("Visual Settings")]
        [Tooltip("Базовый материал (выключенный)")]
        public Material offMaterial;
        [Tooltip("Материал активного красного")]
        public Material redMaterial;
        [Tooltip("Материал активного жёлтого")]
        public Material yellowMaterial;
        [Tooltip("Материал активного зелёного")]
        public Material greenMaterial;

        [Header("Timing (секунды)")]
        [Tooltip("Длительность зелёного")]
        public float greenDuration = 10f;
        [Tooltip("Длительность мигания зелёного")]
        public float greenFlashDuration = 3f;
        [Tooltip("Длительность жёлтого")]
        public float yellowDuration = 3f;
        [Tooltip("Длительность красного")]
        public float redDuration = 5f;

        [Header("Direction")]
        [Tooltip("Направление, для которого действует этот сигнал")]
        public TrafficDirection direction = TrafficDirection.All;

        public void SetSignal(TrafficSignalType signal)
        {
            // ... ваш существующий код SetSignal ...
            ApplyMaterials(signal);
        }

        private void ApplyMaterials(TrafficSignalType signal)
        {
            if (offMaterial != null) offMaterial.color = GetColor(signal);
            if (redMaterial != null) redMaterial.color = GetColor(signal);
            if (yellowMaterial != null) yellowMaterial.color = GetColor(signal);
            if (greenMaterial != null) greenMaterial.color = GetColor(signal);
        }

        private Color GetColor(TrafficSignalType signal)
        {
            return signal switch
            {
                TrafficSignalType.Red => Color.red,
                TrafficSignalType.Yellow => Color.yellow,
                TrafficSignalType.Green => Color.green,
                _ => Color.black
            };
        }
    }

    /// <summary>
    /// Главный контроллер транспортных светофоров.
    /// Управляет переключением сигналов для нескольких направлений.
    /// Связан с пешеходными светофорами для синхронизации.
    /// </summary>
    public class LightTrafficController : MonoBehaviour
    {
        #region Constants

        private const float DefaultGreenDuration = 5f;
        private const float DefaultYellowDuration = 2f;

        #endregion

        #region Fields

        [Header("Signal Configurations")]
        [Tooltip("Список транспортных сигналов (по одному на направление)")]
        [SerializeField] private List<TrafficLightConfig> signalConfigs = new();

        [Header("Pedestrian Signal Configs")]
        [Tooltip("Список пешеходных сигналов (по одному на переход)")]
        [SerializeField] private List<PedestrianLightConfig> pedestrianSignalConfigs = new();

        [Header("Settings")]
        [Tooltip("Автоматический запуск при старте")]
        [SerializeField] private bool autoStart = true;

        [Header("Debug")]
        [Tooltip("Логировать переключения в консоль")]
        [SerializeField] private bool logSwitches = true;

        #endregion

        #region Private State

        private TrafficSignalType _currentSignal;
        private float _timer;
        private bool _isRunning;
        private Coroutine _coroutine;

        #endregion

        #region Public Methods

        /// <summary>
        /// Запускает цикл переключения сигналов.
        /// Начальный сигнал — Green для всех направлений.
        /// </summary>
        public void StartCycle()
        {
            if (_isRunning) return;

            _isRunning = true;
            SetSignal(TrafficSignalType.Green);
            _coroutine = StartCoroutine(SignalCoroutine());
        }

        /// <summary>
        /// Останавливает цикл переключения.
        /// </summary>
        public void StopCycle()
        {
            _isRunning = false;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        /// <summary>
        /// Мгновенно устанавливает указанный сигнал.
        /// </summary>
        public void SetSignal(TrafficSignalType signal)
        {
            if (_currentSignal == signal) return;

            _currentSignal = signal;

            foreach (var config in signalConfigs)
            {
                config.SetSignal(signal);
            }

            // Синхронизация с пешеходными светофорами
            SyncPedestrianLights(signal);

            if (logSwitches)
            {
                Debug.Log($"🚦 Сигнал: {GetSignalName(signal)}");
            }
        }

        /// <summary>
        /// Принудительно переключает на следующий сигнал в цикле.
        /// Порядок: Green → Yellow → Red → Green
        /// </summary>
        public void ForceNextSignal()
        {
            TrafficSignalType next = _currentSignal switch
            {
                TrafficSignalType.Green => TrafficSignalType.Yellow,
                TrafficSignalType.Yellow => TrafficSignalType.Red,
                TrafficSignalType.Red => TrafficSignalType.Green,
                _ => TrafficSignalType.Green
            };
            SetSignal(next);
        }

        /// <summary>
        /// Возвращает читаемое название сигнала.
        /// </summary>
        public static string GetSignalName(TrafficSignalType signal)
        {
            return signal switch
            {
                TrafficSignalType.Red => "Красный (Стоп)",
                TrafficSignalType.Yellow => "Жёлтый (Внимание)",
                TrafficSignalType.Green => "Зелёный (Ехать)",
                _ => "Неизвестно"
            };
        }

        #endregion

        #region Events

        /// <summary>
        /// Вызывается при каждом обновлении таймера.
        /// </summary>
        public event Action<float> OnTimerUpdate;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (autoStart)
            {
                StartCycle();
            }
        }

        private void OnDestroy()
        {
            StopCycle();
        }

        #endregion

        #region Coroutines

        private IEnumerator SignalCoroutine()
        {
            while (_isRunning)
            {
                while (_timer > 0f && _isRunning)
                {
                    _timer -= Time.deltaTime;
                    yield return null;
                }

                ForceNextSignal();
                _timer = 5f;
            }
        }

        #endregion

        #region Pedestrian Sync

        /// <summary>
        /// Синхронизирует пешеходные светофоры с транспортными.
        /// Зелёный транспортный → Красный пешеходный (идти нельзя).
        /// Красный транспортный → Зелёный пешеходный (идти можно).
        /// </summary>
        private void SyncPedestrianLights(TrafficSignalType carSignal)
        {
            // Синхронизация пешеходных сигналов
            TrafficSignalType pedestrianSignal = carSignal switch
            {
                TrafficSignalType.Green => TrafficSignalType.Red,
                TrafficSignalType.Yellow => TrafficSignalType.Red,
                TrafficSignalType.Red => TrafficSignalType.Green,
                _ => TrafficSignalType.Red
            };

            foreach (var config in pedestrianSignalConfigs)
            {
                config.SetSignal(pedestrianSignal);
            }
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Start Cycle")]
        private void DebugStart() => StartCycle();

        [ContextMenu("Stop Cycle")]
        private void DebugStop() => StopCycle();

        [ContextMenu("Force Next Signal")]
        private void DebugNext() => ForceNextSignal();
#endif
    }
}