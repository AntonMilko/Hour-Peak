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
    [SerializeField] private TMP_Text timerText;
    
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

    #endregion

    #region Public Methods

    /// <summary>
    /// Самостоятельная инициализация
    /// </summary>
    public void Awake()
    {
        LoadDifficultySettings();
    }

    /// <summary>
    /// Обновляет UI.
    /// </summary>
    public void Update()
    {
        if (!isRunning)
            return;

        remainingTime += Time.deltaTime;
        timerText.text = FormatTime(remainingTime);
        UpdateColor();

        if (remainingTime >= totalTime)
            TimeUp();
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
        endMenu.SetActive(true);
        indicatorSuccessForEndMenu.StartAnimation(remainingTime);
        Debug.Log("EndMenu показан");
    }

    #endregion

    #region UI Update

    /// <summary>
    /// Обновляет цвет таймера в зависимости от ПРОШЕДШЕГО времени.
    /// </summary>
    private void UpdateColor()
    {
        // Получаем пороги для текущего уровня сложности
        (float warningStart, float dangerStart, float lastChanceStart, float totalTime) = GetThresholds();
        
        // Определяем цвет на основе прошедшего времени
        if (remainingTime < warningStart)
        {
            // Спокойное время: прошло 0-12.5%
            timerText.color = safeColor;
        }
        else if (remainingTime < dangerStart)
        {
            // Серьёзное время: прошло 12.5-25%
            timerText.color = warningColor;
        }
        else if (remainingTime < lastChanceStart)
        {
            // Критическое время: прошло 25-50%
            timerText.color = dangerColor;
        }
        else if (remainingTime < totalTime)
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

    #endregion
}