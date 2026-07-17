using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Manages game save/load operations with file persistence.
/// Supports multiple save slots with DateTime metadata.
/// </summary>
public class SaveManager : MonoBehaviour
{
    #region Singleton Pattern

    private static SaveManager _instance;

    /// <summary>
    /// Global singleton accessor.
    /// </summary>
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>();
                if (_instance == null)
                {
                    var go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                }
            }
            return _instance;
        }
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Maximum number of save slots available.
    /// </summary>
    [SerializeField] [Range(3, 20)] private int maxSaveSlots = 5;

    /// <summary>
    /// Base directory for save files (relative to Application.persistentDataPath).
    /// </summary>
    [SerializeField] private string saveDirectoryName = "HourPeakSaves";

    /// <summary>
    /// File extension for save files.
    /// </summary>
    [SerializeField] private string fileExtension = ".json";

    /// <summary>
    /// Encoding for save files.
    /// </summary>
    [SerializeField] private string encodingName = "UTF8";

    #endregion

    #region Events

    /// <summary>
    /// Event triggered when a save is created.
    /// </summary>
    public event Action<int, SaveData> OnSaveCreated;

    /// <summary>
    /// Event triggered when a save is loaded.
    /// </summary>
    public event Action<int, SaveData> OnSaveLoaded;

    /// <summary>
    /// Event triggered when a save is deleted.
    /// </summary>
    public event Action<int> OnSaveDeleted;

    /// <summary>
    /// Event triggered when save list is refreshed.
    /// </summary>
    public event Action OnSaveListRefreshed;

    #endregion

    #region Private Fields

    private string _saveDirectoryPath;
    private Dictionary<int, SaveData> _saveCache;
    private Encoding _encoding;
    private bool _isInitialized;

    #endregion

    #region Properties

    /// <summary>
    /// Maximum number of save slots.
    /// </summary>
    public int MaxSaveSlots => maxSaveSlots;

    /// <summary>
    /// Full path to save directory.
    /// </summary>
    public string SaveDirectoryPath => _saveDirectoryPath;

    /// <summary>
    /// Gets cached save data for a slot.
    /// </summary>
    public SaveData GetCachedSave(int slotIndex)
    {
        return _saveCache.TryGetValue(slotIndex, out var data) ? data : null;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Initialize();
    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && _instance != null)
        {
            // Optionally auto-save on pause
            // AutoSaveCurrentGame();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the save system.
    /// </summary>
    private void Initialize()
    {
        if (_isInitialized) return;

        // Setup encoding
        _encoding = Encoding.UTF8;
        if (!string.IsNullOrEmpty(encodingName))
        {
            try
            {
                _encoding = Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning($"Invalid encoding '{encodingName}', using UTF8. Error: {e.Message}");
            }
        }

        // Create save directory
        _saveDirectoryPath = Path.Combine(Application.persistentDataPath, saveDirectoryName);
        
        if (!Directory.Exists(_saveDirectoryPath))
        {
            Directory.CreateDirectory(_saveDirectoryPath);
            Debug.Log($"Save directory created: {_saveDirectoryPath}");
        }

        // Initialize cache
        _saveCache = new Dictionary<int, SaveData>();

        // Load existing saves
        RefreshSaveList();

        _isInitialized = true;
        Debug.Log("SaveManager initialized successfully.");
    }

    /// <summary>
    /// Cleans up resources.
    /// </summary>
    private void Cleanup()
    {
        _saveCache?.Clear();
        _isInitialized = false;
    }

    #endregion

    #region Save Operations

    /// <summary>
    /// Saves game data to specified slot.
    /// </summary>
    /// <param name="slotIndex">Slot index (0 to MaxSaveSlots-1).</param>
    /// <param name="saveData">Data to save.</param>
    /// <returns>True if save succeeded.</returns>
    public bool SaveGame(int slotIndex, SaveData saveData)
    {
        if (!_isInitialized) Initialize();
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}. Must be between 0 and {maxSaveSlots - 1}.");
            return false;
        }
        if (saveData == null)
        {
            Debug.LogError("Cannot save null SaveData.");
            return false;
        }

        try
        {
            // Update metadata
            saveData.LastModified = DateTime.Now;
            saveData.Touch();

            // Serialize to JSON
            string json = JsonUtility.ToJson(saveData, true);

            // Write to file
            string filePath = GetSaveFilePath(slotIndex);
            File.WriteAllText(filePath, json, _encoding);

            // Update cache
            _saveCache[slotIndex] = saveData;

            // Trigger event
            OnSaveCreated?.Invoke(slotIndex, saveData);

            Debug.Log($"Game saved to slot {slotIndex}: {saveData.SaveName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game to slot {slotIndex}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves current game state to specified slot.
    /// </summary>
    public bool SaveCurrentGame(int slotIndex)
    {
        var saveData = CreateCurrentSaveData();
        return SaveGame(slotIndex, saveData);
    }

    /// <summary>
    /// Creates SaveData from current game state.
    /// </summary>
    public SaveData CreateCurrentSaveData()
    {
        var saveData = new SaveData
        {
            SaveId = Guid.NewGuid().ToString(),
            SaveName = GenerateSaveNameFromProgress(),
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now,
            CurrentLevel = GetCurrentLevel(),
            DifficultyLevel = GetDifficultyLevel(),
            PlayerPosition = GetPlayerPosition(),
            PlayerRotation = GetPlayerRotation(),
            LevelTime = GetCurrentLevelTime(),
            TotalPlaytime = GetTotalPlaytime(),
            IsLevelCompleted = IsLevelCompleted(),
            LevelStars = GetCurrentLevelStars(),
            TotalStars = GetTotalStars(),
            CrowdDensity = GetCrowdDensity(),
            CurrentSpeed = GetCurrentSpeed()
        };

        return saveData;
    }

    /// <summary>
    /// Generates save name from current progress.
    /// </summary>
    private string GenerateSaveNameFromProgress()
    {
        int level = GetCurrentLevel();
        DateTime now = DateTime.Now;
        return $"Level {level + 1} - {now:dd.MM.yyyy HH.mm}";
    }

    #endregion

    #region Load Operations

    /// <summary>
    /// Loads game data from specified slot.
    /// </summary>
    /// <param name="slotIndex">Slot index (0 to MaxSaveSlots-1).</param>
    /// <returns>Loaded SaveData or null if failed.</returns>
    public SaveData LoadGame(int slotIndex)
    {
        if (!_isInitialized) Initialize();
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}. Must be between 0 and {maxSaveSlots - 1}.");
            return null;
        }

        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file found at slot {slotIndex}.");
                return null;
            }

            // Read and deserialize
            string json = File.ReadAllText(filePath, _encoding);
            var saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogError($"Failed to parse save data from slot {slotIndex}.");
                return null;
            }

            // Update cache
            _saveCache[slotIndex] = saveData;

            // Trigger event
            OnSaveLoaded?.Invoke(slotIndex, saveData);

            Debug.Log($"Game loaded from slot {slotIndex}: {saveData.SaveName}");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game from slot {slotIndex}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads and applies game data from specified slot.
    /// </summary>
    public bool LoadAndApplyGame(int slotIndex)
    {
        var saveData = LoadGame(slotIndex);
        if (saveData == null) return false;

        ApplySaveData(saveData);
        return true;
    }

    /// <summary>
    /// Applies loaded save data to current game state.
    /// </summary>
    private void ApplySaveData(SaveData saveData)
    {
        SetCurrentLevel(saveData.CurrentLevel);
        SetDifficultyLevel(saveData.DifficultyLevel);
        SetPlayerPosition(saveData.PlayerPosition);
        SetPlayerRotation(saveData.PlayerRotation);
        SetCurrentLevelTime(saveData.LevelTime);
        SetTotalPlaytime(saveData.TotalPlaytime);
        SetLevelCompleted(saveData.IsLevelCompleted);
        SetCurrentLevelStars(saveData.LevelStars);
        SetTotalStars(saveData.TotalStars);
        SetCrowdDensity(saveData.CrowdDensity);
        SetCurrentSpeed(saveData.CurrentSpeed);

        Debug.Log("Save data applied to game state.");
    }

    #endregion

    #region Save Management

    /// <summary>
    /// Deletes save data from specified slot.
    /// </summary>
    public bool DeleteSave(int slotIndex)
    {
        if (!_isInitialized) Initialize();
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}.");
            return false;
        }

        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _saveCache.Remove(slotIndex);
            OnSaveDeleted?.Invoke(slotIndex);

            Debug.Log($"Save deleted from slot {slotIndex}.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save from slot {slotIndex}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Refreshes the list of available saves.
    /// </summary>
    public List<SaveData> RefreshSaveList()
    {
        _saveCache?.Clear();
        _saveCache = new Dictionary<int, SaveData>();

        var saves = new List<SaveData>();

        for (int i = 0; i < maxSaveSlots; i++)
        {
            string filePath = GetSaveFilePath(i);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath, _encoding);
                    var saveData = JsonUtility.FromJson<SaveData>(json);
                    
                    if (saveData != null)
                    {
                        _saveCache[i] = saveData;
                        saves.Add(saveData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to read save at slot {i}: {e.Message}");
                }
            }
        }

        // Sort by last modified (newest first)
        saves.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

        OnSaveListRefreshed?.Invoke();
        return saves;
    }

    /// <summary>
    /// Gets all available save slots.
    /// </summary>
    public List<int> GetAvailableSlots()
    {
        var slots = new List<int>();
        for (int i = 0; i < maxSaveSlots; i++)
        {
            if (File.Exists(GetSaveFilePath(i)))
            {
                slots.Add(i);
            }
        }
        return slots;
    }

    /// <summary>
    /// Checks if a slot has a save file.
    /// </summary>
    public bool HasSave(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    /// <summary>
    /// Gets file path for a slot.
    /// </summary>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(_saveDirectoryPath, $"save_{slotIndex:00}{fileExtension}");
    }

    #endregion

    #region Game State Getters/Setters

    // These should be implemented to get/set actual game state
    // Replace with your actual game manager methods or GameSaveController

    private int GetCurrentLevel()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetCurrentLevel")?.Invoke(controller, null) is int level ? level : 0 : 0;
    }

    private int GetDifficultyLevel()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetDifficultyLevel")?.Invoke(controller, null) is int level ? level : 0 : 0;
    }

    private Vector3 GetPlayerPosition()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetPlayerPosition")?.Invoke(controller, null) is Vector3 pos ? pos : Vector3.zero : Vector3.zero;
    }

    private Vector4 GetPlayerRotation()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetPlayerRotation")?.Invoke(controller, null) is Vector4 rot ? rot : new Vector4(0, 0, 0, 1) : new Vector4(0, 0, 0, 1);
    }

    private float GetCurrentLevelTime()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetCurrentLevelTime")?.Invoke(controller, null) is float time ? time : 0f : 0f;
    }

    private float GetTotalPlaytime()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetTotalPlaytime")?.Invoke(controller, null) is float time ? time : 0f : 0f;
    }

    private bool IsLevelCompleted()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_IsLevelCompleted")?.Invoke(controller, null) is bool completed ? completed : false : false;
    }

    private int GetCurrentLevelStars()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetCurrentLevelStars")?.Invoke(controller, null) is int stars ? stars : 0 : 0;
    }

    private int GetTotalStars()
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        return controller != null ? controller.GetType().GetMethod("SaveManager_GetTotalStars")?.Invoke(controller, null) is int stars ? stars : 0 : 0;
    }

    private int GetCrowdDensity() => 20;
    private float GetCurrentSpeed() => 2.5f;

    private void SetCurrentLevel(int level)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetCurrentLevel")?.Invoke(controller, new object[] { level });
    }

    private void SetDifficultyLevel(int difficulty)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetDifficultyLevel")?.Invoke(controller, new object[] { difficulty });
    }

    private void SetPlayerPosition(Vector3 pos)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetPlayerPosition")?.Invoke(controller, new object[] { pos });
    }

    private void SetPlayerRotation(Vector4 rot)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetPlayerRotation")?.Invoke(controller, new object[] { rot });
    }

    private void SetCurrentLevelTime(float time)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetCurrentLevelTime")?.Invoke(controller, new object[] { time });
    }

    private void SetTotalPlaytime(float time)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetTotalPlaytime")?.Invoke(controller, new object[] { time });
    }

    private void SetLevelCompleted(bool completed)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetLevelCompleted")?.Invoke(controller, new object[] { completed });
    }

    private void SetCurrentLevelStars(int stars)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetCurrentLevelStars")?.Invoke(controller, new object[] { stars });
    }

    private void SetTotalStars(int stars)
    {
        var controller = FindFirstObjectByType<GameSaveController>();
        controller?.GetType().GetMethod("SaveManager_SetTotalStars")?.Invoke(controller, new object[] { stars });
    }

    private void SetCrowdDensity(int density) { /* From GameSettings */ }
    private void SetCurrentSpeed(float speed) { /* From GameSettings */ }

    #endregion
}