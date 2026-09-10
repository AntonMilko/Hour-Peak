using UnityEngine;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    /// <summary>
    /// Индикатор успеха для финального меню.
    /// Отображает результат прохождения части уровня с цветовой индикацией в зависимости от времени.
    /// Ползунок двигается слева направо, оставляя за собой цветной след.
    /// Цвет результата зависит от того, за сколько времени был пройден уровень.
    /// </summary>
    [RequireComponent(typeof(Scrollbar))]
    public class IndicatorSuccessForEndMenu : MonoBehaviour
    {
    #region Constants

    private const float HANDLE_MOVEMENT_SPEED = 2f; // Скорость движения ползунка (единиц в секунду)

    #endregion

    #region Fields

    [Header("UI References")]
    [SerializeField] private Scrollbar resultScrollbar;
    [SerializeField] private Image handleImage;
    [SerializeField] private Image trackBackground;
    
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
    [Tooltip("Автоматически запускать анимацию при старте")]
    [SerializeField] private bool autoStart = true;
    
    [Tooltip("Текущая сложность")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
    
    [Tooltip("Целевое количество индикаторов")]
    [SerializeField] private int targetIndicators = 5;

    #endregion

    #region Private State

    private float startTime;
    private float elapsedTime;
    private float levelTimeLimit;
    private bool isRunning;
    private bool isFinished;
    private float currentProgress;
    private GameObject[] createdIndicators;

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
        if (resultScrollbar == null)
            resultScrollbar = GetComponent<Scrollbar>();
        
        if (trackBackground == null && resultScrollbar != null)
        {
            Transform fillArea = resultScrollbar.transform.Find("Sliding Area");
            if (fillArea != null)
                trackBackground = fillArea.GetComponent<Image>();
        }
    }
        
    private void Start()
    {
        if (autoStart)
        {
            StartAnimation();
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
    public void StartAnimation()
    {
        LoadDifficultySettings();
        
        startTime = Time.time;
        elapsedTime = 0f;
        isRunning = true;
        isFinished = false;
        currentProgress = 0f;

        ResetUI();
        
        Debug.Log($"🏁 Запуск индикатора: Сложность: {GetDifficultyName()} | Лимит: {levelTimeLimit:F1}с | Индикаторов: {targetIndicators}");
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
        UpdateScrollbarUI();
        
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
        
        // Двигаем ползунок слева направо
        currentProgress = Mathf.Clamp01(elapsedTime / levelTimeLimit);
        
        UpdateScrollbarUI();
        
        // Проверяем, достигли ли финиша
        if (currentProgress >= 1f)
        {
            FinishRun();
        }
    }

    private void UpdateScrollbarUI()
    {
        if (resultScrollbar == null)
            return;

        // Устанавливаем значение скроллбара (0 = слева, 1 = справа)
        resultScrollbar.value = currentProgress;
        
        // Обновляем цвет фона трека (след)
        if (trackBackground != null)
        {
            // Если финиш достигнут — цвет зависит от результата, иначе — серый
            if (isFinished)
            {
                trackBackground.color = GetColorForElapsedTime();
            }
            else
            {
                trackBackground.color = new Color(0.3f, 0.3f, 0.3f);
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
        if (handleImage != null)
            handleImage.color = color;
        
        if (trackBackground != null)
            trackBackground.color = color;
        
        if (resultScrollbar != null)
            resultScrollbar.colors = SetColorBlock(color);
    }

    private ColorBlock SetColorBlock(Color color)
    {
        ColorBlock colors = resultScrollbar.colors;
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
        if (resultScrollbar != null)
        {
            resultScrollbar.value = 0f;
            resultScrollbar.colors = ColorBlock.defaultColorBlock;
        }
        
        if (trackBackground != null)
        {
            trackBackground.color = new Color(0.3f, 0.3f, 0.3f);
        }
        
        if (handleImage != null)
        {
            handleImage.color = Color.white;
        }
    }

    private static string ColorToString(Color color)
    {
        return $"R:{color.r:F2} G:{color.g:F2} B:{color.b:F2}";
    }

    #endregion
    }
}