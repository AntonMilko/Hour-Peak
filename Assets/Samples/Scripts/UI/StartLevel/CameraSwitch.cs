using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Переключение между камерой персонажа и картой.
/// При нажатии кнопки карты скрывается камера персонажа и отображается карта.
/// </summary>
public class CameraSwitch : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Ключ для сохранения состояния карты.
    /// </summary>
    private const string MAP_OPEN_KEY = "IsMapOpen";

    #endregion

    #region Fields

    [Header("Cameras")]
    [Tooltip("Камера персонажа (основная)")]
    [SerializeField] private Camera playerCamera;
    
    [Tooltip("Камера карты")]
    [SerializeField] private Camera mapCamera;

    [Header("UI")]
    [Tooltip("Кнопка открытия карты")]
    [SerializeField] private GameObject mapButton;
    
    [Tooltip("Панель карты")]
    [SerializeField] private GameObject mapPanel;
    
    [Tooltip("Кнопка закрытия карты")]
    [SerializeField] private GameObject closeButton;

    [Header("Settings")]
    [Tooltip("Высота камеры карты")]
    [SerializeField] private float cameraHeight = 50f;

    [Tooltip("Включить авто-создание UI")]
    [SerializeField] private bool autoCreateUI = true;
    
    [Tooltip("Открывать карту при старте")]
    [SerializeField] private bool startWithMapOpen = false;
    
    [Tooltip("Закрывать карту при начале движения")]
    [SerializeField] private bool closeMapOnMove = true;

    #endregion

    #region Private State

    private bool isMapOpen;
    private Button mapButtonComponent;
    private Button closeButtonComponent;
    private Camera mainCamera;

    #endregion

    #region Properties

    /// <summary>
    /// Текущее состояние карты (открыта/закрыта).
    /// </summary>
    public bool IsMapOpen => isMapOpen;

    /// <summary>
    /// Основная камера.
    /// </summary>
    public Camera MainCamera => mainCamera;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Находим основные камеры
        ResolveCameras();
        
        if (autoCreateUI)
        {
            AutoCreateUI();
        }

        // Восстанавливаем состояние из сохранения
        LoadMapState();
    }

    private void Start()
    {
        SubscribeToButtons();
        
        if (startWithMapOpen && !isMapOpen)
        {
            OpenMap();
        }
    }

    private void Update()
    {
        // Закрываем карту при движении
        if (closeMapOnMove && isMapOpen)
        {
            Vector2 input = GetMovementInput();
            if (input.sqrMagnitude > 0.01f)
            {
                CloseMap();
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromButtons();
        SaveMapState();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Переключает состояние карты (открыть/закрыть).
    /// Вызывается из UI кнопки.
    /// </summary>
    public void ToggleMap()
    {
        if (isMapOpen)
        {
            CloseMap();
        }
        else
        {
            OpenMap();
        }
    }

    /// <summary>
    /// Открывает карту.
    /// </summary>
    public void OpenMap()
    {
        if (isMapOpen)
            return;

        Debug.Log("🗺️ Карта открыта");

        // 1. Отключаем камеру персонажа
        if (playerCamera != null)
            playerCamera.enabled = false;

        // 2. Включаем камеру карты
        if (mapCamera != null)
            mapCamera.enabled = true;

        // 3. Показываем панель карты
        if (mapPanel != null)
            mapPanel.SetActive(true);

        // 4. Показываем кнопку закрытия
        if (closeButton != null)
            closeButton.SetActive(true);

        // 5. Скрываем кнопку карты
        if (mapButton != null)
            mapButton.SetActive(false);

        // 6. Останавливаем время (игра на паузе)
        Time.timeScale = 0f;

        // Устанавливаем камеру в фиксированную позицию
        if (mapCamera != null)
        {
            mapCamera.transform.position = new Vector3(0, cameraHeight, 0);
            mapCamera.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }

        isMapOpen = true;
    }

    /// <summary>
    /// Закрывает карту.
    /// </summary>
    public void CloseMap()
    {
        if (!isMapOpen)
            return;

        Debug.Log("🔙 Карта закрыта");

        // 1. Включаем камеру персонажа
        if (playerCamera != null)
            playerCamera.enabled = true;

        // 2. Отключаем камеру карты
        if (mapCamera != null)
            mapCamera.enabled = false;

        // 3. Скрываем панель карты
        if (mapPanel != null)
            mapPanel.SetActive(false);

        // 4. Скрываем кнопку закрытия
        if (closeButton != null)
            closeButton.SetActive(false);

        // 5. Показываем кнопку карты
        if (mapButton != null)
            mapButton.SetActive(true);

        // 6. Возобновляем время
        Time.timeScale = 1f;

        isMapOpen = false;
    }

    /// <summary>
    /// Принудительно устанавливает состояние карты.
    /// </summary>
    public void SetMapOpen(bool open)
    {
        if (open)
            OpenMap();
        else
            CloseMap();
    }

    /// <summary>
    /// Принудительно сбрасывает все ссылки.
    /// </summary>
    public void ResetReferences()
    {
        playerCamera = null;
        mapCamera = null;
        mainCamera = null;
        mapButton = null;
        mapPanel = null;
        closeButton = null;
        mapButtonComponent = null;
        closeButtonComponent = null;
        Debug.Log("🔄 CameraSwitcher: ссылки сброшены");
    }

    #endregion

    #region Camera Resolution

    /// <summary>
    /// Находит и инициализирует камеры.
    /// </summary>
    private void ResolveCameras()
    {
        // Находим основную камеру
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        // Находим камеру персонажа
        if (playerCamera == null)
        {
            playerCamera = mainCamera;
            Debug.Log("✅ Камера персонажа: Camera.main");
        }

        // Находим камеру карты
        if (mapCamera == null)
        {
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in allCameras)
            {
                if (cam.name.Contains("Map"))
                {
                    mapCamera = cam;
                    Debug.Log($"✅ Камера карты найдена: {cam.name}");
                    break;
                }
            }

            if (mapCamera == null)
            {
                Debug.LogWarning("⚠️ Камера карты не найдена! Создаю новую...");
                mapCamera = CreateMapCamera();
            }
        }
    }

    /// <summary>
    /// Создаёт камеру карты.
    /// </summary>
    private Camera CreateMapCamera()
    {
        GameObject mapCameraObj = new GameObject("MapCamera");
        Camera mapCam = mapCameraObj.AddComponent<Camera>();
        
        // Настройки
        mapCam.clearFlags = CameraClearFlags.SolidColor;
        mapCam.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        mapCam.fieldOfView = 60f;
        mapCam.nearClipPlane = 0.1f;
        mapCam.farClipPlane = 1000f;
        
        // Позиция сверху
        mapCameraObj.transform.position = new Vector3(0, cameraHeight, 0);
        mapCameraObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
        
        // Culling Mask - только слой карты
        int mapLayer = LayerMask.NameToLayer("Map");
        if (mapLayer >= 0)
            mapCam.cullingMask = 1 << mapLayer;
        else
            mapCam.cullingMask = ~0;
        
        Debug.Log("✅ Камера карты создана");
        return mapCam;
    }

    #endregion

    #region UI Creation

    /// <summary>
    /// Автоматически создаёт UI элементы.
    /// </summary>
    private void AutoCreateUI()
    {
        // Проверяем Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateCanvas();
        }

        // Создаём кнопку карты
        if (mapButton == null)
        {
            mapButton = CreateMapButton(canvas.transform);
            Debug.Log("✅ Кнопка карты создана");
        }

        // Создаём панель карты
        if (mapPanel == null)
        {
            mapPanel = CreateMapPanel(canvas.transform);
            Debug.Log("✅ Панель карты создана");
        }

        // Создаём кнопку закрытия
        if (closeButton == null)
        {
            closeButton = CreateCloseButton(canvas.transform);
            Debug.Log("✅ Кнопка закрытия создана");
        }
    }

    /// <summary>
    /// Создаёт Canvas для UI.
    /// </summary>
    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        
        DontDestroyOnLoad(canvasObject);
        
        return canvas;
    }

    /// <summary>
    /// Создаёт кнопку открытия карты.
    /// </summary>
    private GameObject CreateMapButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("MapButton");
        buttonObj.transform.SetParent(parent, false);
        
        // Button
        Button button = buttonObj.AddComponent<Button>();
        mapButtonComponent = button;
        
        // Image
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.6f, 1f, 0.8f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "🗺️ Карта";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        
        // RectTransform
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(120, 50);
        rectTransform.anchoredPosition = new Vector2(-90, 90);
        
        // Subscribe
        button.onClick.AddListener(ToggleMap);
        
        return buttonObj;
    }

    /// <summary>
    /// Создаёт панель карты.
    /// </summary>
    private GameObject CreateMapPanel(Transform parent)
    {
        GameObject panel = new GameObject("MapPanel");
        panel.transform.SetParent(parent, false);
        
        // Image background
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.3f);
        
        // RectTransform
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Скрыть по умолчанию
        panel.SetActive(false);
        
        return panel;
    }

    /// <summary>
    /// Создаёт кнопку закрытия карты.
    /// </summary>
    private GameObject CreateCloseButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(parent, false);
        
        // Button
        Button button = buttonObj.AddComponent<Button>();
        closeButtonComponent = button;
        
        // Image
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "✖️ Закрыть";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        
        // RectTransform
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(150, 50);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Subscribe
        button.onClick.AddListener(CloseMap);
        
        // Скрыть по умолчанию
        buttonObj.SetActive(false);
        
        return buttonObj;
    }

    #endregion

    #region UI Subscription

    /// <summary>
    /// Подписывается на события кнопок.
    /// </summary>
    private void SubscribeToButtons()
    {
        if (mapButtonComponent != null)
        {
            mapButtonComponent.onClick.RemoveAllListeners();
            mapButtonComponent.onClick.AddListener(ToggleMap);
        }

        if (closeButtonComponent != null)
        {
            closeButtonComponent.onClick.RemoveAllListeners();
            closeButtonComponent.onClick.AddListener(CloseMap);
        }
    }

    /// <summary>
    /// Отписывается от событий кнопок.
    /// </summary>
    private void UnsubscribeFromButtons()
    {
        if (mapButtonComponent != null)
        {
            mapButtonComponent.onClick.RemoveAllListeners();
        }

        if (closeButtonComponent != null)
        {
            closeButtonComponent.onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Input

    /// <summary>
    /// Получает ввод для движения.
    /// </summary>
    private Vector2 GetMovementInput()
    {
        // Проверяем джойстик
        MovementJoystick movementJoystick = FindFirstObjectByType<MovementJoystick>();
        if (movementJoystick != null)
        {
            // Используем рефлексию для получения ввода
            var inputProperty = movementJoystick.GetType().GetProperty("CurrentInput");
            if (inputProperty != null)
            {
                return (Vector2)inputProperty.GetValue(movementJoystick);
            }
        }

        // Fallback: клавиатура
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical);
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Сохраняет состояние карты.
    /// </summary>
    private void SaveMapState()
    {
        PlayerPrefs.SetInt(MAP_OPEN_KEY, isMapOpen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Загружает состояние карты.
    /// </summary>
    private void LoadMapState()
    {
        int saved = PlayerPrefs.GetInt(MAP_OPEN_KEY, 0);
        isMapOpen = saved == 1;
        
        if (isMapOpen)
        {
            Debug.Log("📂 Состояние карты загружено: карта открыта");
        }
    }

    #endregion
}
