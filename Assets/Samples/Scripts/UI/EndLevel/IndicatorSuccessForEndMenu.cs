using UnityEngine;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    public class IndicatorSuccessForEndMenu : MonoBehaviour
    {
    #region Constants

    private const float HANDLE_MOVEMENT_SPEED = 2f; // Скорость движения ползунка (единиц в секунду)

    #endregion

    #region Fields

    [Header("UI References")]
    [SerializeField] private Slider resultSlider;
    [SerializeField] private Image progressFill;
    
    [Header("Colors")]
    [Tooltip("Превосходно (шустро) - зелёный")]
    [SerializeField] private Color perfectColor = new Color(0f, 0.8f, 0f);
    
    [Tooltip("Супер (быстро) - синий")]
    [SerializeField] private Color superColor = new Color(0f, 0.5f, 1f);
    
    [Tooltip("Отлично (хорошо) - жёлтый")]
    [SerializeField] private Color excellentColor = new Color(1f, 0.8f, 0f);
    
    [Tooltip("Вовремя (нормально) - красный")]
    [SerializeField] private Color onTimeColor = new Color(1f, 0.3f, 0f);
    
    [Tooltip("Провал (опоздал) - чёрный")]
    [SerializeField] private Color failColor = Color.black;
    
    [Header("Settings")]
    [Tooltip("Текущая сложность")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;

    #endregion

    #region Private State

    private float resultTime;
    private float startTime;
    private float elapsedTime;
    private float levelTimeLimit;
    private bool isRunning;
    private bool isFinished;
    private float currentProgress;

    #endregion

    #region Properties

    public bool IsRunning => isRunning;
    public bool IsFinished => isFinished;
    public float ElapsedTime => elapsedTime;
    public DifficultyLevel CurrentDifficulty => currentDifficulty;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Автоматически получаем компоненты, если не назначены
        if (resultSlider == null)
            resultSlider = GetComponent<Slider>();
        
        if (progressFill == null && resultSlider != null)
        {
            Transform fillArea = resultSlider.transform.Find("Sliding Area");
            if (fillArea != null)
                progressFill = fillArea.GetComponent<Image>();
        }
    }

    private void Update()
    {
        if (!isRunning || isFinished)
            return;

        UpdateProgress();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Запускает анимацию индикатора.
    /// </summary>
    public void StartAnimation(float _resultTime)
    {
        LoadDifficultySettings();
        
        resultTime = _resultTime;
        startTime = Time.time;
        elapsedTime = 0f;
        isRunning = true;
        isFinished = false;
        currentProgress = 0f;

        ResetUI();
    }

    /// <summary>
    /// Завершает анимацию и фиксирует результат.
    /// Вызывается при достижении финиша.
    /// </summary>
    public void FinishRun()
    {
        if (!isRunning || isFinished)
            return;

        isFinished = true;
        isRunning = false;
        
        elapsedTime = Time.time - startTime;
        
        // Устанавливаем финальный прогресс (1.0 = 100%)
        currentProgress = 1f;
        UpdateSliderUI();
        
        // Устанавливаем цвет в зависимости от времени
        Color resultColor = GetColorForElapsedTime();
        UpdateIndicatorColor(resultColor);
        
        Debug.Log($"🏆 Результат: Время {elapsedTime:F2}с | Цвет: {ColorToString(resultColor)}");
    }

    /// <summary>
    /// Сбрасывает индикатор в начальное состояние.
    /// </summary>
    public void ResetIndicator()
    {
        isRunning = false;
        isFinished = false;
        currentProgress = 0f;
        elapsedTime = 0f;
        ResetUI();
    }

    #endregion

    #region Progress Logic

    private void UpdateProgress()
    {
        elapsedTime = Time.time - startTime;
        
        float timeRatio = resultTime / levelTimeLimit;
            if (timeRatio < 0.25f)
               timeRatio = timeRatio * 2;
            else if (timeRatio < 0.5f)
                timeRatio = timeRatio + 0.25f;
            else 
                timeRatio = timeRatio / 2 + 0.5f;

        // Двигаем ползунок слева направо
        currentProgress = Mathf.Clamp(elapsedTime / 2f, 0f, timeRatio);
        
        UpdateSliderUI();
        
        // Проверяем, достигли ли финиша
        if (currentProgress >= 1f)
        {
            FinishRun();
        }
    }

    private void UpdateSliderUI()
    {
        if (resultSlider == null)
            return;

        // Устанавливаем значение скроллбара (0 = слева, 1 = справа)
        resultSlider.value = currentProgress;
        
        // Обновляем цвет фона трека (след)
        if (progressFill != null)
        {
            // Если финиш достигнут — цвет зависит от результата, иначе — серый
            if (isFinished)
            {
                progressFill.color = GetColorForElapsedTime();
            }
            else
            {
                progressFill.color = new Color(0.3f, 0.3f, 0.3f);
            }
        }
    }

    #endregion

    #region Color Logic

    private Color GetColorForElapsedTime()
    {
        if (levelTimeLimit <= 0f)
            return failColor;

        float timeRatio = elapsedTime / levelTimeLimit;
        
        // Чем меньше ratio, тем лучше (быстрее прошли)
        if (timeRatio <= 0.25f)
        {
            return perfectColor; // Превосходно
        }
        else if (timeRatio <= 0.5f)
        {
            return superColor; // Супер
        }
        else if (timeRatio <= 0.75f)
        {
            return excellentColor; // Отлично
        }
        else if (timeRatio <= 1f)
        {
            return onTimeColor; // Вовремя
        }
        else
        {
            return failColor; // Провал
        }
    }

    private void UpdateIndicatorColor(Color color)
    {
        // Применяем цвет ко всем элементам UI
        if (progressFill != null)
            progressFill.color = color;
        
        if (progressFill != null)
            progressFill.color = color;
        
        if (resultSlider != null)
            resultSlider.colors = SetColorBlock(color);
    }

    private ColorBlock SetColorBlock(Color color)
    {
        ColorBlock colors = resultSlider.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.pressedColor = color;
        colors.disabledColor = color;
        return colors;
    }

    #endregion

    #region Difficulty Settings

    private void LoadDifficultySettings()
    {
        SettingDifficulty difficultyManager = SettingDifficulty.Instance;
        
        if (difficultyManager != null)
        {
            // Получаем текущую сложность из менеджера (если не назначена вручную)
            if (currentDifficulty == DifficultyLevel.Beginner && 
                difficultyManager.CurrentDifficulty != DifficultyLevel.Beginner)
            {
                currentDifficulty = difficultyManager.CurrentDifficulty;
            }
            
            // Получаем лимит времени для текущей сложности
            HourPeak.Settings.DifficultySettings settings = difficultyManager.GetCurrentSettings();
            levelTimeLimit = settings.TimeLimit;
            
            Debug.Log($"📋 Сложность: {GetDifficultyName()} | Лимит времени: {levelTimeLimit:F1}с");
        }
        else
        {
            // Fallback: используем дефолтные значения
            SetDefaultTimeLimits();
            Debug.LogWarning($"⚠️ SettingDifficulty не найден! Используем дефолтные значения: {levelTimeLimit:F1}с");
        }
    }

    private void SetDefaultTimeLimits()
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

    #region Utilities

    private void ResetUI()
    {
        if (resultSlider != null)
        {
            resultSlider.value = 0f;
            resultSlider.colors = ColorBlock.defaultColorBlock;
        }
        
        if (progressFill != null)
        {
            progressFill.color = new Color(0.3f, 0.3f, 0.3f);
        }
        
        if (progressFill != null)
        {
            progressFill.color = Color.white;
        }
    }

    private static string ColorToString(Color color)
    {
        return $"R:{color.r:F2} G:{color.g:F2} B:{color.b:F2}";
    }

    #endregion
    }
}