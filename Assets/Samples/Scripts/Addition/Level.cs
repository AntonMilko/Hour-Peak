using System;
using UnityEngine;
using UnityEngine.UI;
using HourPeak.Levels;

/// <summary>
/// Level controller - simplified version without LevelConfig dependency.
/// </summary>
public class Level : MonoBehaviour
{
    #region Configuration

    [Header("Level Settings")]
    [Tooltip("Unique level identifier (0-based index)")]
    [SerializeField] private int levelIndex = 0;

    [Tooltip("Level name for display")]
    [SerializeField] private string levelName = "Level 1";

    [Header("Visual Appearance")]
    [Tooltip("Image для применения визуального состояния")]
    [SerializeField] private Image levelImage;

    [Tooltip("Прозрачность заблокированного уровня (0..1)")]
    [SerializeField] private float blockedAlpha = 0.3f;

    [Header("Morning / Evening Variants")]
    [Tooltip("Image для утренней версии (переопределяет levelImage)")]
    [SerializeField] private Image morningImage;

    [Tooltip("Прозрачность заблокированного утра (0..1)")]
    [SerializeField] private float morningBlockedAlpha = 0.3f;

    [Tooltip("Image для вечерней версии (переопределяет levelImage)")]
    [SerializeField] private Image eveningImage;

    [Tooltip("Прозрачность заблокированного вечера (0..1)")]
    [SerializeField] private float eveningBlockedAlpha = 0.3f;

    #endregion

    #region Private State

    private Color originalColor;
    private Color morningOriginalColor;
    private Color eveningOriginalColor;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (levelImage != null)
        {
            originalColor = levelImage.color;
        }

        if (morningImage != null)
        {
            morningOriginalColor = morningImage.color;
        }

        if (eveningImage != null)
        {
            eveningOriginalColor = eveningImage.color;
        }

