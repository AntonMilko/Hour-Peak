using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using HourPeak.Levels;
using HourPeak.Settings;

/// <summary>
/// Manages game save/load operations with file persistence.
/// Supports multiple save slots with DateTime metadata.
/// </summary>
public static class SaveManager
{
    #region Configuration

    /// <summary>
    /// Maximum number of save slots available.
    /// </summary>
    private const int maxSaveSlots = 20;

    private static int slotIndex = -1;

    /// <summary>
    /// Base directory for save files (relative to Application.persistentDataPath).
    /// </summary>
    private const string saveDirectoryName = "HourPeakSaves";

    /// <summary>
    /// File extension for save files.
    /// </summary>
    private const string fileExtension = ".json";

    #endregion

    #region Initialization

    static SaveManager()
    {
        string _saveDirectoryPath = Path.Combine(Application.persistentDataPath, saveDirectoryName);
        if (!Directory.Exists(_saveDirectoryPath))
        {
            Directory.CreateDirectory(_saveDirectoryPath);
        }
    }

    #endregion

    #region Save Operations

    /// <summary>
    /// Сохраняет данные игры в указанный слот.
    /// </summary>
    /// <param name="slotIndex">Индекс слота (от 0 до MaxSaveSlots-1).</param>
    /// <param name="saveData">Данные для сохранения.</param>
    /// <returns>True, если сохранение прошло успешно.</returns>
    public static bool SaveGame()
    {
        try
        {
            SaveData saveData = new SaveData();
            
            // Update metadata
            saveData.LastModified = DateTime.Now;
            saveData.DifficultyLevel = (int)SettingDifficulty.CurrentDifficulty;

            // Save passed level
            saveData.MorningStars = LevelMenuManager._morningStars;
            saveData.EveningStars = LevelMenuManager._eveningStars;

            // Serialize to JSON
            string json = JsonUtility.ToJson(saveData, true);

            // Write to file
            string filePath = GetSaveFilePath(slotIndex);
            File.WriteAllText(filePath, json);
            Debug. Log(filePath);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Load Operations

    /// <summary>
    /// Загружает данные игры из указанного слота.
    /// </summary>
    /// <param name="slotIndex">Индекс слота (от 0 до MaxSaveSlots-1).</param>
    /// <returns>Загруженные данные или null, если не удалось.</returns>
    public static SaveData LoadGame(int slotIndex)
    {
        try
        {
            string filePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file found at slot {slotIndex}.");
                return null;
            }

            // Read and deserialize
            string json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogError($"Failed to parse save data from slot {slotIndex}.");
                return null;
            }

            ApplySaveData(saveData);
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game from slot {slotIndex}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Applies loaded save data to current game state.
    /// </summary>
    private static void ApplySaveData(SaveData saveData)
    {
        SettingDifficulty.CurrentDifficulty = (DifficultyLevel)saveData.DifficultyLevel;

        // Импортируем прогресс Hour-Peak в LevelMenuManager
        
        LevelMenuManager._morningStars = saveData.MorningStars;
        LevelMenuManager._eveningStars = saveData.EveningStars;
    }

    #endregion

    #region Save Management

    /// <summary>
    /// Gets file path for a slot.
    /// </summary>
    private static string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, saveDirectoryName, $"save_{slotIndex:00}{fileExtension}");
    }

    /// <summary>
    /// Обновляет DateTimeText в UI для указанного слота при сохранении.
    /// </summary>
    private static void UpdateSlotDateTimeUI(int slotIndex)
    {
        try
        {
            // Ищем SaveSlotSystem на сцене
            var saveSlotSystem = GameObject.FindFirstObjectByType<SaveSlotSystem>();
            if (saveSlotSystem == null)
            {
                Debug.LogWarning("[SaveManager] SaveSlotSystem не найден — DateTimeText не обновлён.");
                return;
            }

            // Форматируем дату и время: dd.MM.yyyy HH:mm:ss
            string formatted = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            saveSlotSystem.UpdateDateTimeText(slotIndex, formatted);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка обновления DateTimeText для слота {slotIndex}: {e.Message}");
        }
    }

    #endregion
}