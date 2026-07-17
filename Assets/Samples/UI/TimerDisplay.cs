using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HourPeak.Settings;

/// <summary>
/// Отображение таймера с цветовой индикацией в зависимости от оставшегося времени.
/// Цвета меняются в зависимости от уровня сложности и критичности времени.
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Формат времени M:SS:ms (минуты:секунды:миллисекунды).
    /// </summary>
    private const string TIME_FORMAT = "{0:0}:{1:00}.{2:000}";

    #endregion

    #region Fields

    [Header("UI References")]
    [Tooltip("Текст таймера (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Tooltip("Панель таймера (для скрытия при конце времени)")]
    [SerializeField] private GameObject timerDisplayPanel;
    
    [Tooltip("Меню конца игры (показывается когда время вышло)")]
    [SerializeField] private GameObject endMenu;

    [Header("Colors")]
    [Tooltip("Безопасное время (спокойствие) - белый")]
    [SerializeField] private Color safeColor = Color.white;
    
    [Tooltip("Серьёзное время (предупреждение) - жёлтый")]
    [SerializeField] private Color warningColor = Color.yellow;
    
    [Tooltip("Критическое время (опасность) - красный")]
    [SerializeField] private Color dangerColor = Color.red;
    
    [Tooltip("Последний шанс (эскалация) - бордовый")]
    [SerializeField] private Color lastChanceColor = new Color(0.5f, 0f, 0f);
    
    [Tooltip("Время вышло (провал) - чёрный")]
    [SerializeField] private Color timeUpColor = Color.black;

    [Header("Settings")]
    [Tooltip("Текущий уровень сложности")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
    
    [Tooltip("Начинать таймер сразу")]
    [SerializeField] private bool startAutomatically = true;
    
    [Tooltip("Закрывать панель таймера и показывать EndMenu когда время вышло")]
    [SerializeField] private bool showEndMenuOnTimeUp = true;

    #endregion

    #region Private State

    /// <summary>
    /// Текущее оставшееся время (в секундах).
    /// </summary>
    private float remainingTime;
    
    /// <summary>
    /// Общее время на уровне сложности.
    /// </summary>
    private float totalTime;
    
    /// <summary>
    /// Флаг: запущен ли таймер.
    /// </summary>
    private bool isRunning;
    
    /// <summary>
    /// Флаг: вышло ли время.
    /// </summary>
    private bool isTimeUp;

    #endregion

    #region Properties

    /// <summary>
    /// Оставшееся время (только для чтения).
    /// </summary>
    public float RemainingTime => remainingTime;
    
    /// <summary>
    /// Общее время (только для чтения).
    /// </summary>
    public float TotalTime => totalTime;
    
    /// <summary>
    /// Таймер запущен (только для чтения).
    /// </summary>
    public bool IsRunning => isRunning;
    
    /// <summary>
    /// Время вышло (только для чтения).
    /// </summary>
    public bool IsTimeUp => isTimeUp;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Кэшируем ссылки
        if (timerDisplayPanel == null)
        {
            timerDisplayPanel = gameObject;
        }
    }

    private void Start()
    {
        if (startAutomatically)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning || isTimeUp)
            return;

        UpdateTimer();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Запускает таймер с текущим уровнем сложности.
    /// </summary>
    public void StartTimer()
    {
        // Получаем настройки времени для текущего уровня сложности
        LoadDifficultySettings();
        
        remainingTime = totalTime;
        isRunning = true;
        isTimeUp = false;
        
        // Показываем таймер, скрываем EndMenu
        if (timerDisplayPanel != null)
            timerDisplayPanel.SetActive(true);
        
        if (endMenu != null)
            endMenu.SetActive(false);
        
        UpdateDisplay();
        
        Debug.Log($"⏱️ Таймер запущен: {FormatTime(totalTime)} ({GetDifficultyName()})");
    }

    /// <summary>
    /// Останавливает таймер.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("⏸️ Таймер остановлен");
    }

    /// <summary>
    /// Возобновляет таймер.
    /// </summary>
    public void ResumeTimer()
    {
        if (!isTimeUp)
        {
            isRunning = true;
            Debug.Log("▶️ Таймер возобновлён");
        }
    }

    /// <summary>
    /// Сбрасывает таймер и запускает заново.
    /// </summary>
    public void ResetTimer()
    {
        isRunning = false;
        isTimeUp = false;
        StartTimer();
    }

    /// <summary>
    /// Устанавливает уровень сложности и перезапускает таймер.
    /// </summary>
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        ResetTimer();
    }

    /// <summary>
    /// Добавляет время к таймеру.
    /// </summary>
    public void AddTime(float seconds)
    {
        if (!isTimeUp)
        {
            remainingTime += seconds;
            Debug.Log($"+{seconds}с к времени. Всего: {FormatTime(remainingTime)}");
        }
    }

    /// <summary>
    /// Отнимает время у таймера.
    /// </summary>
    public void RemoveTime(float seconds)
    {
        if (!isTimeUp)
        {
            remainingTime = Mathf.Max(0, remainingTime - seconds);
            Debug.Log($"-{seconds}с от времени. Осталось: {FormatTime(remainingTime)}");
        }
    }

    /// <summary>
    /// Устанавливает оставшееся время напрямую.
    /// </summary>
    public void SetTime(float seconds)
    {
        remainingTime = Mathf.Max(0, seconds);
        if (remainingTime == 0 && isRunning)
        {
            TimeUp();
        }
    }

    #endregion

    #region Timer Logic

    /// <summary>
    /// Загружает настройки времени для текущего уровня сложности.
    /// </summary>
    private void LoadDifficultySettings()
    {
        SettingDifficulty difficultyManager = SettingDifficulty.Instance;
        
        if (difficultyManager != null)
        {
            // Используем AcceptableTime как МАКСИМАЛЬНОЕ время уровня
            totalTime = difficultyManager.GetSettingsByLevel(currentDifficulty).AcceptableTime;
            
            Debug.Log($"📋 Настройки сложности загружены: {GetDifficultyName()} | " +
                      $"Макс. время: {totalTime}с");
        }
        else
        {
            // Fallback
            switch (currentDifficulty)
            {
                case DifficultyLevel.Beginner:
                    totalTime = 240f;
                    break;
                case DifficultyLevel.Professional:
                    totalTime = 120f;
                    break;
                case DifficultyLevel.Extremal:
                    totalTime = 60f;
                    break;
                default:
                    totalTime = 240f;
                    break;
            }
            
            Debug.LogWarning($"⚠️ SettingDifficulty не найден! Используем локальные настройки: {totalTime}с");
        }
    }

    /// <summary>
    /// Обновляет таймер каждый кадр.
    /// </summary>
    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            TimeUp();
        }
        
        UpdateDisplay();
    }

    /// <summary>
    /// Вызывается когда время вышло.
    /// </summary>
    private void TimeUp()
    {
        isTimeUp = true;
        isRunning = false;
        
        Debug.Log("⏰ Время вышло!");
        
        // Обновляем цвет на чёрный
        if (timerText != null)
        {
            timerText.color = timeUpColor;
        }
        
        // Показываем EndMenu
        if (showEndMenuOnTimeUp)
        {
            ShowEndMenu();
        }
    }

    /// <summary>
    /// Показывает меню конца игры.
    /// </summary>
    private void ShowEndMenu()
    {
        if (timerDisplayPanel != null)
            timerDisplayPanel.SetActive(false);
        
        if (endMenu != null)
            endMenu.SetActive(true);
        
        Debug.Log("🏁 EndMenu показан");
    }

    #endregion

    #region UI Update

    /// <summary>
    /// Обновляет отображение таймера (текст и цвет).
    /// </summary>
    private void UpdateDisplay()
    {
        if (timerText == null)
            return;

        // Форматируем время
        timerText.text = FormatTime(remainingTime);
        
        // Обновляем цвет
        UpdateColor();
    }

    /// <summary>
    /// Обновляет цвет таймера в зависимости от ПРОШЕДШЕГО времени.
    /// </summary>
    private void UpdateColor()
    {
        if (timerText == null)
            return;

        // Получаем пороги для текущего уровня сложности
        (float warningStart, float dangerStart, float lastChanceStart, float maxTime) = GetThresholds();
        
        // Вычисляем прошедшее время (elapsedTime — это по сумме remainingTime в обратном отсчёте,
        // но нам нужно время от 0, поэтому elapsed = maxTime - remainingTime)
        float elapsedTime = totalTime - remainingTime;
        
        // Определяем цвет на основе прошедшего времени
        if (elapsedTime < warningStart)
        {
            // Спокойное время: прошло 0-12.5%
            timerText.color = safeColor;
        }
        else if (elapsedTime < dangerStart)
        {
            // Серьёзное время: прошло 12.5-25%
            timerText.color = warningColor;
        }
        else if (elapsedTime < lastChanceStart)
        {
            // Критическое время: прошло 25-50%
            timerText.color = dangerColor;
        }
        else if (elapsedTime < totalTime)
        {
            // Последний шанс: прошло 50-100%
            timerText.color = lastChanceColor;
        }
        else
        {
            // Время вышло
            timerText.color = timeUpColor;
        }
    }

    /// <summary>
    /// Получает пороги в СЕКУНДАХ для текущего уровня сложности.
    /// Время начинается с 0 и растёт до максимума.
    /// 
    /// Новичок (240с макс):
    /// - Спокойное:    0-30с     (0-12.5%)
    /// - Серьёзное:    30-60с    (12.5-25%)
    /// - Критическое:  60-120с   (25-50%)
    /// - Последний шанс: 120-240с (50-100%)
    /// 
    /// Профессионал (120с макс):
    /// - Спокойное:    0-15с     (0-12.5%)
    /// - Серьёзное:    15-30с    (12.5-25%)
    /// - Критическое:  30-60с    (25-50%)
    /// - Последний шанс: 60-120с  (50-100%)
    /// 
    /// Экстремал (60с макс):
    /// - Спокойное:    0-7.5с    (0-12.5%)
    /// - Серьёзное:    7.5-15с   (12.5-25%)
    /// - Критическое:  15-30с    (25-50%)
    /// - Последний шанс: 30-60с  (50-100%)
    /// </summary>
    private (float warningStart, float dangerStart, float lastChanceStart, float maxTime) GetThresholds()
    {
        return currentDifficulty switch
        {
            DifficultyLevel.Beginner => (30f, 60f, 120f, 240f),
            DifficultyLevel.Professional => (15f, 30f, 60f, 120f),
            DifficultyLevel.Extremal => (7.5f, 15f, 30f, 60f),
            _ => (30f, 60f, 120f, 240f)
        };
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Форматирует время в MM:SS:ms (минуты:секунды:миллисекунды).
    /// </summary>
    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time % 1f) * 1000);
        return string.Format(TIME_FORMAT, minutes, seconds, milliseconds);
    }

    /// <summary>
    /// Получает название уровня сложности.
    /// </summary>
    private string GetDifficultyName()
    {
        return currentDifficulty switch
        {
            DifficultyLevel.Beginner => "Новичок",
            DifficultyLevel.Professional => "Профессионал",
            DifficultyLevel.Extremal => "Экстремал",
            _ => "Неизвестно"
        };
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;

        GUILayout.BeginArea(new Rect(10, 200, 300, 200));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("⏱️ TimerDisplay Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"Осталось: {FormatTime(remainingTime)}");
        GUILayout.Label($"Всего: {FormatTime(totalTime)}");
        GUILayout.Label($"Доля: {(totalTime > 0 ? (remainingTime / totalTime * 100f): 0f):F1}%");
        GUILayout.Label($"Сложность: {GetDifficultyName()}");
        GUILayout.Label($"Запущен: {isRunning}");
        GUILayout.Label($"Время вышло: {isTimeUp}");
        GUILayout.Label($"Цвет: {timerText?.color ?? Color.white}");
        GUILayout.Label("═══════════════════════════════");
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Старт")) StartTimer();
        if (GUILayout.Button("Стоп")) StopTimer();
        if (GUILayout.Button("Рестарт")) ResetTimer();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
#endif
    }

    #endregion
}