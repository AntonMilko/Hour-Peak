using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI manager for save/load menu.
/// Displays list of save slots and handles user interaction.
/// </summary>
public class SaveManagerUI : MonoBehaviour
{
    #region Configuration

    [Header("UI References")]
    [SerializeField] private GameObject saveMenuPanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TextMeshProUGUI statusText;

    #endregion

    #region Private Fields

    private List<SaveSlotUI> _slotUIs;
    private int _selectedSlotIndex = -1;
    private bool _isMenuOpen;

    #endregion

    #region Properties

    /// <summary>
    /// Whether the save menu is currently open.
    /// </summary>
    public bool IsMenuOpen => _isMenuOpen;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    private void InitializeUI()
    {
        _slotUIs = new List<SaveSlotUI>();

        // Create slot UIs
        for (int i = 0; i < SaveManager.Instance.MaxSaveSlots; i++)
        {
            // Slot prefab should be assigned in Inspector
            // var slotGO = Instantiate(slotPrefab, slotContainer);
            // _slotUIs.Add(slotGO);
        }

        // Subscribe to SaveManager events
        SubscribeToEvents();

        Debug.Log("SaveManagerUI initialized.");
    }

    private void SubscribeToEvents()
    {
        SaveManager.Instance.OnSaveListRefreshed += OnSaveListRefreshed;
    }

    private void UnsubscribeFromEvents()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveListRefreshed -= OnSaveListRefreshed;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Opens the save/load menu.
    /// </summary>
    public void OpenMenu()
    {
        if (!_isMenuOpen)
        {
            RefreshSlots();
            
            if (saveMenuPanel != null)
            {
                saveMenuPanel.SetActive(true);
            }
            
            _isMenuOpen = true;
            _selectedSlotIndex = -1;
            UpdateStatus("Select a save slot");
        }
    }

    /// <summary>
    /// Closes the save/load menu.
    /// </summary>
    public void CloseMenu()
    {
        if (_isMenuOpen)
        {
            if (saveMenuPanel != null)
            {
                saveMenuPanel.SetActive(false);
            }
            
            _isMenuOpen = false;
            _selectedSlotIndex = -1;
            UpdateStatus("");
        }
    }

    /// <summary>
    /// Refreshes all save slots.
    /// </summary>
    public void RefreshSlots()
    {
        var saves = SaveManager.Instance.RefreshSaveList();
        
        for (int i = 0; i < _slotUIs.Count; i++)
        {
            SaveData saveData = null;
            
            if (i < saves.Count)
            {
                saveData = saves[i];
            }
            
            _slotUIs[i].Initialize(i, saveData, OnSlotSelected);
        }
    }

    /// <summary>
    /// Gets the currently selected slot index.
    /// </summary>
    public int GetSelectedSlotIndex()
    {
        return _selectedSlotIndex;
    }

    #endregion

    #region Event Handlers

    private void OnSlotSelected(int slotIndex)
    {
        _selectedSlotIndex = slotIndex;
        
        var saveData = SaveManager.Instance.GetCachedSave(slotIndex);
        if (saveData != null)
        {
            UpdateStatus($"Selected: {saveData.SaveName}");
        }
        else
        {
            UpdateStatus("Selected: Empty Slot");
        }
    }

    private void OnCreateNewSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Please select a slot first!");
            return;
        }

        var saveData = SaveManager.Instance.CreateCurrentSaveData();
        saveData.SaveName = $"Save {_selectedSlotIndex + 1}";
        
        if (SaveManager.Instance.SaveGame(_selectedSlotIndex, saveData))
        {
            RefreshSlots();
            UpdateStatus($"Save created: {saveData.SaveName}");
        }
        else
        {
            UpdateStatus("Failed to create save!");
        }
    }

    private void OnLoadSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Please select a slot first!");
            return;
        }

        if (!SaveManager.Instance.HasSave(_selectedSlotIndex))
        {
            UpdateStatus("Slot is empty!");
            return;
        }

        if (SaveManager.Instance.LoadAndApplyGame(_selectedSlotIndex))
        {
            CloseMenu();
            UpdateStatus($"Loaded: {SaveManager.Instance.GetCachedSave(_selectedSlotIndex).SaveName}");
        }
        else
        {
            UpdateStatus("Failed to load save!");
        }
    }

    private void OnDeleteSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Please select a slot first!");
            return;
        }

        if (!SaveManager.Instance.HasSave(_selectedSlotIndex))
        {
            UpdateStatus("Slot is empty!");
            return;
        }

        if (SaveManager.Instance.DeleteSave(_selectedSlotIndex))
        {
            RefreshSlots();
            UpdateStatus($"Save deleted from slot {_selectedSlotIndex + 1}");
        }
        else
        {
            UpdateStatus("Failed to delete save!");
        }
    }

    private void OnSaveListRefreshed()
    {
        RefreshSlots();
    }

    #endregion

    #region UI Updates

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    #endregion
}
