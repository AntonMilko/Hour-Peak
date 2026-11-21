using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Управляет меню выбора 36 уровней (по 2 части: Утро и Вечер).
/// Создаёт кнопки программно, как SaveSlotSystem.
/// 
/// Логика:
/// - Уровень 1-Утро открыт с начала
/// - После прохождения Утро X → открывается Вечер X
/// - После прохождения Вечер X → открывается Утро (X+1)
/// 
/// Визуал:
/// - Morning: белый Image с чёрными звёздами
/// - Evening: чёрный Image с белыми звёздами
/// - Тени (CanvasGroup.alpha): Locked = 0.4, Unlocked = 1.0
/// </summary>
public class LevelMenuManager : MonoBehaviour
{
    #region Singleton

    public static LevelMenuManager Instance { get; private set; }

    #endregion

    #region Configuration

    [Header("Настройки")]
    [SerializeField] private int totalBaseLevels = 36;
    [SerializeField] private int gridColumns = 6;
    [SerializeField] private int gridRows = 6;
    [SerializeField] private float cellWidth = 100f;
    [SerializeField] private float cellHeight = 80f;
    [SerializeField] private float spacingX = 40f;
    [SerializeField] private float spacingY = 40f;

    [Header("Цвета")]
    [SerializeField] private Color morningLockedColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color eveningLockedColor = new Color(0.25f, 0.25f, 0.25f);

    [Header("Цвета Спрайтов")]
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Sprite star2Sprite;
    [SerializeField] private Color eveningImage = Color.black;
    [SerializeField] private Color morningImage = Color.white;

    [Header("Прозрачность теней")]
    [SerializeField] private float lockedAlpha = 0.4f;
    [SerializeField] private float unlockedAlpha = 1.0f;

    #endregion

    #region Private Fields

    private List<GameObject> _levelButtons = new List<GameObject>();
    private List<Image> _morningImages = new List<Image>();
    private List<Image> _eveningImages = new List<Image>();
    public static int[] _morningStars;
    public static int[] _eveningStars;
    private List<CanvasGroup> _morningCanvasGroups = new List<CanvasGroup>();
    private List<CanvasGroup> _eveningCanvasGroups = new List<CanvasGroup>();
    private List<TheDisplayingOfStars> _morningStarDisplay = new List<TheDisplayingOfStars>();
    private List<TheDisplayingOfStars> _eveningStarDisplay = new List<TheDisplayingOfStars>();

