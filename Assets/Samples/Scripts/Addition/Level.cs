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
    private LevelManager levelManager;

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
        levelManager = GetComponent<LevelManager>();
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
            bool morningUnlocked = levelIndex == 0 || levelManager.IsLevelFullyCompleted(levelIndex - 1);
            
            Color color = morningUnlocked ? morningOriginalColor : new Color(morningOriginalColor.r, morningOriginalColor.g, morningOriginalColor.b, morningBlockedAlpha);
            morningImage.color = color;
        }
        else if (levelImage != null)
        {
            bool isAccessible = levelManager.CanAccessBaseLevel(levelIndex);
            Color color = isAccessible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, blockedAlpha);
            levelImage.color = color;
        }

        // Evening variant
        if (eveningImage != null)
        {
            bool eveningUnlocked = levelManager.IsLevelFullyCompleted(levelIndex);
            
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
            bool morningUnlocked = levelIndex == 0 || levelManager.IsLevelFullyCompleted(levelIndex - 1);
            Color color = morningUnlocked ? morningOriginalColor : new Color(morningOriginalColor.r, morningOriginalColor.g, morningOriginalColor.b, morningBlockedAlpha);
            morningImage.color = color;
        }
        else if (timeOfDay == TimeOfDay.Evening && eveningImage != null)
        {
            bool eveningUnlocked = levelManager.IsLevelFullyCompleted(levelIndex);
            Color color = eveningUnlocked ? eveningOriginalColor : new Color(eveningOriginalColor.r, eveningOriginalColor.g, eveningOriginalColor.b, eveningBlockedAlpha);
            eveningImage.color = color;
        }
        else if (levelImage != null)
        {
            bool isAccessible = levelManager.CanAccessBaseLevel(levelIndex);
            Color color = isAccessible ? originalColor : new Color(originalColor.r, originalColor.g, originalColor.b, blockedAlpha);
            levelImage.color = color;
        }
    }

    /// <summary>
    /// Доступен ли уровень для игры.
    /// </summary>
    public bool IsAccessible()
    {
        return levelManager.CanAccessBaseLevel(levelIndex);
    }

    /// <summary>
    /// Пройден ли полностью уровень (утро + вечер).
    /// </summary>
    public bool IsFullyCompleted()
    {
        return levelManager.IsLevelFullyCompleted(levelIndex);
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
        levelManager.StartCoroutine(levelManager.LoadLevelWithTimeAsync(levelIndex, TimeOfDay.Morning));
    }

    /// <summary>
    /// Запускает вечернюю версию уровня.
    /// </summary>
    public void StartEvening()
    {
        levelManager.StartCoroutine(levelManager.LoadLevelWithTimeAsync(levelIndex, TimeOfDay.Evening));
    }

    /// <summary>
    /// Запускает уровень с указанным временем суток.
    /// </summary>
    public void StartLevel(TimeOfDay timeOfDay)
    {
        levelManager.StartCoroutine(levelManager.LoadLevelWithTimeAsync(levelIndex, timeOfDay));
    }

    /// <summary>
    /// Запускает уровень с автоматическим временем суток.
    /// </summary>
    public void StartLevelAuto()
    {
        levelManager.StartCoroutine(levelManager.LoadLevelAsync(levelIndex));
    }

    /// <summary>
    /// Запускает следующую часть (Утро → Вечер → следующий уровень).
    /// </summary>
    public void StartNextPart()
    {
        levelManager.StartCoroutine(levelManager.LoadNextLevelPartAsync());
    }

    #endregion

    #region Progress Management

    public bool IsMorningCompleted()
    {
        return levelManager.IsMorningCompleted(levelIndex);
    }

    public bool IsEveningCompleted()
    {
        return levelManager.IsEveningCompleted(levelIndex);
    }

    public bool IsLevelFullyCompleted()
    {
        return levelManager.IsLevelFullyCompleted(levelIndex);
    }

    public int GetMorningStars()
    {
        return levelManager.GetMorningStars(levelIndex);
    }

    public int GetEveningStars()
    {
        return levelManager.GetEveningStars(levelIndex);
    }

    public int GetTotalStars()
    {
        return levelManager.GetTotalStars(levelIndex);
    }

    #endregion

    #region Reset Progress

    public void ResetAllProgress()
    {
        levelManager.ResetLevelProgress(levelIndex);
        Debug.Log("Progress reset for level " + levelIndex + " - " + levelName);
    }

    public void ResetMorningProgress()
    {
        levelManager.ResetVariantProgress(levelIndex, TimeOfDay.Morning);
    }

    public void ResetEveningProgress()
    {
        levelManager.ResetVariantProgress(levelIndex, TimeOfDay.Evening);
    }

    #endregion

    #region Completion

    public void CompleteLevel(bool success, int stars, bool perfectTiming)
    {
        levelManager.CompleteLevel(success, stars, perfectTiming);
        Debug.Log($"{(success ? "✅" : "❌")} {levelName} | {stars}/3 ★");

        // Обновляем визуальное состояние после завершения
        ApplyVisualState(levelManager.CurrentTimeOfDay);
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