using UnityEngine;
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
    private const string TIME_FORMAT = "{0:0}:{1:00}:{2:000}";

    #endregion

    #region Fields

    [Header("UI References")]
    [Tooltip("Текст таймера (TextMeshPro)")]
    [SerializeField] private TMP_Text timerText;
    
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

    [Header("Difficulty")]
    [Tooltip("Текущий уровень сложности")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
    
    [Header("Settings")]
    [Tooltip("Проверка сцены")]
    [SerializeField] private bool isFirstScene;

    #endregion

    #region Global time state

    /// <summary>
    /// Текущее оставшееся время (в секундах).
    /// </summary>
    public static float time { get; private set; }
    
    /// <summary>
    /// Общее время на уровне сложности.
    /// </summary>
    public static float totalTime { get; private set; }

    /// <summary>
    /// Состояние завершения уровня.
    /// </summary>
    public static bool isFinished { get; private set; }
    
    /// <summary>
    /// Флаг: запущен ли таймер.
    /// </summary>
    public static bool isRunning { get; private set; }

    #endregion

    #region Public Methods

    public bool isTimeUp()
    {
        return time >= totalTime;
    }

    public void SetIsRunning(bool isRunning)
    {
        TimerDisplay.isRunning = isRunning;
    }

    public void SetIsFinished(bool isFinished)
    {
        TimerDisplay.isFinished = isFinished;
    }

    /// <summary>
    /// Самостоятельная инициализация
    /// </summary>
    public void Awake()
    {
        LoadDifficultySettings();
        isRunning = true;
        isFinished = false;
        if (isFirstScene)
        {
            time = 0f;
        }
    }

    /// <summary>
    /// Обновляет UI.
    /// </summary>
    public void Update()
    {
        if (!isRunning)
            return;

        time += Time.deltaTime;
        timerText.text = FormatTime(time);
        UpdateColor();

        if (time >= totalTime)
            TimeUp();

        if (isFinished)
            OnIsFinished();
    }

    #endregion

    #region Timer Logic

    /// <summary>
    /// Вызывается когда таймер завершён.
    /// </summary>
    public void OnIsFinished()
    {
        Debug.Log("Дошли до пункта Б вовремя");
        ShowEndMenu();
    }

    /// <summary>
    /// Вызывается когда время вышло.
    /// </summary>
    private void TimeUp()
    {
        Debug.Log("Время вышло! Обновляем цвет на чёрный");
        ShowEndMenu();
    }

    /// <summary>
    /// Показывает меню конца игры.
    /// </summary>
    private void ShowEndMenu()
    {
        if (isRunning)
        {
            endMenu.SetActive(false);
            Debug.Log("EndMenu не показан во время прохождения уровня");
        }

        else if (isFinished)
        {
            endMenu.SetActive(true);
            Debug.Log("EndMenu показан после успешного прохождения уровня");
        }

        else if (time >= totalTime)
        {
            endMenu.SetActive(true);
            Debug.Log("EndMenu показан после провального прохождения уровня");
        }

        else
        {
            endMenu.SetActive(false);
            Debug.Log("EndMenu не показан во время прохождения уровня");
        }
    }

    #endregion

    #region UI Update

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
    /// Обновляет цвет таймера в зависимости от ПРОШЕДШЕГО времени.
    /// </summary>
    private void UpdateColor()
    {        
        // Определяем цвет на основе прошедшего времени
        if (time < totalTime/8)
        {
            // Спокойное время: прошло 0-12.5%
            timerText.color = safeColor;
        }
        else if (time < totalTime/4)
        {
            // Серьёзное время: прошло 12.5-25%
            timerText.color = warningColor;
        }
        else if (time < totalTime/2)
        {
            // Критическое время: прошло 25-50%
            timerText.color = dangerColor;
        }
        else if (time < totalTime)
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

    #endregion
}