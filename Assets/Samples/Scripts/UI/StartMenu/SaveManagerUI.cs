using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-менеджер меню сохранений.
/// Отображает список слотов, обрабатывает выбор и действия пользователя.
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

    /// <summary>
    /// Кэш UI-компонентов слотов. Заполняется при открытии меню.
    /// </summary>
    private SaveSlotUI[] _slotUIs;

    /// <summary>
    /// Индекс выбранного слота (-1 = ничего не выбрано).
    /// </summary>
    private int _selectedSlotIndex = -1;

    /// <summary>
    /// Флаг: меню открыто.
    /// </summary>
    private bool _isMenuOpen;

    /// <summary>
    /// Флаг: идёт обновление UI. Блокирует повторные вызовы RefreshSlots.
    /// </summary>
    private bool _isRefreshing;

    #endregion

    #region Properties

    /// <summary>
    /// Открыто ли в данный момент меню сохранений.
    /// </summary>
    public bool IsMenuOpen => _isMenuOpen;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий SaveManager при уничтожении компонента
        UnsubscribeFromSaveManager();
    }

    private void Update()
    {
        if (_isMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    #endregion

    #region Menu Control

    /// <summary>
    /// Открывает меню сохранений и обновляет список слотов.
    /// </summary>
    public void OpenMenu()
    {
        if (_isMenuOpen) return;

        // Подписываемся на события SaveManager, чтобы автоматически обновлять UI
        SubscribeToSaveManager();

        // Загружаем данные и обновляем UI
        SaveManager.Instance.RefreshSaveList();
        RefreshSlots();

        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(true);
        }

        _isMenuOpen = true;
        _selectedSlotIndex = -1;
        UpdateStatus("Выберите слот сохранения");
    }

    /// <summary>
    /// Закрывает меню сохранений.
    /// </summary>
    public void CloseMenu()
    {
        if (!_isMenuOpen) return;

        UnsubscribeFromSaveManager();

        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(false);
        }

        _isMenuOpen = false;
        _selectedSlotIndex = -1;
        UpdateStatus("");
    }

    #endregion

    #region Slot Refresh

    /// <summary>
    /// Обновляет визуальное представление всех слотов.
    /// Читает актуальные данные из кэша SaveManager и привязывает
    /// каждое сохранение к правильному слоту по индексу.
    /// </summary>
    public void RefreshSlots()
    {
        // Защита от рекурсии: если обновление уже идёт, выходим
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            // Получаем все UI-компоненты слотов из контейнера
            _slotUIs = slotContainer.GetComponentsInChildren<SaveSlotUI>();

            if (_slotUIs.Length == 0)
            {
                Debug.LogWarning("SaveManagerUI: Нет компонентов SaveSlotUI в slotContainer.");
                return;
            }

            // Проходим по каждому слоту и привязываем данные из кэша SaveManager.
            // SaveManager.refreshSaveList() уже заполнил _saveCache с правильным
            // соответствием: ключ = индекс слота, значение = SaveData.
            for (int i = 0; i < _slotUIs.Length; i++)
            {
                SaveData saveData = SaveManager.Instance.GetCachedSave(i);
                _slotUIs[i].Initialize(i, saveData, OnSlotSelected);
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnSlotSelected(int slotIndex)
    {
        _selectedSlotIndex = slotIndex;

        var saveData = SaveManager.Instance.GetCachedSave(slotIndex);
        if (saveData != null)
        {
            UpdateStatus($"Выбрано: {saveData.SaveName}");
        }
        else
        {
            UpdateStatus("Выбран: пустой слот");
        }
    }

    private void OnCreateNewSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Сначала выберите слот!");
            return;
        }

        var saveData = SaveManager.Instance.CreateCurrentSaveData();
        saveData.SaveName = $"Сохранение {_selectedSlotIndex + 1}";

        if (SaveManager.Instance.SaveGame(_selectedSlotIndex, saveData))
        {
            RefreshSlots();
            UpdateStatus($"Создано: {saveData.SaveName}");
        }
        else
        {
            UpdateStatus("Не удалось сохранить!");
        }
    }

    private void OnLoadSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Сначала выберите слот!");
            return;
        }

        if (!SaveManager.Instance.HasSave(_selectedSlotIndex))
        {
            UpdateStatus("Слот пуст!");
            return;
        }

        if (SaveManager.Instance.LoadAndApplyGame(_selectedSlotIndex))
        {
            CloseMenu();
            var saveData = SaveManager.Instance.GetCachedSave(_selectedSlotIndex);
            if (saveData != null)
                UpdateStatus($"Загружено: {saveData.SaveName}");
        }
        else
        {
            UpdateStatus("Не удалось загрузить!");
        }
    }

    private void OnDeleteSave()
    {
        if (_selectedSlotIndex < 0)
        {
            UpdateStatus("Сначала выберите слот!");
            return;
        }

        if (!SaveManager.Instance.HasSave(_selectedSlotIndex))
        {
            UpdateStatus("Слот пуст!");
            return;
        }

        if (SaveManager.Instance.DeleteSave(_selectedSlotIndex))
        {
            RefreshSlots();
            UpdateStatus($"Слот {_selectedSlotIndex + 1} очищен");
        }
        else
        {
            UpdateStatus("Не удалось удалить!");
        }
    }

    #endregion

    #region SaveManager Events

    /// <summary>
    /// Подписывается на события SaveManager для авто-обновления UI.
    /// </summary>
    private void SubscribeToSaveManager()
    {
        SaveManager.Instance.OnSaveCreated += OnSaveCreated;
        SaveManager.Instance.OnSaveDeleted += OnSaveDeleted;
    }

    /// <summary>
    /// Отписывается от событий SaveManager.
    /// </summary>
    private void UnsubscribeFromSaveManager()
    {
        SaveManager.Instance.OnSaveCreated -= OnSaveCreated;
        SaveManager.Instance.OnSaveDeleted -= OnSaveDeleted;
    }

    /// <summary>
    /// Вызывается при создании нового сохранения.
    /// </summary>
    private void OnSaveCreated(int slotIndex, SaveData data)
    {
        // Обновляем UI только если меню открыто — это предотвращает
        // вызов RefreshSlots, когда SaveManager сам только что создал файл.
        if (_isMenuOpen)
        {
            RefreshSlots();
        }
    }

    /// <summary>
    /// Вызывается при удалении сохранения.
    /// </summary>
    private void OnSaveDeleted(int slotIndex)
    {
        if (_isMenuOpen)
        {
            RefreshSlots();
        }
    }

    #endregion

    #region UI Helpers

    /// <summary>
    /// Устанавливает текст статусной строки.
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    #endregion
}
