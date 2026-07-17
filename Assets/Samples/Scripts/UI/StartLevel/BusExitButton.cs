using UnityEngine;
using UnityEngine.UI;
using HourPeak.Addition;

/// <summary>
/// Кнопка выхода из автобуса.
/// Автоматически отображается только когда:
/// - Игрок находится внутри автобуса
/// - Автобус остановился на автобусной остановке
/// </summary>
public class BusExitButton : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Интервал проверки видимости (в секундах).
    /// </summary>
    private const float VISIBILITY_CHECK_INTERVAL = 0.1f;

    #endregion

    #region Fields

    [Header("UI References")]
    [Tooltip("Панель кнопки выхода (отображается)")]
    [SerializeField] private GameObject exitButtonPanel;
    
    [Tooltip("Панель скрытой кнопки (опционально)")]
    [SerializeField] private GameObject hiddenButtonPanel;

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    [Header("Settings")]
    [Tooltip("Включить автопроверку видимости")]
    [SerializeField] private bool autoCheckVisibility = true;

    #endregion

    #region Private State

    private bool playerControllerResolved;
    private bool isButtonVisible;
    private float nextCheckTime;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ BusExitButton: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ BusExitButton: активен на {GetCurrentPlatformName()}");
        ResolveReferences();
    }

    private void Start()
    {
        InitializeButton();
    }

    private void Update()
    {
        if (!IsMobilePlatform())
        {
            enabled = false;
            return;
        }

        UpdateReferences();

        if (autoCheckVisibility && Time.time >= nextCheckTime)
        {
            UpdateButtonVisibility();
            nextCheckTime = Time.time + VISIBILITY_CHECK_INTERVAL;
        }
    }

    private void OnDestroy()
    {
        // Очистка при уничтожении
        SetButtonVisible(false);
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
        Debug.Log("🔄 BusExitButton: ссылки сброшены");
    }

    /// <summary>
    /// Принудительно показывает кнопку выхода.
    /// </summary>
    public void ShowButton()
    {
        SetButtonVisible(true);
    }

    /// <summary>
    /// Принудительно скрывает кнопку выхода.
    /// </summary>
    public void HideButton()
    {
        SetButtonVisible(false);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки выхода.
    /// </summary>
    public void OnExitButtonClick()
    {
        if (playerController == null)
        {
            Debug.LogWarning("⚠️ PlayerController не найден!");
            return;
        }

        if (!playerController.IsOnBus())
        {
            Debug.LogWarning("⚠️ Игрок не находится в автобусе!");
            return;
        }

        Debug.Log("🚪 Запрос выхода из автобуса...");
        playerController.ExitFromBus();
        SetButtonVisible(false);
    }

    #endregion

    #region Reference Resolution

    /// <summary>
    /// Находит все необходимые ссылки.
    /// </summary>
    private void ResolveReferences()
    {
        ResolvePlayerController();
    }

    /// <summary>
    /// Находит PlayerController.
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

        Debug.LogWarning("⚠️ PlayerController не найден!");
    }

    /// <summary>
    /// Обновляет ссылки если они ещё не найдены.
    /// </summary>
    private void UpdateReferences()
    {
        if (!playerControllerResolved)
            ResolvePlayerController();
    }

    #endregion

    #region Button Visibility

    /// <summary>
    /// Инициализирует состояние кнопки.
    /// </summary>
    private void InitializeButton()
    {
        SetButtonVisible(false);
        nextCheckTime = Time.time + VISIBILITY_CHECK_INTERVAL;
        Debug.Log("✅ Кнопка выхода инициализирована (скрыта)");
    }

    /// <summary>
    /// Обновляет видимость кнопки на основе состояния игрока и автобуса.
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (playerController == null)
        {
            SetButtonVisible(false);
            return;
        }

        // Проверяем условие отображения через PlayerController
        bool shouldBeVisible = playerController.ShouldShowBusExitButton;
        
        if (shouldBeVisible != isButtonVisible)
        {
            SetButtonVisible(shouldBeVisible);
            
            if (shouldBeVisible)
                Debug.Log("🚪 Кнопка выхода отображена (автобус на остановке, игрок внутри)");
            else
                Debug.Log("🚪 Кнопка выхода скрыта");
        }
    }

    /// <summary>
    /// Устанавливает видимость кнопки.
    /// </summary>
    private void SetButtonVisible(bool visible)
    {
        if (isButtonVisible == visible)
            return;

        isButtonVisible = visible;

        if (exitButtonPanel != null)
            exitButtonPanel.SetActive(visible);

        if (hiddenButtonPanel != null && hiddenButtonPanel != exitButtonPanel)
            hiddenButtonPanel.SetActive(!visible);
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