        ApplyVisualState();
    }

    private void OnEnable()
    {
        ApplyVisualState();
    }

    #endregion

    #region Visual State

    /// <summary>
    /// Применяет визуальное состояние на основе прогресса.
    /// - Доступные уровни — нормальная прозрачность.
    /// - Заблокированные уровни — затемнение (полупрозрачность).
    /// </summary>
    public void ApplyVisualState()
    {
        // Morning variant
        if (morningImage != null)
        {
            bool morningUnlocked = levelIndex == 0 || LevelManager.Instance.IsLevelFullyCompleted(levelIndex - 1);
            
            Color color = morningUnlocked ? morningOriginalColor : new Color(morningOriginalColor.r, morningOriginalColor.g, morningOriginalColor.b, morningBlockedAlpha);
            morningImage.color = color;
        }
        else if (levelImage != null)
        {
            bool isAccessible = LevelManager.Instance.CanAccessBaseLevel(levelIndex);
            Color color = isAccessible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, blockedAlpha);
            levelImage.color = color;
        }

        // Evening variant
        if (eveningImage != null)
        {
            bool eveningUnlocked = LevelManager.Instance.IsLevelFullyCompleted(levelIndex);
            
            Color color = eveningUnlocked ? eveningOriginalColor : new Color(eveningOriginalColor.r, eveningOriginalColor.g, eveningOriginalColor.b, eveningBlockedAlpha);
            eveningImage.color = color;
        }
    }

    /// <summary>
    /// Применяет визуальное состояние для конкретного времени суток.
    /// </summary>
    public void ApplyVisualState(TimeOfDay timeOfDay)
    {
        if (timeOfDay == TimeOfDay.Morning && morningImage != null)
        {
            bool morningUnlocked = levelIndex == 0 || LevelManager.Instance.IsLevelFullyCompleted(levelIndex - 1);
            Color color = morningUnlocked ? morningOriginalColor : new Color(morningOriginalColor.r, morningOriginalColor.g, morningOriginalColor.b, morningBlockedAlpha);
            morningImage.color = color;
        }
        else if (timeOfDay == TimeOfDay.Evening && eveningImage != null)
        {
            bool eveningUnlocked = LevelManager.Instance.IsLevelFullyCompleted(levelIndex);
            Color color = eveningUnlocked ? eveningOriginalColor : new Color(eveningOriginalColor.r, eveningOriginalColor.g, eveningOriginalColor.b, eveningBlockedAlpha);
            eveningImage.color = color;
        }
        else if (levelImage != null)
        {
            bool isAccessible = LevelManager.Instance.CanAccessBaseLevel(levelIndex);
            Color color = isAccessible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, blockedAlpha);
            levelImage.color = color;
        }
    }

    /// <summary>
    /// Доступен ли уровень для игры.
    /// </summary>
    public bool IsAccessible()
    {
        return LevelManager.Instance.CanAccessBaseLevel(levelIndex);
    }

    /// <summary>
    /// Пройден ли полностью уровень (утро + вечер).
    /// </summary>
    public bool IsFullyCompleted()
    {
        return LevelManager.Instance.IsLevelFullyCompleted(levelIndex);
    }

    /// <summary>
    /// Возвращает Image для указанного времени суток.
    /// </summary>
    public Image GetImage(TimeOfDay timeOfDay)
    {
        return timeOfDay == TimeOfDay.Morning ? morningImage : eveningImage;
    }

    #endregion

    #region Level Loading

    /// <summary>
    /// Запускает утреннюю версию уровня.
    /// </summary>
    public void StartMorning()
    {
        LevelManager.Instance.LoadLevelWithTime(levelIndex, TimeOfDay.Morning);
    }

    /// <summary>
    /// Запускает вечернюю версию уровня.
    /// </summary>
    public void StartEvening()
    {
        LevelManager.Instance.LoadLevelWithTime(levelIndex, TimeOfDay.Evening);
    }

    /// <summary>
    /// Запускает уровень с указанным временем суток.
    /// </summary>
    public void StartLevel(TimeOfDay timeOfDay)
    {
        LevelManager.Instance.LoadLevelWithTime(levelIndex, timeOfDay);
    }

    /// <summary>
    /// Запускает уровень с автоматическим временем суток.
    /// </summary>
    public void StartLevelAuto()
    {
        LevelManager.Instance.LoadLevel(levelIndex);
    }

    /// <summary>
    /// Запускает следующую часть (Утро → Вечер → следующий уровень).
    /// </summary>
    public void StartNextPart()
    {
        LevelManager.Instance.LoadNextLevelPart();
    }

    #endregion

    #region Progress Management

    public bool IsMorningCompleted()
    {
        return LevelManager.Instance.IsMorningCompleted(levelIndex);
    }

    public bool IsEveningCompleted()
    {
        return LevelManager.Instance.IsEveningCompleted(levelIndex);
    }

    public bool IsLevelFullyCompleted()
    {
        return LevelManager.Instance.IsLevelFullyCompleted(levelIndex);
    }

    public int GetMorningStars()
    {
        return LevelManager.Instance.GetMorningStars(levelIndex);
    }

    public int GetEveningStars()
    {
        return LevelManager.Instance.GetEveningStars(levelIndex);
    }

    public int GetTotalStars()
    {
        return LevelManager.Instance.GetTotalStars(levelIndex);
    }

    #endregion

    #region Reset Progress

    public void ResetAllProgress()
    {
        LevelManager.Instance.ResetLevelProgress(levelIndex);
        Debug.Log("Progress reset for level " + levelIndex + " - " + levelName);
    }

    public void ResetMorningProgress()
    {
        LevelManager.Instance.ResetVariantProgress(levelIndex, TimeOfDay.Morning);
    }

    public void ResetEveningProgress()
    {
        LevelManager.Instance.ResetVariantProgress(levelIndex, TimeOfDay.Evening);
    }

    #endregion

    #region Completion

    public void CompleteLevel(bool success, int stars, bool perfectTiming)
    {
        LevelManager.Instance.CompleteLevel(success, stars, perfectTiming);
        Debug.Log($"{(success ? "✅" : "❌")} {levelName} | {stars}/3 ★");

        // Обновляем визуальное состояние после завершения
        ApplyVisualState(LevelManager.Instance.CurrentTimeOfDay);
    }

    #endregion

    #region Debug

    public void DebugLogProgress()
    {
        Debug.Log("=== Level " + levelIndex + " - " + levelName + " ===");
        Debug.Log("Morning: " + (IsMorningCompleted() ? "Completed" : "Not Started") + " | Stars: " + GetMorningStars() + "/3");
        Debug.Log("Evening: " + (IsEveningCompleted() ? "Completed" : "Not Started") + " | Stars: " + GetEveningStars() + "/3");
        Debug.Log("Total Stars: " + GetTotalStars() + "/6 | Fully Completed: " + IsLevelFullyCompleted());
    }

    #endregion
}