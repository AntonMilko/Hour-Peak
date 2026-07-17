using System;
using UnityEngine;

/// <summary>
/// Integrates game state with save system.
/// Attach to your main game manager object.
/// </summary>
public class GameSaveController : MonoBehaviour
{
    #region Configuration

    [Header("Save Settings")]
    [SerializeField] private bool autoSaveOnLevelComplete = true;
    [SerializeField] private bool autoSaveOnCheckpoint = true;
    [SerializeField] private int autoSaveSlot = 0;

    [Header("Debug")]
    [SerializeField] private bool showSaveLogs = true;

    #endregion

    #region Private Fields

    private float _currentLevelTime;
    private float _totalPlaytime;
    private bool _levelCompleted;
    private int _currentLevelStars;
    private int _totalStars;
    private int _currentLevel;
    private int _difficultyLevel;

    #endregion

    #region Properties

    /// <summary>
    /// Current level index (0 = first level).
    /// </summary>
    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            _currentLevel = Mathf.Max(0, value);
            OnLevelChanged();
        }
    }

    /// <summary>
    /// Current difficulty level.
    /// </summary>
    public int DifficultyLevel
    {
        get => _difficultyLevel;
        set
        {
            _difficultyLevel = Mathf.Clamp(value, 0, 2);
            OnDifficultyChanged();
        }
    }

    /// <summary>
    /// Current level time (seconds).
    /// </summary>
    public float CurrentLevelTime => _currentLevelTime;

    /// <summary>
    /// Total playtime (seconds).
    /// </summary>
    public float TotalPlaytime => _totalPlaytime;

    /// <summary>
    /// Stars earned in current level.
    /// </summary>
    public int CurrentLevelStars => _currentLevelStars;

    /// <summary>
    /// Total accumulated stars.
    /// </summary>
    public int TotalStars => _totalStars;

    /// <summary>
    /// Whether current level is completed.
    /// </summary>
    public bool IsLevelCompleted => _levelCompleted;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeGameState();
    }

    private void Update()
    {
        UpdateGameTime();
    }

    private void OnApplicationQuit()
    {
        AutoSaveGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            AutoSaveGame();
        }
    }

    #endregion

    #region Initialization

    private void InitializeGameState()
    {
        _currentLevelTime = 0f;
        _totalPlaytime = 0f;
        _levelCompleted = false;
        _currentLevelStars = 0;
        _totalStars = 0;
        _currentLevel = 0;
        _difficultyLevel = 0;

        if (showSaveLogs)
        {
            Debug.Log("GameSaveController initialized.");
        }
    }

    #endregion

    #region Game Time

    private void UpdateGameTime()
    {
        _currentLevelTime += Time.deltaTime;
        _totalPlaytime += Time.deltaTime;
    }

    #endregion

    #region Level Progress

    /// <summary>
    /// Called when player reaches a checkpoint.
    /// </summary>
    public void OnCheckpointReached()
    {
        if (autoSaveOnCheckpoint)
        {
            SaveGame(autoSaveSlot);
        }
    }

    /// <summary>
    /// Called when level is completed.
    /// </summary>
    /// <param name="earnedStars">Stars earned (0-3).</param>
    public void OnLevelCompleted(int earnedStars)
    {
        _levelCompleted = true;
        _currentLevelStars = Mathf.Clamp(earnedStars, 0, 3);
        _totalStars += _currentLevelStars;

        if (autoSaveOnLevelComplete)
        {
            SaveGame(autoSaveSlot);
        }

        if (showSaveLogs)
        {
            Debug.Log($"Level completed! Stars: {_currentLevelStars}/{3}, Total: {_totalStars}");
        }
    }

    /// <summary>
    /// Called when level is failed/restarted.
    /// </summary>
    public void OnLevelFailed()
    {
        _levelCompleted = false;
        _currentLevelStars = 0;
        _currentLevelTime = 0f;

        if (showSaveLogs)
        {
            Debug.Log("Level failed. Progress reset.");
        }
    }

    /// <summary>
    /// Starts a new level.
    /// </summary>
    public void StartLevel(int levelIndex)
    {
        _currentLevel = Mathf.Max(0, levelIndex);
        _levelCompleted = false;
        _currentLevelStars = 0;
        _currentLevelTime = 0f;

        if (showSaveLogs)
        {
            Debug.Log($"Starting level {_currentLevel + 1}");
        }
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Saves current game state to specified slot.
    /// </summary>
    public bool SaveGame(int slotIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return false;
        }

        var saveData = SaveManager.Instance.CreateCurrentSaveData();
        saveData.SaveName = $"Level {_currentLevel + 1} - {DateTime.Now:dd.MM.yyyy HH.mm}";

        bool success = SaveManager.Instance.SaveGame(slotIndex, saveData);

        if (success && showSaveLogs)
        {
            Debug.Log($"Game saved to slot {slotIndex}: {saveData.SaveName}");
        }

        return success;
    }

    /// <summary>
    /// Loads game state from specified slot.
    /// </summary>
    public bool LoadGame(int slotIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager not found!");
            return false;
        }

        var saveData = SaveManager.Instance.LoadGame(slotIndex);
        if (saveData == null)
        {
            Debug.LogWarning($"No save data found in slot {slotIndex}");
            return false;
        }

        ApplySaveData(saveData);

        if (showSaveLogs)
        {
            Debug.Log($"Game loaded from slot {slotIndex}: {saveData.SaveName}");
        }

        return true;
    }

    /// <summary>
    /// Applies save data to current game state.
    /// </summary>
    private void ApplySaveData(SaveData saveData)
    {
        _currentLevel = saveData.CurrentLevel;
        _difficultyLevel = saveData.DifficultyLevel;
        _totalPlaytime = saveData.TotalPlaytime;
        _levelCompleted = saveData.IsLevelCompleted;
        _currentLevelStars = saveData.LevelStars;
        _totalStars = saveData.TotalStars;
        _currentLevelTime = saveData.LevelTime;

        // Apply player position
        if (saveData.PlayerPosition != Vector3.zero)
        {
            SetPlayerPositionInternal(saveData.PlayerPosition);
        }

        if (showSaveLogs)
        {
            Debug.Log($"Applied save: Level {_currentLevel + 1}, Stars {_totalStars}");
        }
    }

    /// <summary>
    /// Auto-saves to default slot.
    /// </summary>
    private void AutoSaveGame()
    {
        SaveGame(autoSaveSlot);
    }

    #endregion

    #region Player Position (Implement with your player controller)

    private void SetPlayerPositionInternal(Vector3 position)
    {
        // Implement with your player controller
        // Example: playerController.transform.position = position;
        Debug.Log($"Player position set to: {position}");
    }

    private Vector3 GetPlayerPositionInternal()
    {
        // Implement with your player controller
        // Example: return playerController.transform.position;
        return Vector3.zero;
    }

    private Vector4 GetPlayerRotationInternal()
    {
        // Implement with your player controller
        // Example: return new Vector4(playerController.transform.rotation.x, ...);
        return new Vector4(0, 0, 0, 1);
    }

    private void SetPlayerRotationInternal(Vector4 rotation)
    {
        // Implement with your player controller
        Debug.Log($"Player rotation set to: {rotation}");
    }

    #endregion

    #region Callbacks

    private void OnLevelChanged()
    {
        // Implement level change logic
    }

    private void OnDifficultyChanged()
    {
        // Implement difficulty change logic
    }

    #endregion

    #region Getters for SaveManager

    // These are called by SaveManager.CreateCurrentSaveData()
    // They delegate to the actual game state

    private int SaveManager_GetCurrentLevel() => _currentLevel;
    private int SaveManager_GetDifficultyLevel() => _difficultyLevel;
    private Vector3 SaveManager_GetPlayerPosition() => GetPlayerPositionInternal();
    private Vector4 SaveManager_GetPlayerRotation() => GetPlayerRotationInternal();
    private float SaveManager_GetCurrentLevelTime() => _currentLevelTime;
    private float SaveManager_GetTotalPlaytime() => _totalPlaytime;
    private bool SaveManager_IsLevelCompleted() => _levelCompleted;
    private int SaveManager_GetCurrentLevelStars() => _currentLevelStars;
    private int SaveManager_GetTotalStars() => _totalStars;
    private int SaveManager_GetCrowdDensity() => 20; // From GameSettings
    private float SaveManager_GetCurrentSpeed() => 2.5f; // From GameSettings

    private void SaveManager_SetCurrentLevel(int level) => _currentLevel = level;
    private void SaveManager_SetDifficultyLevel(int difficulty) => _difficultyLevel = difficulty;
    private void SaveManager_SetPlayerPosition(Vector3 pos) => SetPlayerPositionInternal(pos);
    private void SaveManager_SetPlayerRotation(Vector4 rot) => SetPlayerRotationInternal(rot);
    private void SaveManager_SetCurrentLevelTime(float time) => _currentLevelTime = time;
    private void SaveManager_SetTotalPlaytime(float time) => _totalPlaytime = time;
    private void SaveManager_SetLevelCompleted(bool completed) => _levelCompleted = completed;
    private void SaveManager_SetCurrentLevelStars(int stars) => _currentLevelStars = stars;
    private void SaveManager_SetTotalStars(int stars) => _totalStars = stars;
    private void SaveManager_SetCrowdDensity(int density) { /* From GameSettings */ }
    private void SaveManager_SetCurrentSpeed(float speed) { /* From GameSettings */ }

    #endregion
}
