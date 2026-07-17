using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление уменьшением зума карты.
/// Уменьшает масштаб карты при нажатии на кнопку "-".
/// </summary>
public class ZoomOut : MonoBehaviour
{
    #region Constants

    private const float ZOOM_STEP = 0.1f;      // Шаг уменьшения зума
    private const float MIN_SCALE = 1.0f;      // Минимальный масштаб карты
    private const float MAX_SCALE = 3.0f;      // Максимальный масштаб карты
    private const float ZOOM_SPEED = 5f;       // Скорость плавного зума

    #endregion

    #region Fields

    [Header("Zoom Settings")]
    [Tooltip("Шаг уменьшения масштаба при каждом нажатии")]
    [SerializeField] private float zoomStep = ZOOM_STEP;
    
    [Tooltip("Минимальный масштаб карты")]
    [SerializeField] private float minScale = MIN_SCALE;
    
    [Tooltip("Максимальный масштаб карты")]
    [SerializeField] private float maxScale = MAX_SCALE;
    
    [Tooltip("Скорость плавного перехода зума")]
    [SerializeField] private float zoomSpeed = ZOOM_SPEED;

    [Header("UI References")]
    [Tooltip("RectTransform карты для изменения масштаба")]
    [SerializeField] private RectTransform mapRectTransform;
    
    [Tooltip("Кнопка уменьшения зума (-)")]
    [SerializeField] private Button zoomOutButton;

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
    /// Уменьшает масштаб карты на один шаг.
    /// Вызывается при нажатии на кнопку "-".
    /// </summary>
    public void MapZoomOut()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
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
    /// Увеличивает масштаб карты на один шаг.
    /// Вызывается при нажатии на кнопку "+".
    /// </summary>
    public void MapZoomIn()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
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
    /// Устанавливает конкретный масштаб карты.
    /// </summary>
    public void SetZoom(float scale)
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
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
        if (zoomOutButton != null)
        {
            Button button = zoomOutButton.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Добавьте компонент Button на {zoomOutButton.name}!", this);
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(MapZoomOut);
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
        if (zoomOutButton == null)
            return;

        Button button = zoomOutButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = CanZoomOut();
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("🔍 ZoomOut Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"Текущий зум: {CurrentScale:F2}");
        GUILayout.Label($"Целевой зум: {currentTargetScale:F2}");
        GUILayout.Label($"Диапазон: {minScale:F2} - {maxScale:F2}");
        GUILayout.Label($"Можно увеличить: {CanZoomIn()}");
        GUILayout.Label($"Можно уменьшить: {CanZoomOut()}");
        GUILayout.Label("═══════════════════════════════");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+")) MapZoomIn();
        if (GUILayout.Button("-")) MapZoomOut();
        if (GUILayout.Button("Сброс")) ResetZoom();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif

    #endregion
}