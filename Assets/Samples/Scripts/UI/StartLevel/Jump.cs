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
    
    [Header("Jump Settings")]
    [Tooltip("Скорость падения (м/с)")]
    [SerializeField] private float fallSpeed = 10f;
    
    [Tooltip("Ссылка на Button (автоматически найдётся по тегу Player)")]
    [SerializeField] private Button jumpButtonReference;

    [Tooltip("Множитель силы прыжка при удержании кнопки")]
    [SerializeField] private float holdJumpMultiplier = 1.5f;

    #endregion

    #region Private State

    private bool playerControllerResolved;
    private bool isJumpButtonPressed;
    private float jumpHoldTimer;
    private bool isGrounded;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        FindJumpButton();
    }

    private void Update()
    {
        if (!isGrounded) 
        {
            if (TryGetComponent<Rigidbody>(out Rigidbody rigidbodyComponent))
            {
                rigidbodyComponent.linearVelocity = new Vector3(rigidbodyComponent.linearVelocity.x, -fallSpeed, rigidbodyComponent.linearVelocity.z);
            }
        }
    }

    private void FindJumpButton()
    {
        // Если ссылка уже перетащена вручную в инспекторе, ничего не делаем
        if (jumpButtonReference != null) return;

        // Находим объект с тегом Player
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            // Ищем компонент Button в дочерних объектах игрока (или на самом Canvas, если скрипт висит там)
            jumpButtonReference = playerObj.GetComponentInChildren<Button>();

            // Если кнопка не внутри игрока, а лежит отдельно на Canvas:
            if (jumpButtonReference == null)
            {
                // Ищем любую кнопку на сцене с именем, содержащим "Jump" (альтернативный безопасный вариант)
                Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
                foreach (Button btn in allButtons)
                {
                    if (btn.gameObject.name.ToLower().Contains("jump"))
                    {
                        jumpButtonReference = btn;
                        break;
                    }
                }
            }

            // Подписываемся на событие нажатия кнопки, если она успешно найдена
            if (jumpButtonReference != null)
            {
                jumpButtonReference.onClick.AddListener(OnJumpButtonPressed);
                Debug.Log($"[Jump.cs] Кнопка прыжка '{jumpButtonReference.gameObject.name}' успешно найдена и привязана!");
            }
            else
            {
                Debug.LogWarning("[Jump.cs] Объект Player найден, но у него или на UI не обнаружена Button прыжка.");
            }
        }
        else
        {
            Debug.LogError("[Jump.cs] Не удалось найти объект с тегом 'Player'! Проверьте теги в Unity.");
        }
    }

    private void OnJumpButtonPressed()
    {
        Debug.Log("Кнопка прыжка нажата! Выполняем прыжок...");
        // Вставьте сюда вашу логику прыжка (например, rigidbody.AddForce или characterController.Move)
    }

    private void ApplyFallSpeed()
    {
        // Использование переменной fallSpeed (например, симуляция кастомной гравитации)
        // transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    // Отписываемся от события при уничтожении объекта, чтобы избежать утечек памяти
    private void OnDestroy()
    {
        if (jumpButtonReference != null)
        {
            jumpButtonReference.onClick.RemoveListener(OnJumpButtonPressed);
        }
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