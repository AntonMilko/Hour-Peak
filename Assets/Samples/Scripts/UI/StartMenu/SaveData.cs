using System;
using HourPeak.Settings;

/// <summary>
/// Data container for game save information.
/// Stores player progress, level state, and metadata.
/// </summary>
[Serializable]
public class SaveData
{
    #region Metadata

    /// <summary>
    /// Timestamp when the save was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Formatted date string for display.
    /// </summary>
    public string FormattedDate => LastModified.ToString("dd.MM.yyyy HH:mm");

    #endregion

    #region Player Progress

    /// <summary>
    /// Current difficulty level.
    /// </summary>
    public int DifficultyLevel { get; set; }

    #endregion

    #region Hour-Peak Progress (36 уровней × 2 части)

    /// <summary>
    /// Звёзды за утренние части (индекс = базовый уровень 0-35, значения 0-3).
    /// </summary>
    public int[] MorningStars { get; set; }

    /// <summary>
    /// Звёзды за вечерние части (индекс = базовый уровень 0-35, значения 0-3).
    /// </summary>
    public int[] EveningStars { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Инициализирует массивы прогресса Hour-Peak (36 уровней).
    /// </summary>
    private void InitializeHourPeakProgress()
    {
        MorningStars = new int[36];
        EveningStars = new int[36];
    }

    /// <summary>
    /// Creates a new save with default values.
    /// </summary>
    public SaveData()
    {
        InitializeHourPeakProgress();
    }

    #endregion
}
