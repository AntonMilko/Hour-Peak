using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using HourPeak.Settings;

/// <summary>
/// Индикатор успеха для игрового процесса.
/// Отображает прогресс сбора индикаторов с цветовой индикацией в зависимости от времени.
/// Ползунок двигается слева направо, оставляя за собой чёрный след.
/// </summary>
public class IndicatorSuccessForGameplay : MonoBehaviour
{
    #region Constants

    private const string TIME_FORMAT = "{0:00}:{1:00}";

    #endregion

    #region Fields

    [Header("UI References")]
    [SerializeField] private Image progressFill;
    [SerializeField] private Slider sliderFill;
    [SerializeField] private TMP_Text timerText;

    [Header("Colors")]
    [Tooltip("Превосходно (шустро) - зелёный")]
    [SerializeField] private Color perfectColor = new Color(0f, 0.8f, 0f);
    
    [Tooltip("Супер (быстро) - синий")]
    [SerializeField] private Color superColor = new Color(0f, 0.5f, 1f);
    
    [Tooltip("Отлично (хорошо) - жёлтый")]
    [SerializeField] private Color excellentColor = new Color(1f, 0.8f, 0f);
    
    [Tooltip("Вовремя (нормально) - красный")]
    [SerializeField] private Color onTimeColor = new Color(1f, 0f, 0f);
    
    [Tooltip("Провал (опоздал) - чёрный")]
    [SerializeField] private Color failColor = Color.black;

    [Header("Settings")]
    [Tooltip("Текущий уровень сложности")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;

    [Tooltip("Автозапуск таймера")]
    [SerializeField] private bool autoStart = true;
    
    [Tooltip("Проверка сцены")]
    [SerializeField] private bool isFirstScene;

    #endregion

    #region Private State

    private float currentLevelTime = 0f;
    private float levelTimeLimit;
    private bool isLevelActive;
    private bool isRunning;

    #endregion

    #region Unity Lifecycle

    public bool isTimeUp()
    {
        return currentLevelTime >= levelTimeLimit;
    }

    public void SetIsRunning(bool isRunning)
    {
        this.isRunning = isRunning;
    }

    private void Awake()
    {
        if (autoStart)
        {
            StartLevel();
        }

        isRunning = true;
        if (isFirstScene)
        {
            currentLevelTime = 0f;
        }
    }

    private void Update()
    {
        if (!isLevelActive)
            return;

        if (!isRunning)
            return;

        UpdateLevel();
        currentLevelTime += Time.deltaTime;
        timerText.text = FormatTime(currentLevelTime);
        UpdateUI();

        if (currentLevelTime >= levelTimeLimit)
            TimeUp();
    }

    #endregion

    #region Public Methods

    public void StartLevel()
    {
        LoadDifficultySettings();

        currentLevelTime = 0f;
        isLevelActive = true;

        UpdateUI();
    }

    public void ResetLevel()
    {
        StartLevel();
    }

    #endregion

    #region Level Logic

    private void LoadDifficultySettings()
    {
        SettingDifficulty difficultyManager = SettingDifficulty.Instance;
        
        if (difficultyManager != null)
        {
            DifficultySettings settings = difficultyManager.GetSettingsByLevel(currentDifficulty);
            levelTimeLimit = settings.TimeLimit;
            Debug.Log($"📋 Настройки сложности: {GetDifficultyName()} | Время: {FormatTime(levelTimeLimit)}");
        }
        else
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Beginner:
                    levelTimeLimit = 240f;
                    break;
                case DifficultyLevel.Professional:
                    levelTimeLimit = 120f;
                    break;
                case DifficultyLevel.Extremal:
                    levelTimeLimit = 60f;
                    break;
                default:
                    levelTimeLimit = 240f;
                    break;
            }
            
            Debug.LogWarning($"⚠️ SettingDifficulty не найден! Используем: {levelTimeLimit}с");
        }
    }

    private void UpdateLevel()
    {
        if (!isRunning)
            return;

        currentLevelTime += Time.deltaTime;
        UpdateUI();
        
        if (currentLevelTime >= levelTimeLimit)
        {
            TimeUp();
        }
    }

    private void TimeUp()
    {
        isLevelActive = false;
        Debug.Log("⏰ Время вышло! Провал.");
    }

    private void LevelCompleted()
    {
        isLevelActive = false;
        
        float timeRatio = currentLevelTime / levelTimeLimit;
        Color resultColor = GetColorForTimeRatio(timeRatio);
        
        Debug.Log($"🎉 Уровень пройден! Время: {FormatTime(currentLevelTime)} | Цвет: {resultColor}");
    }

    #endregion

    #region UI Update

    private void UpdateUI()
    {
        if (progressFill != null)
        {
            float timeRatio = currentLevelTime / levelTimeLimit;
            if (timeRatio < 0.25f)
               timeRatio = timeRatio * 2;
            else if (timeRatio < 0.5f)
                timeRatio = timeRatio + 0.25f;
            else 
                timeRatio = timeRatio / 2 + 0.5f;
            sliderFill.value = 1 - timeRatio;
            progressFill.color = GetColorForTimeRatio(timeRatio);
        }
    }

    private Color GetColorForTimeRatio(float timeRatio)
    {
        if (timeRatio < 0.25f)
        {
            return perfectColor;
        }
        else if (timeRatio < 0.5f)
        {
            return superColor;
        }
        else if (timeRatio < 0.75f)
        {
            return excellentColor;
        }
        else if (timeRatio < 1f)
        {
            return onTimeColor;
        }
        else
        {
            return failColor;
        }
    }

    #endregion

    #region Utilities

    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format(TIME_FORMAT, minutes, seconds);
    }

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