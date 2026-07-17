using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Сброс масштаба карты.
/// Сбрасывает масштаб карты до значения по умолчанию (1.0) при нажатии на кнопку.
/// Работает только если масштаб увеличен (> 1.0).
/// </summary>
public class Reset : MonoBehaviour
{
    #region Constants

    private const float DEFAULT_SCALE = 1.0f;        // Масштаб по умолчанию
    private const float ZOOM_SPEED = 5f;             // Скорость плавного сброса зума

    #endregion

    #region Fields

    [Header("Zoom Settings")]
    [Tooltip("Масштаб карты по умолчанию")]
    [SerializeField] private float defaultScale = DEFAULT_SCALE;
    
    [Tooltip("Скорость плавного перехода к масштабу по умолчанию")]
    [SerializeField] private float zoomSpeed = ZOOM_SPEED;

    [Header("UI References")]
    [Tooltip("RectTransform карты для изменения масштаба")]
    [SerializeField] private RectTransform mapRectTransform;
    
    [Tooltip("Кнопка сброса зума (↺ или Reset)")]
    [SerializeField] private Button resetButton;

    #endregion

    #region Private State

    private float currentTargetScale;
    private bool isAnimating;

    #endregion

    #region Properties

    /// <summary>
    /// Текущий масштаб карты.
    /// </summary>
    public float CurrentScale => mapRectTransform?.localScale.x ?? DEFAULT_SCALE;
    
    /// <summary>
    /// Масштаб по умолчанию.
    /// </summary>
    public float DefaultScale => defaultScale;
    
    /// <summary>
    /// Активен ли сброс зума (текущий масштаб > 1.0).
    /// </summary>
    public bool CanReset => CurrentScale > 1.0f;

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
    /// Сбрасывает масштаб карты до значения по умолчанию (1.0).
    /// Вызывается при нажатии на кнопку сброса.
    /// Работает только если масштаб увеличен (> 1.0).
    /// </summary>
    public void ResetMapZoom()
    {
        if (!CanReset)
        {
            Debug.Log($"🔄 Сброс невозможен: текущий масштаб {CurrentScale:F2} ≤ 1.0");
            return;
        }

        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        // Устанавливаем целевой масштаб на значение по умолчанию
        currentTargetScale = defaultScale;
        isAnimating = true;

        Debug.Log($"🔄 Сброс зума: {CurrentScale:F2} → {defaultScale:F2}");
    }

    /// <summary>
    /// Мгновенно сбрасывает масштаб без анимации.
    /// </summary>
    public void InstantReset()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        currentTargetScale = defaultScale;
        mapRectTransform.localScale = Vector3.one * defaultScale;
        isAnimating = false;

        Debug.Log($"🔄 Мгновенный сброс зума: {defaultScale:F2}");
    }

    /// <summary>
    /// Проверяет, можно ли сбросить зум.
    /// </summary>
    public bool CanResetZoom()
    {
        return CanReset;
    }

    /// <summary>
    /// Устанавливает масштаб по умолчанию.
    /// </summary>
    public void SetDefaultScale(float scale)
    {
        defaultScale = Mathf.Max(scale, 0.1f);
        Debug.Log($"📏 Масштаб по умолчанию установлен: {defaultScale:F2}");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Настраивает UI компоненты при инициализации.
    /// </summary>
    private void SetupUI()
    {
        if (resetButton != null)
        {
            Button button = resetButton.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Добавьте компонент Button на {resetButton.name}!", this);
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ResetMapZoom);
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] resetButton не назначен в Inspector!", this);
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
    /// Обновляет состояние кнопки сброса.
    /// </summary>
    private void UpdateButtonState()
    {
        if (resetButton == null)
            return;

        Button button = resetButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = CanReset;
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying)
            return;

        GUILayout.BeginArea(new Rect(600, 10, 250, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("🔄 Reset Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"Текущий зум: {CurrentScale:F2}");
        GUILayout.Label($"Целевой зум: {currentTargetScale:F2}");
        GUILayout.Label($"Масштаб по умолчанию: {defaultScale:F2}");
        GUILayout.Label($"Можно сбросить: {CanReset}");
        GUILayout.Label($"Анимация: {(isAnimating ? "🟢 В процессе" : "⏹ Остановлено")}");
        GUILayout.Label("═══════════════════════════════");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Сброс")) ResetMapZoom();
        if (GUILayout.Button("Мгновенный сброс")) InstantReset();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif

    #endregion
}
