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

    #region Hour-Peak Progress (36 уровней × 2 части)

    /// <summary>
    /// Прогресс утренних частей (индекс = базовый уровень 0-35).
    /// true = утро уровня пройдено.
    /// </summary>
    public bool[] MorningCompleted { get; set; }

    /// <summary>
    /// Прогресс вечерних частей (индекс = базовый уровень 0-35).
    /// true = вечер уровня пройдено.
    /// </summary>
    public bool[] EveningCompleted { get; set; }

    /// <summary>
    /// Звёзды за утренние части (индекс = базовый уровень 0-35, значения 0-3).
    /// </summary>
    public int[] MorningStars { get; set; }

    /// <summary>
    /// Звёзды за вечерние части (индекс = базовый уровень 0-35, значения 0-3).
    /// </summary>
    public int[] EveningStars { get; set; }

    /// <summary>
    /// Время прохождения утренних частей (индекс = базовый уровень 0-35).
    /// </summary>
    public float[] MorningTimes { get; set; }

    /// <summary>
    /// Время прохождения вечерних частей (индекс = базовый уровень 0-35).
    /// </summary>
    public float[] EveningTimes { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Инициализирует массивы прогресса Hour-Peak (36 уровней).
    /// </summary>
    private void InitializeHourPeakProgress()
    {
        MorningCompleted = new bool[36];
        EveningCompleted = new bool[36];
        MorningStars = new int[36];
        EveningStars = new int[36];
        MorningTimes = new float[36];
        EveningTimes = new float[36];
    }

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
        InitializeHourPeakProgress();
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
        InitializeHourPeakProgress();
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

        // Copy Hour-Peak progress arrays
        if (other.MorningCompleted != null)
            System.Array.Copy(other.MorningCompleted, MorningCompleted, 36);
        if (other.EveningCompleted != null)
            System.Array.Copy(other.EveningCompleted, EveningCompleted, 36);
        if (other.MorningStars != null)
            System.Array.Copy(other.MorningStars, MorningStars, 36);
        if (other.EveningStars != null)
            System.Array.Copy(other.EveningStars, EveningStars, 36);
        if (other.MorningTimes != null)
            System.Array.Copy(other.MorningTimes, MorningTimes, 36);
        if (other.EveningTimes != null)
            System.Array.Copy(other.EveningTimes, EveningTimes, 36);
    }

    /// <summary>
    /// Creates a deep copy of this save data.
    /// </summary>
    public SaveData Clone()
    {
        var clone = new SaveData
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

        // Clone Hour-Peak progress arrays
        if (this.MorningCompleted != null)
            clone.MorningCompleted = (bool[])this.MorningCompleted.Clone();
        if (this.EveningCompleted != null)
            clone.EveningCompleted = (bool[])this.EveningCompleted.Clone();
        if (this.MorningStars != null)
            clone.MorningStars = (int[])this.MorningStars.Clone();
        if (this.EveningStars != null)
            clone.EveningStars = (int[])this.EveningStars.Clone();
        if (this.MorningTimes != null)
            clone.MorningTimes = (float[])this.MorningTimes.Clone();
        if (this.EveningTimes != null)
            clone.EveningTimes = (float[])this.EveningTimes.Clone();

        return clone;
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
