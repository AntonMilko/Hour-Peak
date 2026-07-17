using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление картой.
/// При нажатии кнопки карты отображается карта и игра ставится на паузу.
/// </summary>
public class Map : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Ключ для сохранения состояния карты.
    /// </summary>
    private const string MAP_OPEN_KEY = "IsMapOpen";

    #endregion

    #region Fields

    [Header("Cameras")]
    [Tooltip("Камера карты (вид сверху)")]
    [SerializeField] private Camera mapCamera;
    
    [Header("UI")]
    [Tooltip("Кнопка открытия карты")]
    [SerializeField] private Button mapButton;
    
    [Tooltip("Панель карты")]
    [SerializeField] private GameObject mapUIPanel;
    
    [Tooltip("Кнопка закрытия карты")]
    [SerializeField] private GameObject closeButton;

    [Header("Settings")]
    [Tooltip("Высота камеры карты")]
    [SerializeField] private float cameraHeight = 50f;
    
    [Tooltip("Включить авто-создание UI")]
    [SerializeField] private bool autoCreateUI = true;
    
    [Tooltip("Закрывать карту при начале движения")]
    [SerializeField] private bool closeMapOnMove = true;

    #endregion

    #region Private State

    private bool isMapOpen;
    private Button closeButtonComponent;

    #endregion

    #region Properties

    /// <summary>
    /// Текущее состояние карты (открыта/закрыта).
    /// </summary>
    public bool IsMapOpen => isMapOpen;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ Map: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ Map: активен на {GetCurrentPlatformName()}");
        
        if (autoCreateUI)
        {
            AutoCreateUI();
        }

        // Восстанавливаем состояние из сохранения
        LoadMapState();
    }

    private void Start()
    {
        SetupButtonListeners();
        
        // Скрываем карту на старте
        if (mapUIPanel != null) mapUIPanel.SetActive(false);
        if (mapCamera != null) mapCamera.gameObject.SetActive(false);
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

        isMapOpen = true;

        // Показываем UI
        if (mapUIPanel != null) mapUIPanel.SetActive(true);
        if (mapCamera != null) mapCamera.gameObject.SetActive(true);
        if (closeButton != null) closeButton.SetActive(true);
        if (mapButton != null) mapButton.gameObject.SetActive(false);

        // Останавливаем время (пауза)
        Time.timeScale = 0f;

        // Устанавливаем камеру в фиксированную позицию
        if (mapCamera != null)
        {
            mapCamera.transform.position = new Vector3(0, cameraHeight, 0);
            mapCamera.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
    }

    /// <summary>
    /// Закрывает карту.
    /// </summary>
    public void CloseMap()
    {
        if (!isMapOpen)
            return;

        Debug.Log("🔙 Карта закрыта");

        isMapOpen = false;

        // Скрываем UI
        if (mapUIPanel != null) mapUIPanel.SetActive(false);
        if (mapCamera != null) mapCamera.gameObject.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);
        if (mapButton != null) mapButton.gameObject.SetActive(true);

        // Возобновляем время
        Time.timeScale = 1f;
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
        mapCamera = null;
        mapButton = null;
        mapUIPanel = null;
        closeButton = null;
        closeButtonComponent = null;
        Debug.Log("🔄 Map: ссылки сброшены");
    }

    #endregion

    #region UI Creation

    /// <summary>
    /// Автоматически создаёт UI элементы.
    /// </summary>
    private void AutoCreateUI()
    {
        // Находим или создаём Canvas
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
        if (mapUIPanel == null)
        {
            mapUIPanel = CreateMapPanel(canvas.transform);
            Debug.Log("✅ Панель карты создана");
        }

        // Создаём кнопку закрытия
        if (closeButton == null)
        {
            closeButton = CreateCloseButton(canvas.transform);
            Debug.Log("✅ Кнопка закрытия создана");
        }

        // Находим камеру карты
        if (mapCamera == null)
        {
            mapCamera = CreateMapCamera();
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
    private Button CreateMapButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("MapButton");
        buttonObj.transform.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.6f, 1f, 0.8f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "🗺️ Карта";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(120, 50);
        rectTransform.anchoredPosition = new Vector2(-90, 90);
        
        return button;
    }

    /// <summary>
    /// Создаёт панель карты.
    /// </summary>
    private GameObject CreateMapPanel(Transform parent)
    {
        GameObject panel = new GameObject("MapPanel");
        panel.transform.SetParent(parent, false);
        
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.3f);
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
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
        
        Button button = buttonObj.AddComponent<Button>();
        closeButtonComponent = button;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "✖️ Закрыть";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(150, 50);
        rectTransform.anchoredPosition = Vector2.zero;
        
        buttonObj.SetActive(false);
        
        return buttonObj;
    }

    /// <summary>
    /// Создаёт камеру карты.
    /// </summary>
    private Camera CreateMapCamera()
    {
        GameObject mapCameraObj = new GameObject("MapCamera");
        Camera mapCam = mapCameraObj.AddComponent<Camera>();
        
        mapCam.clearFlags = CameraClearFlags.SolidColor;
        mapCam.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        mapCam.fieldOfView = 60f;
        mapCam.nearClipPlane = 0.1f;
        mapCam.farClipPlane = 1000f;
        
        mapCameraObj.transform.position = new Vector3(0, cameraHeight, 0);
        mapCameraObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
        
        int mapLayer = LayerMask.NameToLayer("Map");
        if (mapLayer >= 0)
            mapCam.cullingMask = 1 << mapLayer;
        else
            mapCam.cullingMask = ~0;
        
        Debug.Log("✅ Камера карты создана");
        return mapCam;
    }

    #endregion

    #region UI Subscription

    /// <summary>
    /// Настраивает слушателей кнопок.
    /// </summary>
    private void SetupButtonListeners()
    {
        if (mapButton != null)
        {
            mapButton.onClick.RemoveAllListeners();
            mapButton.onClick.AddListener(ToggleMap);
        }
        else
        {
            Debug.LogWarning("⚠️ MapButton не назначен!");
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
        if (mapButton != null)
        {
            mapButton.onClick.RemoveAllListeners();
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
        MovementJoystick movementJoystick = FindFirstObjectByType<MovementJoystick>();
        if (movementJoystick != null)
        {
            var inputProperty = movementJoystick.GetType().GetProperty("CurrentInput");
            if (inputProperty != null)
            {
                return (Vector2)inputProperty.GetValue(movementJoystick);
            }
        }

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

    #region Debug

    private void OnGUI()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 160, 250, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("🗺️ Map Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"IsMapOpen: {isMapOpen}");
        GUILayout.Label($"TimeScale: {Time.timeScale}");
        GUILayout.Label($"MapCamera: {(mapCamera != null ? "✅" : "❌")}");
        GUILayout.Label("═══════════════════════════════");
        if (GUILayout.Button("Toggle Map"))
        {
            ToggleMap();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
#endif
    }

    #endregion
}
