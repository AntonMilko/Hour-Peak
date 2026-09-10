using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление зумом карты.
/// Увеличивает масштаб карты при нажатии на кнопку "+".
/// </summary>
public class ZoomIn : MonoBehaviour
{
    #region Constants

    private const float ZOOM_STEP = 0.1f;      // Шаг увеличения зума
    private const float MAX_SCALE = 3.0f;      // Максимальный масштаб карты
    private const float MIN_SCALE = 1.0f;      // Минимальный масштаб карты
    private const float ZOOM_SPEED = 5f;       // Скорость плавного зума

    #endregion

    #region Fields

    [Header("Zoom Settings")]
    [Tooltip("Шаг увеличения масштаба при каждом нажатии")]
    [SerializeField] private float zoomStep = ZOOM_STEP;
    
    [Tooltip("Максимальный масштаб карты")]
    [SerializeField] private float maxScale = MAX_SCALE;
    
    [Tooltip("Минимальный масштаб карты")]
    [SerializeField] private float minScale = MIN_SCALE;
    
    [Tooltip("Скорость плавного перехода зума")]
    [SerializeField] private float zoomSpeed = ZOOM_SPEED;

    [Header("UI References")]
    [Tooltip("RectTransform карты для изменения масштаба")]
    [SerializeField] private RectTransform mapRectTransform;
    
    [Tooltip("Кнопка увеличения зума (+)")]
    [SerializeField] private Button zoomInButton;

    #endregion

    #region Private State

    private float currentTargetScale;
    private bool isAnimating;

    #endregion

    #region Properties

    /// <summary>
    /// Текущий масштаб карты.
    /// </summary>
    public float CurrentScale => mapRectTransform?.localScale.x ?? 1f;
    
    /// <summary>
    /// Максимально доступный масштаб.
    /// </summary>
    public float MaxScale => maxScale;
    
    /// <summary>
    /// Минимально доступный масштаб.
    /// </summary>
    public float MinScale => minScale;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        SetupUI();
    }

    private void Update()
    {
        UpdateZoom();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Увеличивает масштаб карты на один шаг.
    /// Вызывается при нажатии на кнопку "+".
    /// </summary>
    public void MapZoomIn()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!");
            return;
        }

        // Увеличиваем целевой масштаб
        currentTargetScale = Mathf.Clamp(
            CurrentScale + zoomStep,
            minScale,
            maxScale
        );

        // Если достигли максимума, отключаем кнопку
        UpdateButtonState();

        Debug.Log($"🔍 Зум увеличен: {CurrentScale:F2} → {currentTargetScale:F2}");
    }

    /// <summary>
    /// Уменьшает масштаб карты на один шаг.
    /// Вызывается при нажатии на кнопку "-".
    /// </summary>
    public void MapZoomOut()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!");
            return;
        }

        // Уменьшаем целевой масштаб
        currentTargetScale = Mathf.Clamp(
            CurrentScale - zoomStep,
            minScale,
            maxScale
        );

        // Если достигли минимума, отключаем кнопку
        UpdateButtonState();

        Debug.Log($"🔍 Зум уменьшен: {CurrentScale:F2} → {currentTargetScale:F2}");
    }

    /// <summary>
    /// Устанавливает конкретный масштаб карты.
    /// </summary>
    public void SetZoom(float scale)
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!");
            return;
        }

        currentTargetScale = Mathf.Clamp(scale, minScale, maxScale);
        mapRectTransform.localScale = Vector3.one * currentTargetScale;
        UpdateButtonState();

        Debug.Log($"🔍 Зум установлен: {currentTargetScale:F2}");
    }

    /// <summary>
    /// Сбрасывает зум к начальному значению (1.0).
    /// </summary>
    public void ResetZoom()
    {
        SetZoom(1f);
        Debug.Log($"🔍 Зум сброшен: 1.0");
    }

    /// <summary>
    /// Устанавливает максимальный зум.
    /// </summary>
    public void ZoomToMax()
    {
        SetZoom(maxScale);
    }

    /// <summary>
    /// Устанавливает минимальный зум.
    /// </summary>
    public void ZoomToMin()
    {
        SetZoom(minScale);
    }

    /// <summary>
    /// Проверяет, можно ли увеличить зум.
    /// </summary>
    public bool CanZoomIn()
    {
        return CurrentScale < maxScale;
    }

    /// <summary>
    /// Проверяет, можно ли уменьшить зум.
    /// </summary>
    public bool CanZoomOut()
    {
        return CurrentScale > minScale;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Настраивает UI компоненты при инициализации.
    /// </summary>
    private void SetupUI()
    {
        // Настраиваем слушатель на кнопке
        if (zoomInButton != null)
        {
            Button button = zoomInButton.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Добавьте компонент Button на {zoomInButton.name}!", this);
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(MapZoomIn);
        }

        // Инициализируем текущий масштаб
        if (mapRectTransform != null)
        {
            currentTargetScale = mapRectTransform.localScale.x;
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен в Inspector!", this);
        }

        UpdateButtonState();
    }

    /// <summary>
    /// Плавное обновление зума в каждом кадре.
    /// </summary>
    private void UpdateZoom()
    {
        if (mapRectTransform == null || !isAnimating)
            return;

        // Плавная интерполяция к целевому масштабу
        float currentX = mapRectTransform.localScale.x;
        float newX = Mathf.Lerp(currentX, currentTargetScale, Time.deltaTime * zoomSpeed);

        // Применяем новый масштаб
        mapRectTransform.localScale = Vector3.one * newX;

        // Проверяем, достигли ли цели
        if (Mathf.Abs(newX - currentTargetScale) < 0.001f)
        {
            mapRectTransform.localScale = Vector3.one * currentTargetScale;
            isAnimating = false;
            UpdateButtonState();
        }
    }

    /// <summary>
    /// Обновляет состояние кнопки зума.
    /// </summary>
    private void UpdateButtonState()
    {
        if (zoomInButton == null)
            return;

        Button button = zoomInButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = CanZoomIn();
        }
    }

    #endregion
}