    private Transform _levelPanel;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton — если уже есть экземпляр, уничтожаем этот
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[LevelMenuManager] Найден дубликат LevelMenuManager! Уничтожаем {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region State Update

    /// <summary>
    /// Обновляет визуальное состояние всех кнопок уровней.
    /// </summary>
    public void UpdateAllPartsState()
    {
        for (int i = 0; i < _levelButtons.Count && i < totalBaseLevels; i++)
        {
            UpdateLevelState(i);
        }
        Debug.Log($"[LevelMenuManager] Обновлены {_levelButtons.Count} уровней");
    }

    /// <summary>
    /// Применяет визуальное состояние к Morning или Evening внутри кнопки уровня.
    /// </summary>
    private void UpdateLevelState(int levelIndex)
    {
        bool isMorningUnlocked = IsMorningUnlocked(levelIndex);
        bool isEveningUnlocked = IsEveningUnlocked(levelIndex);

        // Morning
        if (levelIndex < _morningImages.Count)
        {
            _morningImages[levelIndex].color = isMorningUnlocked ? morningImage : morningLockedColor;
        }
        if (levelIndex < _morningCanvasGroups.Count)
        {
            _morningCanvasGroups[levelIndex].alpha = isMorningUnlocked ? unlockedAlpha : lockedAlpha;
            _morningCanvasGroups[levelIndex].blocksRaycasts = isMorningUnlocked;
        }
        // Morning Stars — через TheDisplayingOfStars
        if (levelIndex < _morningStarDisplay.Count && _morningStarDisplay[levelIndex] != null)
        {
            bool morningPassed = _morningStars[levelIndex] > 0;
            _morningStarDisplay[levelIndex].SetBackgroundMode(true);
            _morningStarDisplay[levelIndex].SetLevelResult(morningPassed, _morningStars[levelIndex]);
        }

        // Evening
        if (levelIndex < _eveningImages.Count)
        {
            _eveningImages[levelIndex].color = isEveningUnlocked ? eveningImage : eveningLockedColor;
        }
        if (levelIndex < _eveningCanvasGroups.Count)
        {
            _eveningCanvasGroups[levelIndex].alpha = isEveningUnlocked ? unlockedAlpha : lockedAlpha;
            _eveningCanvasGroups[levelIndex].blocksRaycasts = isEveningUnlocked;
        }
        // Evening Stars — через TheDisplayingOfStars
        if (levelIndex < _eveningStarDisplay.Count && _eveningStarDisplay[levelIndex] != null)
        {
            bool eveningPassed = _eveningStars[levelIndex] > 0;
            _eveningStarDisplay[levelIndex].SetBackgroundMode(false);
            _eveningStarDisplay[levelIndex].SetLevelResult(eveningPassed, _eveningStars[levelIndex]);
        }
    }

    #endregion

    #region Progress Checking

    private bool IsMorningUnlocked(int levelIndex)
    {
        if (levelIndex == 0)
        {
            return true;
        }
        return _eveningStars[levelIndex - 1] > 0;
    }

    private bool IsEveningUnlocked(int levelIndex)
    {
        return _morningStars[levelIndex] > 0;
    }

    #endregion

    #region Level Loading

    private static int _currentBaseLevelIndex = 0;
    private static int _currentPartIndex = 1; // 1 = Morning, 2 = Evening
    private static int _currentSceneIndex = 1;

    public int CurrentBaseLevelIndex => _currentBaseLevelIndex;

    /// <summary>
    /// Возвращает количество сцен для уровня (1 и 19 → 3, остальные → 2).
    /// </summary>
    private static int GetMaxScenes(int baseLevelIndex)
    {
        int level = baseLevelIndex + 1;
        return (level == 1 || level == 19) ? 3 : 2;
    }

/// <summary>
    /// Возвращает имя сцены для уровня, части и номера сцены.
    /// </summary>
    private static string GetSceneName(int baseLevelIndex, bool isEvening, int sceneIndex)
    {
        int level = baseLevelIndex + 1;
        string part = isEvening ? "Evening" : "Morning";
        return $"{part} {level}.{sceneIndex}";
    }

    /// <summary>
    /// Загружает Утро текущего уровня (первая сцена).
    /// </summary>
    public bool LoadLevelMorning(int baseLevelIndex)
    {
        Debug.Log($"[LevelMenuManager] >>> LoadLevelMorning: Уровень {baseLevelIndex + 1}");

        _currentBaseLevelIndex = baseLevelIndex;
        _currentPartIndex = 1;
        _currentSceneIndex = 1;

        LoadLevelScene(baseLevelIndex, false, 1);
        return true;
    }

    /// <summary>
    /// Загружает Вечер текущего уровня (последняя сцена, т.к. вечер идёт в обратном порядке).
    /// </summary>
    public bool LoadLevelEvening(int baseLevelIndex)
    {
        Debug.Log($"[LevelMenuManager] >>> LoadLevelEvening: Уровень {baseLevelIndex + 1}");

        _currentBaseLevelIndex = baseLevelIndex;
        _currentPartIndex = 2;
        _currentSceneIndex = GetMaxScenes(baseLevelIndex);

        LoadLevelScene(baseLevelIndex, true, _currentSceneIndex);
        return true;
    }

    /// <summary>
    /// Загружает сцену уровня по имени.
    /// </summary>
    private static void LoadLevelScene(int baseLevelIndex, bool isEvening, int sceneIndex)
    {
        Time.timeScale = 1f;

        string sceneName = GetSceneName(baseLevelIndex, isEvening, sceneIndex);
        Debug.Log($"[LevelMenuManager] >>> SceneManager.LoadScene('{sceneName}')");

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Статический метод перехода к следующей сцене/части/уровню.
    /// Вызывается из EndManager2 после сохранения прогресса.
    /// </summary>
    public static IEnumerator LoadNextLevelPartStatic()
    {
        Time.timeScale = 1f;

        bool isEvening = _currentPartIndex >= 2;
        int maxScenes = GetMaxScenes(_currentBaseLevelIndex);

        // Для Evening последняя сцена — когда _currentSceneIndex <= 1
        bool isLastSceneInPart = isEvening
            ? _currentSceneIndex <= 1
            : _currentSceneIndex >= maxScenes;

        bool isLastLevel = _currentBaseLevelIndex >= 35; // 36 уровней (0-based)

        // Случай 4: Evening уровня 36 завершён → сертификат
        if (isEvening && isLastSceneInPart && isLastLevel)
        {
            Debug.Log("🏆 Все уровни пройдены! Переход к сертификату...");
            SceneManager.LoadSceneAsync("Game completion certificate");
            yield break;
        }

        // Случай 3: Evening завершён (не последний уровень) → Morning следующего уровня
        if (isEvening && isLastSceneInPart)
        {
            int nextLevel = _currentBaseLevelIndex + 1;
            _currentBaseLevelIndex = nextLevel;
            _currentPartIndex = 1;
            _currentSceneIndex = 1;
            string sceneName = GetSceneName(nextLevel, false, 1);
            Debug.Log($"🔄 Переход: Уровень {nextLevel + 1} Morning → {sceneName}");
            SceneManager.LoadSceneAsync(sceneName);
            yield break;
        }

        // Случай 2: Morning завершена (последняя сцена) → Evening того же уровня
        if (!isEvening && isLastSceneInPart)
        {
            _currentPartIndex = 2;
            _currentSceneIndex = maxScenes;
            string sceneName = GetSceneName(_currentBaseLevelIndex, true, _currentSceneIndex);
            Debug.Log($"🔄 Переход: Уровень {_currentBaseLevelIndex + 1} Evening → {sceneName}");
            SceneManager.LoadSceneAsync(sceneName);
            yield break;
        }

        // Случай 1: Следующая сцена в той же части
        // Для Evening — уменьшаем (обратный порядок), для Morning — увеличиваем
        _currentSceneIndex += isEvening ? -1 : 1;
        string partName = isEvening ? "Evening" : "Morning";
        string nextScene = GetSceneName(_currentBaseLevelIndex, isEvening, _currentSceneIndex);
        Debug.Log($"🔄 Переход: Уровень {_currentBaseLevelIndex + 1} {partName} сцена {_currentSceneIndex}");
        SceneManager.LoadSceneAsync(nextScene);
    }

    /// <summary>
    /// Загружает сцену сертификата.
    /// </summary>
    public void LoadCertificateScene()
    {
        Debug.Log("🏆 Загрузка сертификата: Game completion certificate");
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("Game completion certificate");
    }

    /// <summary>
    /// Загружает сцену StartMenu.
    /// </summary>
    public void LoadStartMenuScene()
    {
        Debug.Log("📺 Загрузка StartMenu");
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("StartMenu");
    }

    /// <summary>
    /// Вызывается при клике на Morning части уровня.
    /// </summary>
    public void OnMorningClicked(int baseLevelIndex)
    {
        Debug.Log($"[LevelMenuManager] >>> КЛИК Morning: Уровень {baseLevelIndex + 1}");
        LoadLevelMorning(baseLevelIndex);
    }

    /// <summary>
    /// Вызывается при клике на Evening части уровня.
    /// </summary>
    public void OnEveningClicked(int baseLevelIndex)
    {
        Debug.Log($"[LevelMenuManager] >>> КЛИК Evening: Уровень {baseLevelIndex + 1}");
        LoadLevelEvening(baseLevelIndex);
    }

    /// <summary>
    /// Вызывается при клике на кнопку уровня.
    /// </summary>
    public void OnLevelClicked(int levelIndex)
    {
        Debug.Log($"[LevelMenuManager] Нажат уровень {levelIndex + 1}");
        LoadLevelMorning(levelIndex);
    }

    #endregion
}
