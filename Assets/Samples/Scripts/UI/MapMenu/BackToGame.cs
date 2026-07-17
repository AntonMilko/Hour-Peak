using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт для кнопки "Вернуться в игру" (Закрыть карту).
/// Отвечает за скрытие меню карты, показ интерфейса игры и возобновление времени.
/// </summary>
[RequireComponent(typeof(Button))]
public class BackToGame : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [Tooltip("Панель карты (скрывается при нажатии)")]
    [SerializeField] private GameObject mapPanel;

    [Tooltip("Основной игровой интерфейс (показывается при нажатии)")]
    [SerializeField] private GameObject gameUI;

    [Tooltip("Кнопка открытия карты (показывается при нажатии)")]
    [SerializeField] private GameObject mapButton;

    [Header("Settings")]
    [Tooltip("Сбрасывать позицию и масштаб карты в начальное состояние при закрытии?")]
    [SerializeField] private bool resetMapToHome = true;

    #endregion

    #region Private Fields

    private RectTransform mapRectTransform;
    private Vector2 initialPosition;
    private Vector3 initialScale;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeReferences();
        SetupButton();
        
        // Запоминаем "домашнее" положение карты при старте сцены
        if (mapRectTransform != null)
        {
            initialPosition = mapRectTransform.anchoredPosition;
            initialScale = mapRectTransform.localScale;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Главная функция, вызываемая при нажатии кнопки.
    /// </summary>
    public void OnBackToGameClicked()
    {
        // 1. Опционально сбрасываем карту в центр
        if (resetMapToHome)
        {
            ResetMapPosition();
        }

        // 2. Переключаем видимость UI
        ToggleUIPanels();

        // 3. Возобновляем управление игроком и время
        ResumeGameplay();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Инициализация ссылок на компоненты.
    /// </summary>
    private void InitializeReferences()
    {
        if (mapPanel != null)
        {
            mapRectTransform = mapPanel.GetComponent<RectTransform>();
            if (mapRectTransform == null)
                Debug.LogWarning($"[{GetType().Name}] На объекте {mapPanel.name} не найден RectTransform!", this);
        }
    }

    /// <summary>
    /// Настраивает слушатель событий на кнопке.
    /// </summary>
    private void SetupButton()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            // Очищаем старые слушатели, чтобы не было дублирования при повторном включении
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnBackToGameClicked);
        }
    }

    /// <summary>
    /// Скрывает карту, показывает интерфейс игры и кнопку карты.
    /// </summary>
    private void ToggleUIPanels()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        if (mapButton != null) mapButton.SetActive(true);
    }

    /// <summary>
    /// Возвращает карту в исходное положение (изначально запомненное в Awake).
    /// </summary>
    private void ResetMapPosition()
    {
        if (mapRectTransform != null)
        {
            // Возвращаем координаты и масштаб
            mapRectTransform.anchoredPosition = initialPosition;
            mapRectTransform.localScale = initialScale;
        }
    }

    /// <summary>
    /// Сбрасывает время и возвращает управление.
    /// </summary>
    private void ResumeGameplay()
    {
        // Размораживаем время (снимаем паузу)
        Time.timeScale = 1f;

        // Блокируем и скрываем курсор мыши
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion
}