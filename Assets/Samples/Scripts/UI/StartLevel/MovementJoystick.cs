using UnityEngine;
using HourPeak.Addition;

/// <summary>
/// Управление движением персонажа через джойстик на экране.
/// Читает ввод с UI джойстика и передаёт его PlayerController.
/// Поддерживает переключение на клавиатуру для отладки в Editor.
/// </summary>
public class MovementJoystick : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Минимальная величина ввода для считания активным.
    /// </summary>
    private const float INPUT_THRESHOLD = 0.01f;

    /// <summary>
    /// Максимальная величина вектора ввода.
    /// </summary>
    private const float MAX_INPUT_MAGNITUDE = 1f;

    #endregion

    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    [Header("Joystick Settings")]
    [Tooltip("Имя джойстика для поиска (должно совпадать с ClickTracker.buttonName)")]
    [SerializeField] private string joystickName = "Movement";
    
    [Tooltip("Включить переключение на клавиатуру, если джойстик не найден")]
    [SerializeField] private bool useKeyboardFallback = true;

    #endregion

    #region Private State

    private ClickTracker movementStick;
    private bool playerControllerResolved;
    private bool movementStickResolved;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ MovementJoystick: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ MovementJoystick: активен на {GetCurrentPlatformName()}");
        ResolvePlayerController();
    }

    private void Start()
    {
        ResolveMovementStick();
    }

    private void Update()
    {
        if (!IsMobilePlatform())
        {
            enabled = false;
            return;
        }
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

    #region Public Methods

    /// <summary>
    /// Принудительно сбрасывает все кэшированные ссылки.
    /// Вызывается при необходимости переподключения.
    /// </summary>
    public void ResetReferences()
    {
        playerController = null;
        playerControllerResolved = false;
        movementStick = null;
        movementStickResolved = false;
        Debug.Log("🔄 MovementJoystick: ссылки сброшены");
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

        Debug.LogWarning("⚠️ PlayerController не найден! Будет найдено при первом вводе.");
    }

    /// <summary>
    /// Находит джойстик движения среди всех ClickTracker.
    /// </summary>
    private void ResolveMovementStick()
    {
        if (movementStick != null)
        {
            movementStickResolved = true;
            return;
        }

        ClickTracker[] allTrackers = FindObjectsByType<ClickTracker>(FindObjectsSortMode.None);
        
        for (int i = 0; i < allTrackers.Length; i++)
        {
            ClickTracker tracker = allTrackers[i];
            if (tracker.isJoystick && tracker.buttonName == joystickName)
            {
                movementStick = tracker;
                movementStickResolved = true;
                Debug.Log($"✅ Джойстик '{joystickName}' найден: {tracker.gameObject.name}");
                return;
            }
        }

        Debug.LogWarning($"⚠️ Джойстик '{joystickName}' не найден! Будет использоваться клавиатура.");
    }

    /// <summary>
    /// Обновляет ссылки если они ещё не найдены.
    /// </summary>
    private void UpdateReferences()
    {
        if (!playerControllerResolved)
            ResolvePlayerController();

        if (!movementStickResolved)
            ResolveMovementStick();
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Отправляет ввод в PlayerController.
    /// </summary>
    private void SendInputToPlayer()
    {
        if (playerController == null)
            return;

        Vector2 input = ReadInput();
        playerController.JoystickMoveInput = input;
    }

    /// <summary>
    /// Читает ввод из джойстика или клавиатуры.
    /// </summary>
    private Vector2 ReadInput()
    {
        Vector2 input = GetJoystickInput();
        
        // Если джойстик не активен и включён fallback на клавиатуру
        if (useKeyboardFallback && input.sqrMagnitude < INPUT_THRESHOLD)
        {
            input = GetKeyboardInput();
        }

        // Ограничиваем величину вектора
        return Vector2.ClampMagnitude(input, MAX_INPUT_MAGNITUDE);
    }

    /// <summary>
    /// Получает ввод с джойстика.
    /// </summary>
    private Vector2 GetJoystickInput()
    {
        // Способ 1: Через ClickTracker
        if (movementStick != null)
        {
            Vector2 stickInput = movementStick.GetInputAxis();
            if (stickInput.sqrMagnitude > INPUT_THRESHOLD)
                return stickInput;
        }

        // Способ 2: Через MobileControls (если есть)
        if (MobileControls.instance != null)
        {
            Vector2 mobileInput = MobileControls.instance.GetJoystick(joystickName);
            if (mobileInput.sqrMagnitude > INPUT_THRESHOLD)
                return mobileInput;
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Получает ввод с клавиатуры (WASD / Стрелки).
    /// </summary>
    private Vector2 GetKeyboardInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical);
    }

    #endregion
}
