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

    // Статический прогресс — живёт между сценами
    private static Dictionary<int, Dictionary<int, int>> _sharedProgress = new Dictionary<int, Dictionary<int, int>>();

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
    private List<List<Image>> _morningStars = new List<List<Image>>();
    private List<List<Image>> _eveningStars = new List<List<Image>>();
    private List<TextMeshProUGUI> _levelNumbers = new List<TextMeshProUGUI>();
    private List<CanvasGroup> _morningCanvasGroups = new List<CanvasGroup>();
    private List<CanvasGroup> _eveningCanvasGroups = new List<CanvasGroup>();
    private List<TheDisplayingOfStars> _morningStarDisplay = new List<TheDisplayingOfStars>();
    private List<TheDisplayingOfStars> _eveningStarDisplay = new List<TheDisplayingOfStars>();

    private Transform _levelPanel;

    // Прогресс: [levelIndex][partIndex] = stars (0-3) — ссылка на статические данные
    private Dictionary<int, Dictionary<int, int>> _progress;

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
        
        Debug.Log($"[LevelMenuManager] Awake: {gameObject.name}");
        
        // Используем статический прогресс (сохраняется между сценами)
        _progress = _sharedProgress;
        InitializeProgress();
        FindLevelPanel();
    }

    private void Start()
    {
        StartCoroutine(SpawnAllButtonsCoroutine());
    }

    private System.Collections.IEnumerator SpawnAllButtonsCoroutine()
    {
        yield return null; // Ждём один кадр, чтобы Layout обновился
        SpawnAllButtons();
    }

    /// <summary>
    /// Инициализирует прогресс для всех уровней.
    /// </summary>
    private void InitializeProgress()
    {
        for (int i = 0; i < totalBaseLevels; i++)
        {
            if (!_progress.ContainsKey(i))
            {
                _progress[i] = new Dictionary<int, int>();
                _progress[i][0] = 0; // Morning
                _progress[i][1] = 0; // Evening
            }
        }

        Debug.Log($"✅ LevelMenuManager инициализирован с {totalBaseLevels} базовыми уровнями ({totalBaseLevels * 2} частями)");
    }

    /// <summary>
    /// Ищет LevelPanel в иерархии.
    /// </summary>
    private void FindLevelPanel()
    {
        _levelPanel = transform.root.Find("LevelMenu/LevelPanel");
        if (_levelPanel == null)
        {
            _levelPanel = transform.root.Find("LevelPanel");
        }
        if (_levelPanel == null)
        {
            Debug.LogError("[LevelMenuManager] LevelPanel не найден!");
            return;
        }
        
        Debug.Log($"[LevelMenuManager] Найден LevelPanel: {_levelPanel.name}");
    }

    /// <summary>
    /// Вызывается извне для обновления состояния (например, после возврата из игры).
    /// </summary>
    public void Refresh()
    {
        UpdateAllPartsState();
    }

    #endregion

    #region Spawn All Buttons

    // ========================================================================
    // ГЛАВНЫЙ МЕТОД: СОЗДАНИЕ ВСЕХ 36 КНОПОК УРОВНЕЙ
    // ========================================================================

    [ContextMenu("🔥 Spawn All Buttons")]
    public void SpawnAllButtons()
    {
        // LevelPanel уже найден в Awake()
        if (_levelPanel == null)
        {
            Debug.LogError("[LevelMenuManager] LevelPanel не найден! Вызови FindLevelPanel() сначала.");
            return;
        }
        
        Debug.Log($"[LevelMenuManager] LevelPanel: {_levelPanel.name}");

        _levelButtons.Clear();
        _morningImages.Clear();
        _eveningImages.Clear();
        _morningStars.Clear();
        _eveningStars.Clear();
        _levelNumbers.Clear();
        _morningCanvasGroups.Clear();
        _eveningCanvasGroups.Clear();
        _morningStarDisplay.Clear();
        _eveningStarDisplay.Clear();

        // Удаляем старые кнопки из LevelPanel (собираем в список, потом удаляем)
        List<GameObject> toDelete = new List<GameObject>();
        foreach (Transform child in _levelPanel)
        {
            if (child.name.StartsWith("Level ", System.StringComparison.OrdinalIgnoreCase))
            {
                toDelete.Add(child.gameObject);
            }
        }
        foreach (GameObject go in toDelete)
        {
            DestroyImmediate(go);
        }

        // НЕ удаляем GameObject — просто пересоздаём кнопки заново
        // Старые GameObject останутся, но мы будем использовать новые из _levelButtons
        Debug.Log($"[LevelMenuManager] Пересоздание {totalBaseLevels} КНОПОК...");
        int createdCount = 0;
        int failedCount = 0;

        for (int i = 0; i < totalBaseLevels; i++)
        {
            bool success = CreateSingleLevel(i);
            
            if (success)
            {
                createdCount++;
            }
            else
            {
                failedCount++;
            }
        }

        // 📊 ОТЧЁТ ПО СТАТУСУ
        Debug.Log($"[LevelMenuManager] ========================================");
        Debug.Log($"[LevelMenuManager] Total levels: {totalBaseLevels}");
        Debug.Log($"[LevelMenuManager] Created levels: {createdCount}");
        Debug.Log($"[LevelMenuManager] Failed levels: {failedCount}");
        Debug.Log($"[LevelMenuManager] Render check: {(failedCount == 0 ? "PASSED" : "FAILED")}");
        Debug.Log($"[LevelMenuManager] ========================================");

        // Если есть ошибки — делаем скриншот
        if (failedCount > 0)
        {
            Debug.LogError($"[LevelMenuManager] КРИТИЧЕСКАЯ ОШИБКА: {failedCount} кнопок не созданы!");
#if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot("level_button_error.png");
#endif
            throw new System.Exception($"Failed to create {failedCount} out of {totalBaseLevels} level buttons!");
        }

        // 💥 УСИЛЕННАЯ ЛОГИКА: финальная проверка
        Debug.Assert(_levelButtons.Count == totalBaseLevels, "Critical failure: level button count mismatch!");
        
        // Валидация: все кнопки в списке
        Debug.Assert(_levelButtons.Count == 36, "Wrong level button count!");
        
        // Ждём один кадр, чтобы Layout обновился, затем размещаем кнопки в сетке
        StartCoroutine(ArrangeAfterDelay());
    }

    private System.Collections.IEnumerator ArrangeAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        ArrangeButtonsInGrid();
        UpdateAllPartsState();
        Debug.Log($"[LevelMenuManager] Итого {_levelButtons.Count} объектов, {_morningImages.Count} Morning Image, {_eveningImages.Count} Evening Image");
    }

    // ========================================================================
    // СОЗДАНИЕ ОДНОЙ КНОПКИ УРОВНЯ С ПОЛНОЙ ВАЛИДАЦИЕЙ
    // ========================================================================

    private bool CreateSingleLevel(int index)
    {
        string levelName = $"Level {index + 1}";
        
        // Создаём GameObject
        GameObject levelGO = new GameObject(levelName);
        
        // Проверяем, что объект создан
        if (levelGO == null)
        {
            Debug.LogError($"[LevelMenuManager] [ERROR] Level {index} не создан!");
            return false;
        }

        // Добавляем в иерархию — прямой ребёнок LevelPanel
        levelGO.transform.SetParent(_levelPanel, false);
        levelGO.transform.localScale = Vector3.one;
        levelGO.transform.localRotation = Quaternion.identity;
        levelGO.SetActive(true);
        
        Debug.Log($"[CreateSingleLevel] Кнопка #{index} создана. Родитель: {_levelPanel.name}, childCount родителя: {_levelPanel.childCount}");

        // Получаем или создаём RectTransform для levelGO
        RectTransform levelRT = levelGO.GetComponent<RectTransform>();
        if (levelRT == null)
        {
            levelRT = levelGO.AddComponent<RectTransform>();
        }
        levelRT.anchorMin = new Vector2(0.5f, 0.5f);
        levelRT.anchorMax = new Vector2(0.5f, 0.5f);
        levelRT.pivot = new Vector2(0.5f, 0.5f);
        levelRT.sizeDelta = new Vector2(cellWidth, cellHeight);

        // Morning Image (белый)
        GameObject morningGO = new GameObject("Morning");
        morningGO.transform.SetParent(levelGO.transform, false);
        RectTransform morningRT = morningGO.GetComponent<RectTransform>();
        if (morningRT == null)
        {
            morningRT = morningGO.AddComponent<RectTransform>();
        }
        morningRT.anchorMin = new Vector2(0f, 0.5f);
        morningRT.anchorMax = new Vector2(1f, 1f);
        morningRT.offsetMin = new Vector2(2f, 2f);
        morningRT.offsetMax = new Vector2(-2f, -2f);

        // CanvasGroup для Morning — тени
        CanvasGroup morningCanvasGroup = morningGO.GetComponent<CanvasGroup>();
        if (morningCanvasGroup == null)
        {
            morningCanvasGroup = morningGO.AddComponent<CanvasGroup>();
        }
        morningCanvasGroup.alpha = lockedAlpha;
        morningCanvasGroup.blocksRaycasts = true;
        _morningCanvasGroups.Add(morningCanvasGroup);

        Image morningImg = morningGO.GetComponent<Image>();
        if (morningImg == null)
        {
            morningImg = morningGO.AddComponent<Image>();
        }
        morningImg.sprite = null;
        morningImg.color = morningImage;
        morningImg.raycastTarget = true;
        _morningImages.Add(morningImg);

        // Button на Morning — обработка клика
        Button morningBtn = morningGO.GetComponent<Button>();
        if (morningBtn == null) morningBtn = morningGO.AddComponent<Button>();
        morningBtn.targetGraphic = morningImg;
        int morningLevelIndex = index;
        morningBtn.onClick.AddListener(() => OnMorningClicked(morningLevelIndex));

// Morning Stars (3 чёрные) — с TheDisplayingOfStars
        List<Image> morningStars = new List<Image>();
        GameObject morningStarsParent = new GameObject("Stars");
        morningStarsParent.transform.SetParent(morningGO.transform, false);
        RectTransform morningStarsRT = morningStarsParent.GetComponent<RectTransform>();
        if (morningStarsRT == null) morningStarsRT = morningStarsParent.AddComponent<RectTransform>();
        morningStarsRT.anchorMin = new Vector2(0.5f, 0.5f);
        morningStarsRT.anchorMax = new Vector2(0.5f, 0.5f);
        morningStarsRT.pivot = new Vector2(0.5f, 0.5f);
        morningStarsRT.sizeDelta = new Vector2(80f, 20f);
        morningStarsRT.anchoredPosition = Vector2.zero;

        for (int s = 0; s < 3; s++)
        {
            GameObject starGO = new GameObject($"Star{s}");
            starGO.transform.SetParent(morningStarsParent.transform, false);
            RectTransform starRT = starGO.GetComponent<RectTransform>();
            if (starRT == null) starRT = starGO.AddComponent<RectTransform>();
            starRT.anchorMin = new Vector2(0.5f, 0.5f);
            starRT.anchorMax = new Vector2(0.5f, 0.5f);
            starRT.pivot = new Vector2(0.5f, 0.5f);
            starRT.sizeDelta = new Vector2(16f, 16f);
            starRT.anchoredPosition = new Vector2((s - 1) * 20f, 0f);

            Image starImg = starGO.GetComponent<Image>();
            if (starImg == null) starImg = starGO.AddComponent<Image>();
            starImg.sprite = starSprite;
            starImg.color = Color.black;
            starImg.raycastTarget = false;
            morningStars.Add(starImg);
        }

        TheDisplayingOfStars morningDisplay = morningStarsParent.GetComponent<TheDisplayingOfStars>();
        if (morningDisplay == null) morningDisplay = morningStarsParent.AddComponent<TheDisplayingOfStars>();
        morningDisplay.isMorning = true;
        morningDisplay.colorPass = Color.yellow;
        morningDisplay.colorFailMorning = Color.black;
        morningDisplay.colorFailEvening = Color.white;
        _morningStarDisplay.Add(morningDisplay);
        _morningStars.Add(morningStars);

        // Evening Image (чёрный)
        GameObject eveningGO = new GameObject("Evening");
        eveningGO.transform.SetParent(levelGO.transform, false);
        RectTransform eveningRT = eveningGO.GetComponent<RectTransform>();
        if (eveningRT == null)
        {
            eveningRT = eveningGO.AddComponent<RectTransform>();
        }
        eveningRT.anchorMin = new Vector2(0f, 0f);
        eveningRT.anchorMax = new Vector2(1f, 0.5f);
        eveningRT.offsetMin = new Vector2(2f, 2f);
        eveningRT.offsetMax = new Vector2(-2f, -2f);

        // CanvasGroup для Evening — тени
        CanvasGroup eveningCanvasGroup = eveningGO.GetComponent<CanvasGroup>();
        if (eveningCanvasGroup == null)
        {
            eveningCanvasGroup = eveningGO.AddComponent<CanvasGroup>();
        }
        eveningCanvasGroup.alpha = lockedAlpha;
        eveningCanvasGroup.blocksRaycasts = true;
        _eveningCanvasGroups.Add(eveningCanvasGroup);

        Image eveningImg = eveningGO.GetComponent<Image>();
        if (eveningImg == null)
        {
            eveningImg = eveningGO.AddComponent<Image>();
        }
        eveningImg.sprite = null;
        eveningImg.color = eveningImage;
        eveningImg.raycastTarget = true;
        _eveningImages.Add(eveningImg);

        // Button на Evening — обработка клика
        Button eveningBtn = eveningGO.GetComponent<Button>();
        if (eveningBtn == null) eveningBtn = eveningGO.AddComponent<Button>();
        eveningBtn.targetGraphic = eveningImg;
        int eveningLevelIndex = index;
        eveningBtn.onClick.AddListener(() => OnEveningClicked(eveningLevelIndex));

// Evening Stars (3 белые) — с TheDisplayingOfStars
        List<Image> eveningStars = new List<Image>();
        GameObject eveningStarsParent = new GameObject("Stars");
        eveningStarsParent.transform.SetParent(eveningGO.transform, false);
        RectTransform eveningStarsRT = eveningStarsParent.GetComponent<RectTransform>();
        if (eveningStarsRT == null) eveningStarsRT = eveningStarsParent.AddComponent<RectTransform>();
        eveningStarsRT.anchorMin = new Vector2(0.5f, 0.5f);
        eveningStarsRT.anchorMax = new Vector2(0.5f, 0.5f);
        eveningStarsRT.pivot = new Vector2(0.5f, 0.5f);
        eveningStarsRT.sizeDelta = new Vector2(80f, 20f);
        eveningStarsRT.anchoredPosition = Vector2.zero;

        for (int s = 0; s < 3; s++)
        {
            GameObject starGO = new GameObject($"Star{s}");
            starGO.transform.SetParent(eveningStarsParent.transform, false);
            RectTransform starRT = starGO.GetComponent<RectTransform>();
            if (starRT == null) starRT = starGO.AddComponent<RectTransform>();
            starRT.anchorMin = new Vector2(0.5f, 0.5f);
            starRT.anchorMax = new Vector2(0.5f, 0.5f);
            starRT.pivot = new Vector2(0.5f, 0.5f);
            starRT.sizeDelta = new Vector2(16f, 16f);
            starRT.anchoredPosition = new Vector2((s - 1) * 20f, 0f);

            Image starImg = starGO.GetComponent<Image>();
            if (starImg == null) starImg = starGO.AddComponent<Image>();
            starImg.sprite = star2Sprite;
            starImg.color = Color.white;
            starImg.raycastTarget = false;
            eveningStars.Add(starImg);
        }

        TheDisplayingOfStars eveningDisplay = eveningStarsParent.GetComponent<TheDisplayingOfStars>();
        if (eveningDisplay == null) eveningDisplay = eveningStarsParent.AddComponent<TheDisplayingOfStars>();
        eveningDisplay.isMorning = false;
        eveningDisplay.colorPass = Color.yellow;
        eveningDisplay.colorFailMorning = Color.black;
        eveningDisplay.colorFailEvening = Color.white;
        _eveningStarDisplay.Add(eveningDisplay);
        _eveningStars.Add(eveningStars);

        // Номер уровня — текст сверху (поверх всех дочерних элементов)
        GameObject levelNumberGO = new GameObject("LevelNumber");
        levelNumberGO.transform.SetParent(levelGO.transform, false);
        RectTransform levelNumberRT = levelNumberGO.GetComponent<RectTransform>();
        if (levelNumberRT == null)
        {
            levelNumberRT = levelNumberGO.AddComponent<RectTransform>();
        }
        levelNumberRT.anchorMin = new Vector2(0.5f, 1f);
        levelNumberRT.anchorMax = new Vector2(0.5f, 1f);
        levelNumberRT.anchoredPosition = new Vector2(0, -5f);
        levelNumberRT.sizeDelta = new Vector2(cellWidth - 10f, 20f);

        TextMeshProUGUI levelNumberText = levelNumberGO.GetComponent<TextMeshProUGUI>();
        if (levelNumberText == null)
        {
            levelNumberText = levelNumberGO.AddComponent<TextMeshProUGUI>();
        }
        levelNumberText.text = $"{index + 1}";
        levelNumberText.fontSize = 14;
        levelNumberText.fontStyle = FontStyles.Bold;
        levelNumberText.alignment = TextAlignmentOptions.Center;
        levelNumberText.color = Color.black;
        _levelNumbers.Add(levelNumberText);

        _levelButtons.Add(levelGO);
        return true;
    }

    // ========================================================================
    // РАЗМЕЩЕНИЕ КНОПОК В СЕТКЕ
    // ========================================================================

    /// <summary>
    /// Размещает все кнопки уровней в сетке.
    /// </summary>
    private void ArrangeButtonsInGrid()
    {
        // Получаем размер родителя
        RectTransform parentRT = _levelPanel.GetComponent<RectTransform>();
        if (parentRT == null)
        {
            Debug.LogError("[LevelMenuManager] У LevelPanel нет RectTransform!");
            return;
        }

        float parentWidth = parentRT.rect.width;
        float parentHeight = parentRT.rect.height;

        // Рассчитываем позицию относительно центра родителя
        float totalWidth = gridColumns * cellWidth + (gridColumns - 1) * spacingX;
        float totalHeight = gridRows * cellHeight + (gridRows - 1) * spacingY;

        // Отступ снизу, чтобы не накладывалось на кнопку "Вернуться"
        float bottomOffset = 60f;

        Debug.Log($"[LevelMenuManager] Parent size: {parentWidth:F1}x{parentHeight:F1}");
        Debug.Log($"[LevelMenuManager] Grid size: {totalWidth:F1}x{totalHeight:F1}");
        Debug.Log($"[LevelMenuManager] Размещаю {_levelButtons.Count} кнопок в сетке {gridColumns}x{gridRows}");

        for (int i = 0; i < _levelButtons.Count; i++)
        {
            GameObject obj = _levelButtons[i];
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                int col = i % gridColumns;
                int row = i / gridColumns;

                // Позиция: сверху-вниз, слева-направо
                // row 0 = верхняя строка, row 5 = нижняя
                float x = -totalWidth / 2f + col * (cellWidth + spacingX) + cellWidth / 2f;
                float y = totalHeight / 2f - row * (cellHeight + spacingY) - cellHeight / 2f;
                
                // Сдвигаем всю сетку вверх на bottomOffset/2
                y += bottomOffset / 2f;

                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(cellWidth, cellHeight);

                // Явно задаём порядок в иерархии
                obj.transform.SetSiblingIndex(i);

                Debug.Log($"[LevelMenuManager] Кнопка {i + 1}: {obj.name} -> pos = ({x:F1}, {y:F1}), sibling = {obj.transform.GetSiblingIndex()}");
            }
        }
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
        int morningStars = GetPartStars(levelIndex, 0);
        int eveningStars = GetPartStars(levelIndex, 1);

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
            bool morningPassed = morningStars > 0;
            _morningStarDisplay[levelIndex].SetBackgroundMode(true);
            _morningStarDisplay[levelIndex].SetLevelResult(morningPassed, morningStars);
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
            bool eveningPassed = eveningStars > 0;
            _eveningStarDisplay[levelIndex].SetBackgroundMode(false);
            _eveningStarDisplay[levelIndex].SetLevelResult(eveningPassed, eveningStars);
        }
    }

    #endregion

    #region Progress Checking

    public bool IsLevelUnlocked(int baseLevelIndex)
    {
        if (baseLevelIndex < 0 || baseLevelIndex >= totalBaseLevels)
            return false;

        if (baseLevelIndex == 0)
            return true;

        return IsPartCompleted(baseLevelIndex - 1, 1); // Вечер предыдущего уровня
    }

    private bool IsMorningUnlocked(int levelIndex)
    {
        return IsLevelUnlocked(levelIndex);
    }

    private bool IsEveningUnlocked(int levelIndex)
    {
        return IsPartCompleted(levelIndex, 0); // Утро текущего уровня
    }

    public bool IsPartCompleted(int levelIndex, int partIndex)
    {
        if (_progress.ContainsKey(levelIndex) && _progress[levelIndex].ContainsKey(partIndex))
        {
            return _progress[levelIndex][partIndex] > 0;
        }
        return false;
    }

    public int GetPartStars(int levelIndex, int partIndex)
    {
        if (_progress.ContainsKey(levelIndex) && _progress[levelIndex].ContainsKey(partIndex))
        {
            return _progress[levelIndex][partIndex];
        }
        return 0;
    }

    /// <summary>
    /// Статический метод получения звёзд — работает без экземпляра.
    /// </summary>
    public static int GetStarsStatic(int levelIndex, int partIndex)
    {
        if (_sharedProgress.ContainsKey(levelIndex) && _sharedProgress[levelIndex].ContainsKey(partIndex))
        {
            return _sharedProgress[levelIndex][partIndex];
        }
        return 0;
    }

    public void SaveProgress(int levelIndex, int partIndex, int stars)
    {
        if (!_progress.ContainsKey(levelIndex))
        {
            _progress[levelIndex] = new Dictionary<int, int>();
        }
        _progress[levelIndex][partIndex] = Mathf.Clamp(stars, 0, 3);
        if (Instance != null) Instance.UpdateAllPartsState();
        Debug.Log($"[LevelMenuManager] Сохранён прогресс: Уровень {levelIndex + 1}, Часть {partIndex + 1}, Звёзды: {stars}");
    }

    /// <summary>
    /// Статическое сохранение прогресса — работает без экземпляра.
    /// </summary>
    public static void SaveProgressStatic(int levelIndex, int partIndex, int stars)
    {
        if (!_sharedProgress.ContainsKey(levelIndex))
        {
            _sharedProgress[levelIndex] = new Dictionary<int, int>();
        }
        _sharedProgress[levelIndex][partIndex] = Mathf.Clamp(stars, 0, 3);
        Debug.Log($"[LevelMenuManager] Статическое сохранение: Уровень {levelIndex + 1}, Часть {partIndex + 1}, Звёзды: {stars}");
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

    #region Debug

    public void LogAllPartsState()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("📋 Состояние всех 72 частей уровней");
        Debug.Log("═══════════════════════════════════════");

        int totalParts = totalBaseLevels * 2;
        for (int i = 0; i < totalParts; i++)
        {
            int baseLevelIndex = i / 2;
            bool isSecondPart = i % 2 != 0;
            string partName = isSecondPart ? "2 часть" : "1 часть";

            bool isLevelUnlocked = IsLevelUnlocked(baseLevelIndex);
            bool isPartUnlocked = !isSecondPart && isLevelUnlocked || isSecondPart && IsPartCompleted(baseLevelIndex, 0);
            bool completed = isSecondPart
                ? IsPartCompleted(baseLevelIndex, 1)
                : IsPartCompleted(baseLevelIndex, 0);
            int stars = isSecondPart
                ? GetPartStars(baseLevelIndex, 1)
                : GetPartStars(baseLevelIndex, 0);

            string status = isPartUnlocked
                ? (completed ? $"✅ {stars}/3" : "🔓")
                : "🔒";

            Debug.Log($"  Ч. {i,2} | Уровень {baseLevelIndex + 1,2} {partName,-7} | {status}");
        }

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("═══════════════════════════════════════");
    }

    #endregion
}
