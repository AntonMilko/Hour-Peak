using UnityEngine;
using UnityEngine.UI;
using HourPeak.Addition;

/// <summary>
/// Управление бегом персонажа через UI кнопку на экране.
/// Читает ввод с UI кнопки и вызывает RequestRun() в PlayerController.
/// Поддерживает переключение на Shift для отладки в Editor.
/// </summary>
public class Running : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Минимальная величина ввода для считания активным.
    /// </summary>
    private const float INPUT_THRESHOLD = 0.01f;

    #endregion

    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    [Header("Run Button")]
    [Tooltip("UI кнопка для бега (опционально)")]
    [SerializeField] private Button runButton;
    
    [Tooltip("Включить переключение на Shift, если кнопка не найдена")]
    [SerializeField] private bool useKeyboardFallback = true;

    [Header("Button Settings")]
    [Tooltip("Цвет кнопки при нажатии")]
    [SerializeField] private Color pressedColor = new Color(1f, 0.8f, 0f);
    
    [Tooltip("Цвет кнопки в обычном состоянии")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.5f);

    #endregion

    #region Private State

    private bool playerControllerResolved;
    private bool isRunButtonPressed;
    private Color originalColor;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ Running: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ Running: активен на {GetCurrentPlatformName()}");
        ResolvePlayerController();
    }

    private void Start()
    {
        SubscribeToRunButton();
        SaveButtonColor();
    }

    private void Update()
    {
        if (!IsMobilePlatform())
        {
            enabled = false;
            return;
        }

        UpdateReferences();
        ReadInput();
        UpdateButtonVisuals();
    }

    private void OnDestroy()
    {
        UnsubscribeFromRunButton();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Принудительно сбрасывает все кэшированные ссылки.
    /// </summary>
    public void ResetReferences()
    {
        playerController = null;
        playerControllerResolved = false;
        Debug.Log("🔄 Running: ссылки сброшены");
    }

    /// <summary>
    /// Вызывается при нажатии UI кнопки бега.
    /// </summary>
    public void OnRunButtonPressed()
    {
        isRunButtonPressed = true;
    }

    /// <summary>
    /// Вызывается при отпускании UI кнопки бега.
    /// </summary>
    public void OnRunButtonReleased()
    {
        isRunButtonPressed = false;
    }

    #endregion

    #region Reference Resolution

    /// <summary>
    /// Находит PlayerController при старте.
    /// </summary>
    private void ResolvePlayerController()
    {
        if (playerController != null)
        {
            playerControllerResolved = true;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerControllerResolved = true;
                Debug.Log("✅ PlayerController найден по тегу 'Player'");
                return;
            }
        }

        Debug.LogWarning("⚠️ PlayerController не найден! Будет найден при первом вводе.");
    }

    /// <summary>
    /// Обновляет ссылки если они ещё не найдены.
    /// </summary>
    private void UpdateReferences()
    {
        if (!playerControllerResolved)
            ResolvePlayerController();
    }

    /// <summary>
    /// Подписывается на события кнопки бега.
    /// </summary>
    private void SubscribeToRunButton()
    {
        if (runButton != null)
        {
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(OnRunButtonPressed);
            Debug.Log("✅ Кнопка бега подключена");
        }
    }

    /// <summary>
    /// Отписывается от событий кнопки бега.
    /// </summary>
    private void UnsubscribeFromRunButton()
    {
        if (runButton != null)
        {
            runButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Сохраняет оригинальный цвет кнопки.
    /// </summary>
    private void SaveButtonColor()
    {
        if (runButton != null)
        {
            var colors = runButton.colors;
            originalColor = colors.normalColor;
            Debug.Log($"✅ Оригинальный цвет кнопки сохранён");
        }
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Читает ввод для бега.
    /// </summary>
    private void ReadInput()
    {
        // Проверяем клавиатуру (для отладки)
        if (useKeyboardFallback && Input.GetKey(KeyCode.LeftShift))
        {
            isRunButtonPressed = true;
        }
        else if (useKeyboardFallback && Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunButtonPressed = false;
        }
    }

    /// <summary>
    /// Обновляет визуальное состояние кнопки.
    /// </summary>
    private void UpdateButtonVisuals()
    {
        if (runButton == null)
            return;

        var colors = runButton.colors;
        
        if (isRunButtonPressed)
        {
            colors.normalColor = pressedColor;
        }
        else
        {
            colors.normalColor = normalColor;
        }
        
        runButton.colors = colors;
    }

    #endregion

    #region Platform Check

    /// <summary>
    /// Проверяет, является ли текущая платформа мобильной.
    /// </summary>
    private bool IsMobilePlatform()
    {
        #if UNITY_IOS || UNITY_ANDROID
            return Application.isMobilePlatform;
        #else
            return false;
        #endif
    }

    /// <summary>
    /// Получает имя текущей платформы для отладки.
    /// </summary>
    private string GetCurrentPlatformName()
    {
        #if UNITY_IOS
            return "iOS";
        #elif UNITY_ANDROID
            return "Android";
        #elif UNITY_EDITOR
            return "Editor";
        #else
            return Application.platform.ToString();
        #endif
    }

    #endregion
}
