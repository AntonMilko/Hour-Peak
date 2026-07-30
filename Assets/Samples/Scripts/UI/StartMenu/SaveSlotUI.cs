using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying and interacting with a save slot (level button).
/// Attach to each save slot button in the UI.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    #region Configuration

    [Header("UI References")]
    [SerializeField] private Button slotButton;
    [SerializeField] private TextMeshProUGUI slotDateText;
    [SerializeField] private Image slotImage;          // Изображение уровня (превью)
    [SerializeField] private GameObject lockOverlay;   // Оверлей замка (если есть)

    #endregion

    #region Private Fields

    private int _slotIndex;
    private SaveData _currentSaveData;
    private Action<int> _onSlotSelected;
    private bool _isLevelUnlocked = true;
    private bool _isLevelCompleted = false;

    #endregion

    #region Properties

    /// <summary>
    /// Current slot index.
    /// </summary>
    public int SlotIndex => _slotIndex;

    /// <summary>
    /// Current save data for this slot.
    /// </summary>
    public SaveData CurrentSaveData => _currentSaveData;

    /// <summary>
    /// Whether this slot is empty.
    /// </summary>
    public bool IsEmpty => _currentSaveData == null;

    /// <summary>
    /// Whether the level is unlocked.
    /// </summary>
    public bool IsLevelUnlocked => _isLevelUnlocked;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Активируем слот при загрузке
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            Debug.Log($"SaveSlotUI.Awake: Activated slot '{gameObject.name}'");
        }
        
        Debug.Log($"SaveSlotUI.Awake: slot='{gameObject.name}', active={gameObject.activeInHierarchy}");

        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the slot with data and callback.
    /// </summary>
    public void Initialize(int slotIndex, SaveData saveData, Action<int> onSlotSelected)
    {
        _slotIndex = slotIndex;
        _currentSaveData = saveData;
        _onSlotSelected = onSlotSelected;
        
        Debug.Log($"SaveSlotUI[{slotIndex}]: Initialized with saveData={saveData?.SaveName ?? "null"}, active={gameObject?.activeInHierarchy}");
        
        // Обновляем текст слота
        if (slotDateText != null)
        {
            if (saveData != null)
            {
                slotDateText.text = saveData.SaveName;
            }
            else
            {
                slotDateText.text = $"Файл {slotIndex + 1}";
            }
        }
    }

    /// <summary>
    /// Устанавливает статус разблокировки уровня и обновляет визуал.
    /// </summary>
    public void SetLevelUnlocked(bool unlocked)
    {
        _isLevelUnlocked = unlocked;
        UpdateVisualState();
    }

    /// <summary>
    /// Устанавливает статус прохождения уровня.
    /// </summary>
    public void SetLevelCompleted(bool completed)
    {
        _isLevelCompleted = completed;
        UpdateVisualState();
    }

    /// <summary>
    /// Устанавливает номер уровня (для отображения в кнопке).
    /// </summary>
    public void SetLevelNumber(int levelNumber)
    {
        if (slotDateText != null)
        {
            slotDateText.text = $"Ур. {levelNumber}";
        }
    }

    /// <summary>
    /// Устанавливает выделение слота.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (slotButton != null)
        {
            slotButton.interactable = _isLevelUnlocked;
        }
    }

    /// <summary>
    /// Применяет визуальное состояние (locked/unlocked/completed).
    /// </summary>
    private void UpdateVisualState()
    {
        // 1. Блокировка/разблокировка
        if (slotButton != null)
        {
            slotButton.interactable = _isLevelUnlocked;
        }

        // 2. Затемнение изображения для заблокированных уровней
        if (slotImage != null)
        {
            // Если уровень пройден — яркий, разблокирован — чуть темнее, заблокирован — тёмный
            if (_isLevelCompleted)
            {
                slotImage.color = Color.white;
            }
            else if (_isLevelUnlocked)
            {
                // Разблокирован, но не пройден — чуть приглушённый
                slotImage.color = new Color(0.85f, 0.85f, 0.85f);
            }
            else
            {
                // Заблокирован — сильно затемнён
                slotImage.color = new Color(0.35f, 0.35f, 0.35f);
            }
        }

        // 3. Оверлей замка
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!_isLevelUnlocked);
        }

        // Текст устанавливается через SetLevelNumber() — не перезаписываем здесь
    }

    #endregion

    #region Event Handlers

    private void OnSlotClicked()
    {
        Debug.Log($"SaveSlotUI: Clicked slot index={_slotIndex}, unlocked={_isLevelUnlocked}, saveData={_currentSaveData?.SaveName ?? "null"}");
        
        if (!_isLevelUnlocked)
        {
            Debug.Log($"🔒 Уровень {_slotIndex + 1} заблокирован!");
            return;
        }

        if (_onSlotSelected != null)
        {
            _onSlotSelected(_slotIndex);
        }
    }

    #endregion

    #region Utilities

    private string RepeatChar(char c, int count)
    {
        char[] arr = new char[count];
        for (int i = 0; i < count; i++) arr[i] = c;
        return new string(arr);
    }

    #endregion
}
