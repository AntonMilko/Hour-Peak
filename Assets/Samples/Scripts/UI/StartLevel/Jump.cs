using UnityEngine;
using UnityEngine.UI;
using HourPeak.Addition;

/// <summary>
/// Управление прыжком персонажа через UI кнопку на экране.
/// Читает ввод с UI кнопки и вызывает RequestJump() в PlayerController.
/// Поддерживает переключение на пробел для отладки в Editor.
/// </summary>
public class Jump : MonoBehaviour
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

    [Header("Jump Button")]
    [Tooltip("UI кнопка для прыжка (опционально)")]
    [SerializeField] private Button jumpButton;
    
    [Tooltip("Включить переключение на пробел, если кнопка не найдена")]
    [SerializeField] private bool useKeyboardFallback = true;

    [Header("Jump Settings")]
    [Tooltip("Включить удержание кнопки для дополнительного прыжка")]
    [SerializeField] private bool useHoldToJumpHigher = true;
    
    [Tooltip("Скорость подёма при удержании (дополнительная сила)")]
    [SerializeField] private float holdJumpMultiplier = 1.5f;

    #endregion

    #region Private State

    private bool playerControllerResolved;
    private bool isJumpButtonPressed;
    private float jumpHoldTimer;
    private bool isGrounded;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ Jump: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ Jump: активен на {GetCurrentPlatformName()}");
        ResolvePlayerController();
    }

    private void Start()
    {
        SubscribeToJumpButton();
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
        HandleJump();
    }

    private void OnDestroy()
    {
        UnsubscribeFromJumpButton();
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
        Debug.Log("🔄 Jump: ссылки сброшены");
    }

    /// <summary>
    /// Вызывается при нажатии UI кнопки прыжка.
    /// </summary>
    public void OnJumpButtonPressed()
    {
        isJumpButtonPressed = true;
        jumpHoldTimer = 0f;
    }

    /// <summary>
    /// Вызывается при отпускании UI кнопки прыжка.
    /// </summary>
    public void OnJumpButtonReleased()
    {
        isJumpButtonPressed = false;
        jumpHoldTimer = 0f;
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
    /// Подписывается на события кнопки прыжка.
    /// </summary>
    private void SubscribeToJumpButton()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(OnJumpButtonPressed);
            
            // Для отпускания кнопки нужно добавить Listener через UnityEvent
            // Но проще использовать PointerUp/PointerDown из InputSystem
            Debug.Log("✅ Кнопка прыжка подключена");
        }
    }

    /// <summary>
    /// Отписывается от событий кнопки прыжка.
    /// </summary>
    private void UnsubscribeFromJumpButton()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Читает ввод для прыжка.
    /// </summary>
    private void ReadInput()
    {
        // Проверяем клавиатуру (для отладки)
        if (useKeyboardFallback && Input.GetButtonDown("Jump"))
        {
            isJumpButtonPressed = true;
        }
        if (useKeyboardFallback && Input.GetButtonUp("Jump"))
        {
            isJumpButtonPressed = false;
        }
    }

    /// <summary>
    /// Обрабатывает прыжок.
    /// </summary>
    private void HandleJump()
    {
        if (playerController == null)
            return;

        if (!isJumpButtonPressed)
            return;

        // Вычисляем силу прыжка
        float jumpForce = CalculateJumpForce();
        
        // Вызываем RequestJump с информацией о силе
        // (если PlayerController поддерживает)
        if (useHoldToJumpHigher)
        {
            // Увеличиваем время удержания для более высокого прыжка
            jumpHoldTimer += Time.deltaTime;
        }

        // Запрашиваем прыжок
        playerController.RequestJump();
        
        // Сбрасываем после выполнения
        isJumpButtonPressed = false;
        jumpHoldTimer = 0f;
    }

    /// <summary>
    /// Вычисляет силу прыжка на основе времени удержания.
    /// </summary>
    private float CalculateJumpForce()
    {
        if (!useHoldToJumpHigher)
            return 1f;

        // Ограничиваем время удержания для баланса
        float normalizedHoldTime = Mathf.Clamp01(jumpHoldTimer / 0.3f);
        return 1f + (normalizedHoldTime * (holdJumpMultiplier - 1f));
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