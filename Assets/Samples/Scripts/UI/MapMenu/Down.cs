using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Перемещение карты вниз.
/// Карта опускается вниз при нажатии на кнопку, но только если масштаб увеличен.
/// </summary>
public class Down : MonoBehaviour
{
    #region Constants

    private const float MOVE_AMOUNT = 50f;         // Скорость перемещения карты
    private const float SCALE_THRESHOLD = 1.1f;    // Порог зума для активации кнопки
    private const float MOVE_SPEED = 10f;          // Скорость плавного перемещения

    #endregion

    #region Fields

    [Header("Move Settings")]
    [Tooltip("Насколько перемещать карту вниз за одно нажатие (в единицах)")]
    [SerializeField] private float moveAmount = MOVE_AMOUNT;
    
    [Tooltip("Минимальный масштаб карты для активации кнопки (например, 1.1 = немного увеличен)")]
    [SerializeField] private float scaleThreshold = SCALE_THRESHOLD;
    
    [Tooltip("Скорость плавного перемещения карты")]
    [SerializeField] private float moveSpeed = MOVE_SPEED;

    [Header("UI References")]
    [Tooltip("RectTransform карты для перемещения")]
    [SerializeField] private RectTransform mapRectTransform;
    
    [Tooltip("Кнопка перемещения вниз (↓)")]
    [SerializeField] private Button downButton;

    #endregion

    #region Private State

    private Vector2 targetPosition;
    private bool isMoving;
    private bool isInitialized;

    #endregion

    #region Properties

    /// <summary>
    /// Текущий масштаб карты.
    /// </summary>
    public float CurrentScale => mapRectTransform?.localScale.x ?? 1f;
    
    /// <summary>
    /// Порог зума для активации кнопки.
    /// </summary>
    public float ScaleThreshold => scaleThreshold;
    
    /// <summary>
    /// Количество перемещения за одно нажатие.
    /// </summary>
    public float MoveAmount => moveAmount;
    
