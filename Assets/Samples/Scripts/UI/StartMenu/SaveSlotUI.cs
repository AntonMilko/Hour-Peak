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
    }

    #endregion

    #region Event Handlers

    private void OnSlotClicked()
    {
        _onSlotSelected?.Invoke(_slotIndex);
    }

    #endregion
}
