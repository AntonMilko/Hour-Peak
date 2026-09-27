using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using HourPeak.Settings;
using HourPeak.Samples.Runtime;
using System;

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
    [SerializeField] private TextMeshPro timerText;
    
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

    [Tooltip("Старт таймера когда уровень начался")]
    [SerializeField] private bool isStartTimerWhenLevelIsBegan = true;
    
    [Tooltip("Закрывать панель таймера и показывать EndMenu когда время вышло")]
    [SerializeField] private bool showEndMenuOnTimeUp = true;

    [Tooltip("Закрывать панель таймера и показывать EndMenu когда дошли до пункта Б вовремя")]
    [SerializeField] private bool showEndMenuOnIsFinished = true;

    #endregion

    #region Public State

    /// <summary>
    /// Фиксированное время прибытия
    /// </summary>
    public float fixedTime;

    /// <summary>
    /// Текущее оставшееся время (в секундах).
    /// </summary>
    public float remainingTime;

    /// <summary
    /// Время прибытия до пункта Б.
    /// </summary>
    public float arrivalTime;
    
    /// <summary>
    /// Общее время на уровне сложности.
    /// </summary>
    public float totalTime;
    
    /// <summary>
    /// Флаг: запущен ли таймер.
    /// </summary>
    public bool isRunning;
    
    /// <summary>
    /// Флаг: вышло ли время.
    /// </summary>
    public bool isTimeUp;

    /// <summary>
    /// Флаг: дошли ли до пункта Б вовремя.
    /// </summary>
    public bool isFinished;

    #endregion

    #region Public Methods

    /// <summary>
    /// Самостоятельная инициализация
    /// </summary>
    public void Awake()
    {
        Update();
        StartTimer();
        StopTimer();
        ResumeTimer();
        ResetTimer();
        LoadDifficultySettings();
    }

    /// <summary>
    /// Обновляет UI.
    /// </summary>
    public void Update()
    {
        remainingTime += Time.deltaTime;        
        UpdateDisplay();
    }

    /// <summary>
    /// Запускает таймер с текущим уровнем сложности.
    /// </summary>
    public void StartTimer()
    {
        // Вызываемся когда уровень начался
        Update();

        // Получаем настройки времени для текущего уровня сложности
        LoadDifficultySettings();

        remainingTime = 0;
        isRunning = true;
        isFinished = false;
        isTimeUp = false;
        isStartTimerWhenLevelIsBegan = true;
        
        remainingTime += Time.deltaTime;        
        UpdateDisplay();
    }

    /// <summary>
    /// Останавливает таймер.
    /// </summary>
    public void StopTimer()
    {
        Update();
        isRunning = false;
        isFinished = false;
        isTimeUp = false;
        isStartTimerWhenLevelIsBegan = false;
        Debug.Log("Таймер остановлен");
    }

    /// <summary>
    /// Возобновляет таймер.
    /// </summary>
    public void ResumeTimer()
    {
        if (!isRunning)
        {
            Update();
            isRunning = true;
            isStartTimerWhenLevelIsBegan = true;
            isFinished = false;
            Debug.Log("Таймер возобновлён");
        }

        else
        {
            Update();
            isTimeUp = true;
            isStartTimerWhenLevelIsBegan = false;
            isFinished = false;
            Debug.Log("Время вышло");
        }
    }

    /// <summary>
    /// Сбрасывает таймер и запускает заново.
    /// </summary>
    public void ResetTimer()
    {
        isRunning = false;
        isTimeUp = false;
        isFinished = false;
        isStartTimerWhenLevelIsBegan = true;
        StartTimer();
        Update();
    }

    /// <summary>
    /// Устанавливает уровень сложности и перезапускает таймер.
    /// </summary>
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        StartTimer();
        ResumeTimer();
        ResetTimer();
        Update();
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
            
            Debug.Log($"Настройки сложности загружены: {GetDifficultyName()} Макс. время: {totalTime}с");
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
    /// Вызывается когда таймер завершён.
    /// </summary>
    private void OnIsFinished()
    {
        Debug.Log("Таймер завершён успешно!");

        // Отображается EndMenu после того как столкнулся с GameObject, там где скрипт EndManager
        if (showEndMenuOnIsFinished)
        {
            Debug.Log("Дошли до пункта Б вовремя");
            timerDisplayPanel.SetActive(false);
            isStartTimerWhenLevelIsBegan = false;
            showEndMenuOnIsFinished = true;
            showEndMenuOnTimeUp = false;
            indicatorSuccessForEndMenu.StartAnimation(arrivalTime);
            ShowEndMenu();
            StopTimer();
            Update();
        }

        else
        {
            Debug.Log("Ещё не дошли до пункта Б вовремя");
            timerDisplayPanel.SetActive(false);
            isStartTimerWhenLevelIsBegan = false;
            showEndMenuOnIsFinished = false;
            showEndMenuOnTimeUp = true;
            indicatorSuccessForEndMenu.StartAnimation(totalTime);
            ShowEndMenu();
            StopTimer();
            Update();
        }
    }

    /// <summary>
    /// Вызывается когда время вышло.
    /// </summary>
    private void TimeUp()
    {
        isTimeUp = true;
        isRunning = false;
        
        Debug.Log("Время вышло!");
        
        // Обновляем цвет на чёрный
        if (timerText != null)
        {
            Debug.Log("Время вышло! Обновляем цвет на чёрный");
            StopTimer();
            Update();
            timerText.color = timeUpColor;
        }

        else
        {
            Debug.Log("Время вышло! Обновляем цвет на чёрный");
            StopTimer();
            Update();
            timerDisplayPanel.SetActive(false);
        }
        
        // Показываем EndMenu
        if (showEndMenuOnTimeUp)
        {
            Debug.Log("Время вышло! Остановка таймера...");
            ShowEndMenu();
            Update();
            isStartTimerWhenLevelIsBegan = false;
            showEndMenuOnTimeUp = true;
        }

        else
        {
            Debug.Log("Время вышло! Остановка таймера...");
            ShowEndMenu();
            Update();
            indicatorSuccessForEndMenu.StartAnimation(totalTime);
            timerDisplayPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Показывает меню конца игры.
    /// </summary>
    private void ShowEndMenu()
    {
        Debug.Log("Показываем EndMenu, когда время вышло");

        // Отображаем EndMenu
        if (endMenu != null)
        {
            Debug.Log("Время вышло");
            timerDisplayPanel.SetActive(false);
            isStartTimerWhenLevelIsBegan = false;
            showEndMenuOnTimeUp = true;
            showEndMenuOnIsFinished = false;
            indicatorSuccessForEndMenu.StartAnimation(totalTime);
            ShowEndMenu();
            StopTimer();
            TimeUp();
            OnIsFinished();
            Update();
        }

        else
        {
            Debug.Log("Дошли до пункта Б вовремя");
            timerDisplayPanel.SetActive(false);
            isStartTimerWhenLevelIsBegan = false;
            showEndMenuOnTimeUp = false;
            showEndMenuOnIsFinished = true;
            indicatorSuccessForEndMenu.StartAnimation(fixedTime);
            ShowEndMenu();
            StopTimer();
            TimeUp();
            OnIsFinished();
            Update();
        }
        
        indicatorSuccessForEndMenu.StartAnimation(remainingTime);
        Debug.Log("EndMenu показан");
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

        // Вызываемся когда уровень начался
        Update();
    }

    /// <summary>
    /// Обновляет цвет таймера в зависимости от ПРОШЕДШЕГО времени.
    /// </summary>
    private void UpdateColor()
    {
        if (timerText == null)
            return;

        // Получаем пороги для текущего уровня сложности
        (float warningStart, float dangerStart, float lastChanceStart, float totalTime) = GetThresholds();
        
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
    private (float warningStart, float dangerStart, float lastChanceStart, float totalTime) GetThresholds()
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