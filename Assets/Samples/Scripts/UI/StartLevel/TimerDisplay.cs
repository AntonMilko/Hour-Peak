using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

using HourPeak.Settings;
using HourPeak.Samples.Runtime;

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
    private const string TIME_FORMAT = "{0:0}:{1:00}:{2:000}";

    public static TimerDisplay Instance { get; private set; }

    #endregion

    #region Fields

    [Header("UI References")]
    [Tooltip("Текст таймера (TextMeshPro)")]
    [SerializeField] private TMP_Text timerText;
    
    [Tooltip("Панель таймера (для скрытия при конце времени)")]
    [SerializeField] private GameObject timerDisplayPanel;
    
    [Tooltip("Меню конца игры (показывается когда время вышло)")]
    [SerializeField] private GameObject endMenu;

    [Tooltip("Меню конца игры (показывается когда время вышло)")]
    [SerializeField] private IndicatorSuccessForEndMenu indicatorSuccessForEndMenu;

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
    
    [Tooltip("Закрывать панель таймера и показывать EndMenu когда время вышло")]
    [SerializeField] private bool showEndMenuOnTimeUp = true;

    [Tooltip("Таймер запущен при запуске игры")]
    [SerializeField] private bool isTimerRunning = true;

    [Tooltip("Таймер включён (можно отключить для окончания уровня)")]
    [SerializeField] private bool timerOn = true;

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
    private string sceneBuildIndex;

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

    /// <summary>
    /// Таймер включён (только для чтения).
    /// </summary>
    public bool TimerOn { get => timerOn; set => timerOn = value; }

    /// <summary>
    /// Флаг: таймер запущен (только для чтения).
    /// </summary>
    public bool IsTimerRunning { get => isTimerRunning; set => isTimerRunning = value; }

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        timerOn = true;
    }

    private void Awake()
    {
        if(remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            updateTimer(remainingTime);
        }
        else
        {
            remainingTime = 0f;
            timerOn = false;
        }
    }

    private void updateTimer(float timer)
    {
        timer += 1;
        float min = Mathf.FloorToInt(timer / 60);
        float sec = Mathf.FloorToInt(timer % 60);
        float ms = Mathf.FloorToInt((timer % 1) * 1000);

        timerText.text = string.Format("{0:0}:{1:00}:{2:000}", min, sec, ms);
    }

    #endregion

    #region Public Methods

    public void startTimer()
    {
        timerOn = true;
        LoadDifficultySettings();
    }

    public void stopTimer()
    {
        timerOn = false;
        LoadDifficultySettings();
    }

    public void nextScene()
    {
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        LoadDifficultySettings();
    }

    /// <summary>
    /// Устанавливает уровень сложности и перезапускает таймер.
    /// </summary>
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        stopTimer();
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
            remainingTime = Mathf.Max(totalTime + seconds);
            Debug.Log($"+{seconds}с от времени. Осталось: {FormatTime(remainingTime)}");
        }
    }

    /// <summary>
    /// Устанавливает оставшееся время напрямую.
    /// </summary>
    public void SetTime(float seconds)
    {
        remainingTime = Mathf.Max(totalTime, seconds);
        if (remainingTime == totalTime && isRunning)
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
        remainingTime += Time.deltaTime;
        
        if (remainingTime >= totalTime)
        {
            remainingTime = totalTime;
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
        
        indicatorSuccessForEndMenu.StartAnimation(remainingTime);
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
        // но нам нужно время от 0, поэтому elapsed = 0 + remainingTime)
        float elapsedTime = 0 + remainingTime;
        
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
}