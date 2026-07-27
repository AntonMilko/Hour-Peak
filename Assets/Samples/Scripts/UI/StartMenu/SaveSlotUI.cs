using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying and interacting with a save slot.
/// Attach to each save slot button in the UI.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    #region Configuration

    [Header("UI References")]
    [SerializeField] private Button slotButton;
    [SerializeField] private TextMeshProUGUI slotDateText;

    #endregion

    #region Private Fields

    private int _slotIndex;
    private SaveData _currentSaveData;
    private Action<int> _onSlotSelected;

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

    #endregion

    #region Event Handlers

    private void OnSlotClicked()
    {
        Debug.Log($"SaveSlotUI: Clicked slot index={_slotIndex}, saveData={_currentSaveData?.SaveName ?? "null"}");
        
        if (_onSlotSelected != null)
        {
            _onSlotSelected(_slotIndex);
        }
    }

    #endregion
}
