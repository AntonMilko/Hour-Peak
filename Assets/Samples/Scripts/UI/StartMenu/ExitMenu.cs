using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Меню выхода из игры с подтверждением.
/// Автоматически создаёт UI диалоговое окно.
/// - Кнопка "Да": выход из игры
/// - Кнопка "Нет": возврат в Главное Меню
/// </summary>
public class ExitMenu : MonoBehaviour
{
    #region Singleton

    private static ExitMenu _instance;
    public static ExitMenu Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("ExitMenu");
                _instance = obj.AddComponent<ExitMenu>();
                Debug.Log("✅ ExitMenu Instance создан автоматически");
            }
            return _instance;
        }
    }

    #endregion

    #region Fields

    private GameObject dialogPanel;
    private Button yesButton;
    private Button noButton;
    private TextMeshProUGUI questionText;
    private bool isInitialized = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Статический метод для показа диалога из любого места.
    /// </summary>
    public static void Show()
    {
        Instance.ShowExitDialog();
    }

    /// <summary>
    /// Показывает диалоговое окно выхода.
    /// Вызывается из Unity Inspector кнопкой "Выйти из игры".
    /// </summary>
    public void OnExitButtonClicked()
    {
        ShowExitDialog();
    }

    /// <summary>
    /// Показывает диалоговое окно выхода.
    /// </summary>
    public void ShowExitDialog()
    {
        InitializeUI();
        
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            Time.timeScale = 0f; // Остановить время
            Debug.Log("🚪 Меню выхода: отображено");
        }
    }

    /// <summary>
    /// Подтверждает выход из игры (кнопка "Да").
    /// </summary>
    public void ConfirmExit()
    {
        Debug.Log("✅ Подтверждение выхода из игры...");
        HideDialog();
        QuitApplication();
    }

    /// <summary>
    /// Отменяет выход и возвращается в Главное Меню (кнопка "Нет").
    /// </summary>
    public void CancelExit()
    {
        Debug.Log("🔄 Возврат в Главное Меню...");
        HideDialog();
        ReturnToMainMenu();
    }

    /// <summary>
    /// Скрывает диалоговое окно.
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
            Time.timeScale = 1f; // Восстановить время
            Debug.Log("🚪 Меню выхода: скрыто");
        }
    }

    /// <summary>
    /// Выходит из приложения.
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("⏹️ Завершение работы приложения...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("⏹️ Editor: остановка игры");
#else
        Application.Quit();
        Debug.Log("⏹️ Build: Application.Quit() вызван");
#endif
    }

    /// <summary>
    /// Возвращает в Главное Меню (первая сцена в Build Settings).
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (SceneManager.sceneCountInBuildSettings > 0)
        {
            SceneManager.LoadScene(0);
            Debug.Log($"📺 Загружена сцена 0 (Главное Меню)");
        }
        else
        {
            Debug.LogWarning("⚠️ Нет сцен в Build Settings! Сцена 0 не найдена");
        }
    }

    #endregion

    #region UI Creation

    /// <summary>
    /// Создаёт UI диалогового окна программно.
    /// </summary>
    private void InitializeUI()
    {
        if (isInitialized) return;

        Debug.Log("🎨 Создание UI диалогового окна выхода...");

        // Создаём Canvas если нет
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✅ Canvas создан");
        }

        // Создаём панель диалога
        dialogPanel = new GameObject("ExitDialogPanel");
        dialogPanel.transform.SetParent(canvas.transform, false);
        dialogPanel.SetActive(false);

        // Добавляем Image для фона
        Image bgImage = dialogPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f); // Полупрозрачный чёрный

        // Добавляем Panel для контента
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(dialogPanel.transform, false);
        
        Image panelImage = contentPanel.AddComponent<Image>();
        panelImage.color = Color.white;
        
        // Layout
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(400, 200);
        contentRect.anchoredPosition = Vector2.zero;

        // Создаём текст вопроса
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(contentPanel.transform, false);
        
        questionText = questionObj.AddComponent<TextMeshProUGUI>();
        questionText.text = "Вы действительно хотите выйти из игры?";
        questionText.fontSize = 18;
        questionText.alignment = TextAlignmentOptions.Center;
        questionText.color = Color.black;
        
        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.sizeDelta = new Vector2(350, 50);
        questionRect.anchoredPosition = new Vector2(0, 40);

        // Создаём панель кнопок
        GameObject buttonsObj = new GameObject("ButtonsPanel");
        buttonsObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform buttonsRect = buttonsObj.GetComponent<RectTransform>();
        buttonsRect.sizeDelta = new Vector2(300, 50);
        buttonsRect.anchoredPosition = new Vector2(0, -20);

        // Создаём кнопку "Да"
        yesButton = CreateButton(buttonsObj.transform, "Да", -85, ConfirmExit);
        
        // Создаём кнопку "Нет"
        noButton = CreateButton(buttonsObj.transform, "Нет", 85, CancelExit);

        isInitialized = true;
        Debug.Log("✅ UI диалогового окна создан успешно");
    }

    /// <summary>
    /// Создаёт кнопку.
    /// </summary>
    private Button CreateButton(Transform parent, string text, float xPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);

        // Image
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f); // Синий

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = 16;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.sizeDelta = new Vector2(100, 30);
        txtRect.anchoredPosition = Vector2.zero;

        // Button component
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        // Layout
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(100, 40);
        btnRect.anchoredPosition = new Vector2(xPos, 0);

        return button;
    }

    #endregion

    #region Debug

    /// <summary>
    /// Выводит статус в консоль.
    /// </summary>
    public void DebugLogStatus()
    {
        Debug.Log("═══════════════════════════════");
        Debug.Log("🚪 Статус ExitMenu");
        Debug.Log("═══════════════════════════════");
        Debug.Log($"Инициализирован: {isInitialized}");
        Debug.Log($"DialogPanel: {(dialogPanel != null ? "Создан" : "Не создан")}");
        Debug.Log($"YesButton: {(yesButton != null ? "Создан" : "Не создан")}");
        Debug.Log($"NoButton: {(noButton != null ? "Создан" : "Не создан")}");
        Debug.Log("═══════════════════════════════");
    }

    #endregion
}
