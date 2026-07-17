using System;
using UnityEngine;

/// <summary>
/// Data container for game save information.
/// Stores player progress, level state, and metadata.
/// </summary>
[Serializable]
public class SaveData
{
    #region Metadata

    /// <summary>
    /// Unique save file identifier.
    /// </summary>
    public string SaveId { get; set; }

    /// <summary>
    /// User-friendly name for the save file.
    /// </summary>
    public string SaveName { get; set; }

    /// <summary>
    /// Timestamp when the save was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the save was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Display name with formatted date (for UI).
    /// </summary>
    public string DisplayName => $"{SaveName} - {FormattedDate}";

    /// <summary>
    /// Formatted date string for display.
    /// </summary>
    public string FormattedDate => LastModified.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Days since last save (for sorting).
    /// </summary>
    public int DaysSinceLastSave => (DateTime.Now - LastModified).Days;

    #endregion

    #region Player Progress

    /// <summary>
    /// Current level index (0 = first level).
    /// </summary>
    public int CurrentLevel { get; set; }

    /// <summary>
    /// Current difficulty level.
    /// </summary>
    public int DifficultyLevel { get; set; }

    /// <summary>
    /// Player's current position in the level.
    /// </summary>
    public Vector3 PlayerPosition { get; set; }

    /// <summary>
    /// Player's rotation (Quaternion stored as Vector4).
    /// </summary>
    public Vector4 PlayerRotation { get; set; }

    /// <summary>
    /// Time elapsed in current level (seconds).
    /// </summary>
    public float LevelTime { get; set; }

    /// <summary>
    /// Total playtime across all levels (seconds).
    /// </summary>
    public float TotalPlaytime { get; set; }

    #endregion

    #region Game State

    /// <summary>
    /// Whether the current level is completed.
    /// </summary>
    public bool IsLevelCompleted { get; set; }

    /// <summary>
    /// Stars/points earned in current level.
    /// </summary>
    public int LevelStars { get; set; }

    /// <summary>
    /// Total accumulated stars across all levels.
    /// </summary>
    public int TotalStars { get; set; }

    /// <summary>
    /// Custom game settings state.
    /// </summary>
    public int CrowdDensity { get; set; }

    /// <summary>
    /// Custom game settings state.
    /// </summary>
    public float CurrentSpeed { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new save with default values.
    /// </summary>
    public SaveData()
    {
        SaveId = Guid.NewGuid().ToString();
        SaveName = "New Save";
        CreatedAt = DateTime.Now;
        LastModified = DateTime.Now;
        CurrentLevel = 0;
        DifficultyLevel = 0;
        PlayerPosition = Vector3.zero;
        PlayerRotation = new Vector4(0, 0, 0, 1);
        LevelTime = 0f;
        TotalPlaytime = 0f;
        IsLevelCompleted = false;
        LevelStars = 0;
        TotalStars = 0;
        CrowdDensity = 20;
        CurrentSpeed = 2.5f;
    }

    /// <summary>
    /// Creates a save with specific values.
    /// </summary>
    public SaveData(string name, int level, int difficulty)
    {
        SaveId = Guid.NewGuid().ToString();
        SaveName = name;
        CreatedAt = DateTime.Now;
        LastModified = DateTime.Now;
        CurrentLevel = level;
        DifficultyLevel = difficulty;
        PlayerPosition = Vector3.zero;
        PlayerRotation = new Vector4(0, 0, 0, 1);
        LevelTime = 0f;
        TotalPlaytime = 0f;
        IsLevelCompleted = false;
        LevelStars = 0;
        TotalStars = 0;
        CrowdDensity = 20;
        CurrentSpeed = 2.5f;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the last modified timestamp.
    /// </summary>
    public void Touch()
    {
        LastModified = DateTime.Now;
    }

    /// <summary>
    /// Copies data from another SaveData instance.
    /// </summary>
    public void CopyFrom(SaveData other)
    {
        if (other == null) return;

        SaveId = other.SaveId;
        SaveName = other.SaveName;
        CreatedAt = other.CreatedAt;
        LastModified = DateTime.Now;
        CurrentLevel = other.CurrentLevel;
        DifficultyLevel = other.DifficultyLevel;
        PlayerPosition = other.PlayerPosition;
        PlayerRotation = other.PlayerRotation;
        LevelTime = other.LevelTime;
        TotalPlaytime = other.TotalPlaytime;
        IsLevelCompleted = other.IsLevelCompleted;
        LevelStars = other.LevelStars;
        TotalStars = other.TotalStars;
        CrowdDensity = other.CrowdDensity;
        CurrentSpeed = other.CurrentSpeed;
    }

    /// <summary>
    /// Creates a deep copy of this save data.
    /// </summary>
    public SaveData Clone()
    {
        return new SaveData
        {
            SaveId = this.SaveId,
            SaveName = this.SaveName,
            CreatedAt = this.CreatedAt,
            LastModified = this.LastModified,
            CurrentLevel = this.CurrentLevel,
            DifficultyLevel = this.DifficultyLevel,
            PlayerPosition = this.PlayerPosition,
            PlayerRotation = this.PlayerRotation,
            LevelTime = this.LevelTime,
            TotalPlaytime = this.TotalPlaytime,
            IsLevelCompleted = this.IsLevelCompleted,
            LevelStars = this.LevelStars,
            TotalStars = this.TotalStars,
            CrowdDensity = this.CrowdDensity,
            CurrentSpeed = this.CurrentSpeed
        };
    }

    /// <summary>
    /// Generates a user-friendly save name from metadata.
    /// </summary>
    public static string GenerateSaveName(int level, DateTime date)
    {
        return $"Level {level + 1} - {date:dd.MM.yyyy HH.mm}";
    }

    /// <summary>
    /// Debug string representation.
    /// </summary>
    public override string ToString()
    {
        return $"Save[Name={SaveName}, Level={CurrentLevel + 1}, Stars={TotalStars}, Modified={FormattedDate}]";
    }

    #endregion
}
