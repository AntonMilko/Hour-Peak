using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление паузой игры.
/// При нажатии кнопки паузы отображается PauseMenu и игра ставится на паузу.
/// </summary>
public class Pause : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Ключ для сохранения состояния паузы.
    /// </summary>
    private const string PAUSE_KEY = "IsPaused";

    #endregion

    #region Fields

    [Header("UI References")]
    [Tooltip("Панель паузы (автоматически создаётся, если не назначена)")]
    [SerializeField] private GameObject pauseMenu;
    
    [Tooltip("Игровой UI (джойстики, кнопки и т.д.)")]
    [SerializeField] private GameObject gameUI;
    
    [Tooltip("Кнопка паузы в игре")]
    [SerializeField] private GameObject pauseButton;

    [Header("Settings")]
    [Tooltip("Включить авто-создание UI")]
    [SerializeField] private bool autoCreateUI = true;
    
    [Tooltip("Включить паузу при старте сцены")]
    [SerializeField] private bool startPaused = false;

    #endregion

    #region Private State

    private bool isPaused;
    private Button pauseButtonComponent;

    #endregion

    #region Properties

    /// <summary>
    /// Текущее состояние паузы.
    /// </summary>
    public bool IsPaused => isPaused;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (!IsMobilePlatform())
        {
            Debug.Log($"⏹️ Pause: отключён ({GetCurrentPlatformName()} - не мобильная платформа)");
            enabled = false;
            return;
        }

        Debug.Log($"✅ Pause: активен на {GetCurrentPlatformName()}");
        
        if (autoCreateUI)
        {
            AutoCreateUI();
        }

        // Восстанавливаем состояние паузы из сохранения
        LoadPauseState();
    }

    private void Start()
    {
        SubscribeToPauseButton();
        
        if (startPaused && !isPaused)
        {
            PauseGame();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromPauseButton();
        SavePauseState();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Переключает состояние паузы (вкл/выкл).
    /// Вызывается из UI кнопки паузы.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Возобновляет игру.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused)
            return;

        Debug.Log("▶️ Игра возобновлена");

        // 1. Скрываем меню паузы
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        // 2. Показываем игровой UI
        if (gameUI != null)
            gameUI.SetActive(true);

        // 3. Показываем кнопку паузы
        if (pauseButton != null)
            pauseButton.SetActive(true);

        // 4. Возобновляем время
        Time.timeScale = 1f;

        isPaused = false;
    }

    /// <summary>
    /// Ставит игру на паузу.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused)
            return;

        Debug.Log("⏸️ Игра поставлена на паузу");

        // 1. Показываем меню паузы
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        // 2. Скрываем игровой UI
        if (gameUI != null)
            gameUI.SetActive(false);

        // 3. Скрываем кнопку паузы
        if (pauseButton != null)
            pauseButton.SetActive(false);

        // 4. Останавливаем время
        Time.timeScale = 0f;

        isPaused = true;
    }

    /// <summary>
    /// Принудительно устанавливает состояние паузы.
    /// </summary>
    public void SetPaused(bool paused)
    {
        if (paused)
            PauseGame();
        else
            ResumeGame();
    }

    /// <summary>
    /// Принудительно сбрасывает все ссылки.
    /// </summary>
    public void ResetReferences()
    {
        pauseMenu = null;
        gameUI = null;
        pauseButton = null;
        pauseButtonComponent = null;
        Debug.Log("🔄 Pause: ссылки сброшены");
    }

    #endregion

    #region UI Creation

    /// <summary>
    /// Автоматически создаёт UI элементы, если они не назначены.
    /// </summary>
    private void AutoCreateUI()
    {
        // Проверяем Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("⚠️ Canvas не найден! Создаю новый...");
            canvas = CreateCanvas();
        }

        // Создаём Pause Menu
        if (pauseMenu == null)
        {
            pauseMenu = CreatePauseMenu(canvas.transform);
            Debug.Log("✅ PauseMenu создан автоматически");
        }

        // Создаём Game UI
        if (gameUI == null)
        {
            gameUI = CreateGameUI(canvas.transform);
            Debug.Log("✅ GameUI создан автоматически");
        }

        // Создаём Pause Button
        if (pauseButton == null)
        {
            pauseButton = CreatePauseButton(canvas.transform);
            Debug.Log("✅ PauseButton создан автоматически");
        }
    }

    /// <summary>
    /// Создаёт новый Canvas для UI.
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
    /// Создаёт панель Pause Menu.
    /// </summary>
    private GameObject CreatePauseMenu(Transform parent)
    {
        GameObject menu = new GameObject("PauseMenu");
        menu.transform.SetParent(parent, false);

        // Image background
        Image background = menu.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.7f);

        // Panel
        RectTransform rectTransform = menu.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Add buttons
        CreateResumeButton(menu.transform);
        CreateSettingsButton(menu.transform);
        CreateExitButton(menu.transform);

        // Скрыть по умолчанию
        menu.SetActive(false);

        return menu;
    }

    /// <summary>
    /// Создаёт Game UI.
    /// </summary>
    private GameObject CreateGameUI(Transform parent)
    {
        GameObject gameUI = new GameObject("GameUI");
        gameUI.transform.SetParent(parent, false);
        gameUI.SetActive(true);
        return gameUI;
    }

    /// <summary>
    /// Создаёт кнопку паузы.
    /// </summary>
    private GameObject CreatePauseButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("PauseButton");
        buttonObj.transform.SetParent(parent, false);
        
        // Button component
        Button button = buttonObj.AddComponent<Button>();
        pauseButtonComponent = button;
        
        // Image
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.5f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "⏸️";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 30;
        
        // RectTransform
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(60, 60);
        rectTransform.anchoredPosition = new Vector2(-80, -80);
        
        // Subscribe
        button.onClick.AddListener(TogglePause);
        
        return buttonObj;
    }

    /// <summary>
    /// Создаёт кнопку "Продолжить" в меню паузы.
    /// </summary>
    private void CreateResumeButton(Transform parent)
    {
        GameObject buttonObj = CreateButton("ResumeButton", "▶️ Продолжить", parent, new Vector2(200, 50), new Vector2(0, 50));
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(ResumeGame);
    }

    /// <summary>
    /// Создаёт кнопку "Настройки" в меню паузы.
    /// </summary>
    private void CreateSettingsButton(Transform parent)
    {
        GameObject buttonObj = CreateButton("SettingsButton", "⚙️ Настройки", parent, new Vector2(200, 50), new Vector2(0, 0));
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => Debug.Log("🔧 Настройки..."));
    }

    /// <summary>
    /// Создаёт кнопку "Выйти" в меню паузы.
    /// </summary>
    private void CreateExitButton(Transform parent)
    {
        GameObject buttonObj = CreateButton("ExitButton", "🚪 Выйти", parent, new Vector2(200, 50), new Vector2(0, -50));
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => Application.Quit());
    }

    /// <summary>
    /// Создаёт универсальную кнопку UI.
    /// </summary>
    private GameObject CreateButton(string name, string text, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        // Button
        Button button = buttonObj.AddComponent<Button>();
        
        // Image
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        UnityEngine.UI.Text txt = textObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 20;
        txt.color = Color.white;
        
        // RectTransform
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
        
        return buttonObj;
    }

    #endregion

    #region UI Subscription

    /// <summary>
    /// Подписывается на события кнопки паузы.
    /// </summary>
    private void SubscribeToPauseButton()
    {
        if (pauseButtonComponent != null)
        {
            pauseButtonComponent.onClick.RemoveAllListeners();
            pauseButtonComponent.onClick.AddListener(TogglePause);
        }
    }

    /// <summary>
    /// Отписывается от событий кнопки паузы.
    /// </summary>
    private void UnsubscribeFromPauseButton()
    {
        if (pauseButtonComponent != null)
        {
            pauseButtonComponent.onClick.RemoveAllListeners();
        }
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Сохраняет состояние паузы.
    /// </summary>
    private void SavePauseState()
    {
        PlayerPrefs.SetInt(PAUSE_KEY, isPaused ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Загружает состояние паузы.
    /// </summary>
    private void LoadPauseState()
    {
        int saved = PlayerPrefs.GetInt(PAUSE_KEY, 0);
        isPaused = saved == 1;
        
        if (isPaused)
        {
            Debug.Log("📂 Состояние паузы загружено: пауза активна");
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

        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("⏸️ Pause Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"IsPaused: {isPaused}");
        GUILayout.Label($"TimeScale: {Time.timeScale}");
        GUILayout.Label($"AutoCreateUI: {autoCreateUI}");
        GUILayout.Label("═══════════════════════════════");
        if (GUILayout.Button("Toggle Pause"))
        {
            TogglePause();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
#endif
    }

    #endregion
}