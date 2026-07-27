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

    private void Awake()
    {
        // Если уже есть инстанс — уничтожаем этот
        if (Instance != null && Instance != this)
        {
            // Проверяем, не является ли это папкой-контейнером
            if (gameObject.name.StartsWith("Folder"))
            {
                // Папки — это контейнеры, компонент на них не должен работать
                Destroy(gameObject);
                return;
            }
            
            Debug.LogWarning($"GameSaveController: Найден дубликат на объекте «{gameObject.name}». Удалён. Должен быть только один инстанс.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Сохраняем между сценами — ищем корневой GameObject
        GameObject rootGO = gameObject;
        while (rootGO.transform.parent != null)
        {
            rootGO = rootGO.transform.parent.gameObject;
        }
        DontDestroyOnLoad(rootGO);
        
        InitializeGameState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Единственный инстанс контроллера (Singleton).
    /// </summary>
    public static GameSaveController Instance { get; private set; }

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
    }

    /// <summary>
    /// Called when level is failed/restarted.
    /// </summary>
    public void OnLevelFailed()
    {
        _levelCompleted = false;
        _currentLevelStars = 0;
        _currentLevelTime = 0f;
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
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Saves current game state to specified slot.
    /// </summary>
    public bool SaveGame(int slotIndex)
    {
        var saveManager = FindFirstObjectByType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
            return false;
        }

        var saveData = saveManager.CreateCurrentSaveData();
        saveData.SaveName = $"Level {_currentLevel + 1} - {DateTime.Now:dd.MM.yyyy HH.mm}";

        bool success = saveManager.SaveGame(slotIndex, saveData);

        return success;
    }

    /// <summary>
    /// Loads game state from specified slot.
    /// </summary>
    public bool LoadGame(int slotIndex)
    {
        var saveManager = FindFirstObjectByType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
            return false;
        }

        var saveData = saveManager.LoadGame(slotIndex);
        if (saveData == null)
        {
            Debug.LogWarning($"No save data found in slot {slotIndex}");
            return false;
        }

        ApplySaveData(saveData);

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

    /// <summary>
    /// Получает позицию игрока.
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return GetPlayerPositionInternal();
    }

    /// <summary>
    /// Получает вращение игрока.
    /// </summary>
    public Vector4 GetPlayerRotation()
    {
        return GetPlayerRotationInternal();
    }

    /// <summary>
    /// Устанавливает позицию игрока.
    /// </summary>
    public void SetPlayerPosition(Vector3 position)
    {
        SetPlayerPositionInternal(position);
    }

    /// <summary>
    /// Устанавливает вращение игрока.
    /// </summary>
    public void SetPlayerRotation(Vector4 rotation)
    {
        SetPlayerRotationInternal(rotation);
    }

    /// <summary>
    /// Устанавливает время текущего уровня.
    /// </summary>
    public void SetCurrentLevelTime(float time)
    {
        _currentLevelTime = time;
    }

    /// <summary>
    /// Устанавливает общее время игры.
    /// </summary>
    public void SetTotalPlaytime(float time)
    {
        _totalPlaytime = time;
    }

    /// <summary>
    /// Устанавливает статус завершения уровня.
    /// </summary>
    public void SetLevelCompleted(bool completed)
    {
        _levelCompleted = completed;
    }

    /// <summary>
    /// Устанавливает звёзды за текущий уровень.
    /// </summary>
    public void SetCurrentLevelStars(int stars)
    {
        _currentLevelStars = stars;
    }

    /// <summary>
    /// Устанавливает общее количество звёзд.
    /// </summary>
    public void SetTotalStars(int stars)
    {
        _totalStars = stars;
    }

    #endregion

    #region Internal Player Methods (for SaveManager compatibility)

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
}