    /// <summary>
    /// Активна ли кнопка перемещения.
    /// </summary>
    public bool IsButtonActive => CurrentScale > scaleThreshold;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        SetupUI();
    }

    private void Start()
    {
        InitializePosition();
    }

    private void Update()
    {
        UpdateButtonState();
        UpdateMovement();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Перемещает карту вниз.
    /// Вызывается при нажатии на кнопку вниз.
    /// Работает только если масштаб карты больше порога.
    /// </summary>
    public void MoveMapDown()
    {
        if (!CanMove())
        {
            Debug.Log($"⬇️ Кнопка заблокирована: масштаб {CurrentScale:F2} < {scaleThreshold:F2}");
            return;
        }

        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        // Устанавливаем целевую позицию на moveAmount ниже
        Vector2 currentPosition = mapRectTransform.anchoredPosition;
        targetPosition = new Vector2(currentPosition.x, currentPosition.y - moveAmount);
        
        // Включаем плавное перемещение
        isMoving = true;
        isInitialized = true;

        Debug.Log($"⬇️ Карта опускается: Y {currentPosition.y:F2} → {targetPosition.y:F2}");
    }

    /// <summary>
    /// Перемещает карту вверх.
    /// Вызывается при нажатии на кнопку вверх.
    /// </summary>
    public void MoveMapUp()
    {
        if (!CanMove())
        {
            Debug.Log($"⬆️ Кнопка заблокирована: масштаб {CurrentScale:F2} < {scaleThreshold:F2}");
            return;
        }

        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        Vector2 currentPosition = mapRectTransform.anchoredPosition;
        targetPosition = new Vector2(currentPosition.x, currentPosition.y + moveAmount);
        isMoving = true;
        isInitialized = true;

        Debug.Log($"⬆️ Карта поднимается: Y {currentPosition.y:F2} → {targetPosition.y:F2}");
    }

    /// <summary>
    /// Перемещает карту влево.
    /// </summary>
    public void MoveMapLeft()
    {
        if (!CanMove())
        {
            Debug.Log($"⬅️ Кнопка заблокирована: масштаб {CurrentScale:F2} < {scaleThreshold:F2}");
            return;
        }

        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        Vector2 currentPosition = mapRectTransform.anchoredPosition;
        targetPosition = new Vector2(currentPosition.x - moveAmount, currentPosition.y);
        isMoving = true;
        isInitialized = true;

        Debug.Log($"⬅️ Карта сдвигается влево: X {currentPosition.x:F2} → {targetPosition.x:F2}");
    }

    /// <summary>
    /// Перемещает карту вправо.
    /// </summary>
    public void MoveMapRight()
    {
        if (!CanMove())
        {
            Debug.Log($"➡️ Кнопка заблокирована: масштаб {CurrentScale:F2} < {scaleThreshold:F2}");
            return;
        }

        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        Vector2 currentPosition = mapRectTransform.anchoredPosition;
        targetPosition = new Vector2(currentPosition.x + moveAmount, currentPosition.y);
        isMoving = true;
        isInitialized = true;

        Debug.Log($"➡️ Карта сдвигается вправо: X {currentPosition.x:F2} → {targetPosition.x:F2}");
    }

    /// <summary>
    /// Сбрасывает позицию карты к центру.
    /// </summary>
    public void ResetPosition()
    {
        if (mapRectTransform == null)
        {
            Debug.LogWarning($"[{GetType().Name}] mapRectTransform не назначен!", this);
            return;
        }

        targetPosition = Vector2.zero;
        isMoving = true;
        isInitialized = true;

        Debug.Log($"🔄 Позиция сброшена: (0, 0)");
    }

    /// <summary>
    /// Проверяет, можно ли перемещать карту.
    /// </summary>
    public bool CanMove()
    {
        return mapRectTransform != null && CurrentScale > scaleThreshold;
    }

    /// <summary>
    /// Устанавливает порог зума.
    /// </summary>
    public void SetScaleThreshold(float threshold)
    {
        scaleThreshold = threshold;
        UpdateButtonState();
        Debug.Log($"📏 Порог зума установлен: {scaleThreshold:F2}");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Настраивает UI компоненты при инициализации.
    /// </summary>
    private void SetupUI()
    {
        if (downButton != null)
        {
            Button button = downButton.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[{GetType().Name}] Добавьте компонент Button на {downButton.name}!", this);
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(MoveMapDown);
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] downButton не назначен в Inspector!", this);
        }

        UpdateButtonState();
    }

    /// <summary>
    /// Инициализирует начальную позицию карты.
    /// </summary>
    private void InitializePosition()
    {
        if (mapRectTransform != null)
        {
            targetPosition = mapRectTransform.anchoredPosition;
            isInitialized = true;
        }
    }

    /// <summary>
    /// Плавное перемещение карты в каждом кадре.
    /// </summary>
    private void UpdateMovement()
    {
        if (!isMoving || !isInitialized || mapRectTransform == null)
            return;

        Vector2 currentPosition = mapRectTransform.anchoredPosition;
        
        // Плавная интерполяция к целевой позиции
        Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, Time.deltaTime * moveSpeed);
        mapRectTransform.anchoredPosition = newPosition;

        // Проверяем, достигли ли цели
        if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
        {
            mapRectTransform.anchoredPosition = targetPosition;
            isMoving = false;
        }
    }

    /// <summary>
    /// Обновляет состояние кнопки перемещения.
    /// </summary>
    private void UpdateButtonState()
    {
        if (downButton == null)
            return;

        Button button = downButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = CanMove();
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying)
            return;

        GUILayout.BeginArea(new Rect(10, 340, 280, 180));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("⬇️ Down Map Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"Масштаб: {CurrentScale:F2}");
        GUILayout.Label($"Порог: {scaleThreshold:F2}");
        GUILayout.Label($"Позиция: ({mapRectTransform?.anchoredPosition.x:F1}, {mapRectTransform?.anchoredPosition.y:F1})");
        GUILayout.Label($"Можно перемещать: {CanMove()}");
        GUILayout.Label($"Движение: {(isMoving ? "🟢 В процессе" : "⏹ Остановлено")}");
        GUILayout.Label("═══════════════════════════════");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Направления:");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("⬆️")) MoveMapUp();
        if (GUILayout.Button("⬇️")) MoveMapDown();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("⬅️")) MoveMapLeft();
        if (GUILayout.Button("➡️")) MoveMapRight();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("🔄 Сброс")) ResetPosition();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
#endif

    #endregion
}