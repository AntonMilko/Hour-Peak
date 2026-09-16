using UnityEngine;
using HourPeak.Addition;

/// <summary>
/// Управление вращением камеры через джойстик на экране.
/// Читает ввод с UI джойстика и поворачивает камеру персонажа.
/// Поддерживает переключение на мышь для отладки в Editor.
/// </summary>
public class LookJoystick : MonoBehaviour
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
    [Tooltip("Ссылка на камеру (автоматически найдётся через PlayerController)")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    [Header("Rotation Settings")]
    [Tooltip("Скорость вращения камеры по горизонтали (градусов в секунду)")]
    [SerializeField] private float horizontalSensitivity = 120f;
    
    [Tooltip("Скорость вращения камеры по вертикали (градусов в секунду)")]
    [SerializeField] private float verticalSensitivity = 120f;
    
    [Tooltip("Минимальный угол наклона камеры (вверх)")]
    [SerializeField] private float minLookAngle = -45f;
    
    [Tooltip("Максимальный угол наклона камеры (вниз)")]
    [SerializeField] private float maxLookAngle = 45f;

    [Header("Rotation Speed")]
    [Tooltip("Скорость вращения камеры (градусов в секунду)")]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Ссылка на Joystick (автоматически найдётся по тегу Player)")]
    [SerializeField] private Joystick virtualJoystick;

    #endregion

    #region Private State

    private ClickTracker lookStick;
    private bool playerControllerResolved;
    private bool lookStickResolved;
    private bool cameraResolved;
    
    private float currentVerticalAngle;
    private Vector2 lookInput;
    private Vector2 inputDirection;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        ResolvePlayerController();
        ResolveCamera();
    }

    private void Start()
    {
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
                
                if (playerController == null)
                {
                    Debug.LogError("[LookJoystick] Объект с тегом 'Player' найден, но на нем нет скрипта PlayerController!");
                }
            }
            else
            {
                Debug.LogError("[LookJoystick] Не удалось найти на сцене объект с тегом 'Player'!");
            }
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;

            if (playerCamera == null)
            {
                Debug.LogError("[LookJoystick] На сцене не найдена камера с тегом 'MainCamera'!");
            }
        }

        InitializeLookAngle();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        ApplyInput();
    }

    private void ReadInput()
    {
        inputDirection = virtualJoystick.Direction; 
    }

    private void ApplyInput()
    {
        // Поварачиваем камеру на основе считанных данных
        Vector3 look = new Vector3(inputDirection.x, 0, inputDirection.y) * rotationSpeed * Time.fixedDeltaTime;
        transform.Translate(look);
    }

    private void ResolveLookJoystick()
    {
        if (lookStickResolved) 
        {
            Debug.Log("Джойстик взгляда успешно инициализирован!");
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
        lookStick = null;
        lookStickResolved = false;
        playerCamera = null;
        cameraResolved = false;
        Debug.Log("🔄 LookJoystick: ссылки сброшены");
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
    /// Находит камеру.
    /// </summary>
    private void ResolveCamera()
    {
        if (playerCamera != null)
        {
            cameraResolved = true;
            return;
        }

        // Способ 1: Через PlayerController
        if (playerController != null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                cameraResolved = true;
                Debug.Log("✅ Камера найдена (Camera.main)");
                return;
            }
        }

        // Способ 2: Через FindFirstObjectByType
        playerCamera = FindFirstObjectByType<Camera>();
        if (playerCamera != null)
        {
            cameraResolved = true;
            Debug.Log("✅ Камера найдена (FindFirstObjectByType)");
            return;
        }

        Debug.LogWarning("⚠️ Камера не найдена! Будет найдена при первом вводе.");
    }

    /// <summary>
    /// Обновляет ссылки если они ещё не найдены.
    /// </summary>
    private void UpdateReferences()
    {
        if (!playerControllerResolved)
            ResolvePlayerController();

        if (!cameraResolved)
            ResolveCamera();
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Инициализирует текущий угол наклона камеры.
    /// </summary>
    private void InitializeLookAngle()
    {
        if (playerCamera != null)
        {
            Vector3 euler = playerCamera.transform.localEulerAngles;
            currentVerticalAngle = Mathf.Deg2Rad * euler.x;
        }
    }

    /// <summary>
    /// Получает ввод для вращения.
    /// </summary>
    private Vector2 GetLookInput()
    {
        // Способ 1: Через ClickTracker
        if (lookStick != null)
        {
            Vector2 stickInput = lookStick.GetInputAxis();
            if (stickInput.sqrMagnitude > INPUT_THRESHOLD)
                return stickInput;
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Применяет вращение к камере.
    /// </summary>
    private void ApplyRotation()
    {
        if (playerCamera == null)
            return;

        if (lookInput.sqrMagnitude < INPUT_THRESHOLD)
            return;

        // Вычисляем изменение угла
        float horizontalDelta = lookInput.x * horizontalSensitivity * Time.deltaTime;
        float verticalDelta = lookInput.y * verticalSensitivity * Time.deltaTime;

        // Поворачиваем персонажа по горизонтали
        if (playerController != null)
        {
            transform.Rotate(0, horizontalDelta, 0);
        }

        // Поворачиваем камеру по вертикали (с ограничениями)
        currentVerticalAngle -= verticalDelta * Mathf.Deg2Rad;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minLookAngle * Mathf.Deg2Rad, maxLookAngle * Mathf.Deg2Rad);

        // Применяем вращение к камере
        Vector3 euler = playerCamera.transform.localEulerAngles;
        euler.x = currentVerticalAngle * Mathf.Rad2Deg;
        playerCamera.transform.localEulerAngles = euler;
    }

    #endregion
}