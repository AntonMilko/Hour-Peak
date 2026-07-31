using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HourPeak.Levels;

/// <summary>
/// Управляет меню выбора 72 частей уровней (36 базовых × 2: Утро и Вечер).
/// 
/// Логика открытия:
/// - Уровень 1-Утро открыт с самого начала
/// - После прохождения Утро X → открывается Вечер X
/// - После прохождения Вечер X → открывается Утро (X+1)
/// 
/// Part 1 = Morning (Утро)
/// Part 2 = Evening (Вечер)
/// 
/// Цвета:
/// - Уровень и 1 часть открыты: белые
/// - Уровень и 1 часть закрыты: серая тень
/// - 2 часть открыта: чёрная
/// - 2 часть закрыта: тёмная тень
/// </summary>
public class LevelMenuManager : MonoBehaviour
{
    #region Configuration

    [Header("Настройки")]
    [SerializeField] private int totalBaseLevels = 36; // 36 базовых уровней
    [SerializeField] private Transform levelsTemplate; // Родитель кнопок уровней (36 уровней по 2 элемента)
    [SerializeField] private GameObject levelButtonPrefab; // Префаб кнопки уровня (опционально — если null, создаст автоматически)

    [Header("Цвета — Фон уровня")]
    [SerializeField] private Color levelBackgroundUnlockedColor = Color.white; // Фон уровня открыт — белый
    [SerializeField] private Color levelBackgroundLockedColor = new Color(0.5f, 0.5f, 0.5f); // Фон уровня закрыт — серая тень

    [Header("Цвета — 1 часть (Утро)")]
    [SerializeField] private Color firstPartUnlockedColor = Color.white; // 1 часть открыта — белая
    [SerializeField] private Color firstPartLockedColor = new Color(0.5f, 0.5f, 0.5f); // 1 часть закрыта — серая тень

    [Header("Цвета — 2 часть (Вечер)")]
    [SerializeField] private Color secondPartUnlockedColor = Color.black; // 2 часть открыта — чёрная
    [SerializeField] private Color secondPartLockedColor = new Color(0.25f, 0.25f, 0.25f); // 2 часть закрыта — тёмная тень

    [Header("UI элементы статуса")]
    [Tooltip("Шаблон для звёзд Part 1 (Утро)")]
    [SerializeField] private Transform starsPart1Template; // Шаблоный объект с 3 Image звёзд
    
    [Tooltip("Шаблон для звёзд Part 2 (Вечер)")]
    [SerializeField] private Transform starsPart2Template; // Шаблоный объект с 3 Image звёзд

    #endregion

    #region Data Structures

    /// <summary>
    /// Прогресс прохождения уровня (утро + вечер).
    /// </summary>
    [Serializable]
    public class LevelProgress
    {
        [Tooltip("Пройдена ли утренняя версия")]
        public bool MorningCompleted;

        [Tooltip("Звёзды за утро (0-3)")]
        public int MorningStars;

        [Tooltip("Время прохождения утра")]
        public float MorningTime;

        [Tooltip("Пройдена ли вечерняя версия")]
        public bool EveningCompleted;

        [Tooltip("Звёзды за вечер (0-3)")]
        public int EveningStars;

        [Tooltip("Время прохождения вечера")]
        public float EveningTime;

        public LevelProgress()
        {
            MorningCompleted = false;
            MorningStars = 0;
            MorningTime = 0f;
            EveningCompleted = false;
            EveningStars = 0;
            EveningTime = 0f;
        }
    }

    #endregion

    #region Private Fields

    private List<Image> _partImages = new List<Image>();
    private List<Button> _partButtons = new List<Button>();
    private List<GameObject> _partObjects = new List<GameObject>();
    private int _completedPartsCount;
    private int _totalStarsCount;

    /// <summary>
    /// Прогресс для всех базовых уровней.
    /// </summary>
    [SerializeField] private Dictionary<int, LevelProgress> levelProgress = new Dictionary<int, LevelProgress>();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeProgress();
    }

    private void Start()
    {
        // Синхронизируем прогресс с LevelProgressManager и ProgressManager
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.SyncFromProgressManager();
        }
        
        CollectPartElements();
        UpdateAllPartsState();
    }

    /// <summary>
    /// Получает суффикс сцены для текущего уровня и части.
    /// </summary>
    private int GetScenePartSuffix(int baseLevelIndex, bool isPart2)
    {
        // Part 1 — всегда Morning .1
        if (!isPart2)
            return 1;
        
        // Part 2 — Evening: .3 для уровней 1 и 19, .2 для остальных
        if (baseLevelIndex == 0 || baseLevelIndex == 18)
            return 3;
        
        return 2;
    }

    /// <summary>
    /// Инициализирует прогресс для всех уровней.
    /// </summary>
    private void InitializeProgress()
    {
        // Создаём прогресс для каждого базового уровня
        for (int i = 0; i < totalBaseLevels; i++)
        {
            if (!levelProgress.ContainsKey(i))
            {
                levelProgress[i] = new LevelProgress();
            }
        }

        Debug.Log($"✅ LevelMenuManager инициализирован с {totalBaseLevels} базовыми уровнями ({totalBaseLevels * 2} частями)");
    }

    /// <summary>
    /// Вызывается извне для обновления состояния (например, после возврата из игры).
    /// </summary>
    public void Refresh()
    {
        CollectPartElements();
        UpdateAllPartsState();
    }

    #endregion

    #region Collection

    /// <summary>
    /// Собирает все Image/Button компоненты из дочерних элементов.
    /// Если элементов недостаточно — создаёт их динамически.
    /// Порядок: Part 0, Part 1, Part 2, ... Part 71
    /// Чётные (0, 2, 4...) — 1 часть (Утро), Нечётные (1, 3, 5...) — 2 часть (Вечер).
    /// </summary>
    private void CollectPartElements()
    {
        _partImages.Clear();
        _partButtons.Clear();
        _partObjects.Clear();

        int totalParts = totalBaseLevels * 2;

        // Если levelsTemplate не указан или в нём недостаточно элементов — создаём динамически
        int currentChildrenCount = levelsTemplate != null ? levelsTemplate.childCount : 0;
        
        if (currentChildrenCount < totalParts)
        {
            Debug.Log($"[LevelMenuManager] Создано {totalParts} кнопок (было {currentChildrenCount})");
            CreateLevelButtonsDynamically(totalParts);
        }

        // Собираем все Image/Button компоненты из дочерних элементов
        if (levelsTemplate != null)
        {
            foreach (Transform child in levelsTemplate)
            {
                Image img = child.GetComponent<Image>();
                Button btn = child.GetComponent<Button>();

                if (img != null) _partImages.Add(img);
                if (btn != null) _partButtons.Add(btn);

                _partObjects.Add(child.gameObject);
            }
        }

        Debug.Log($"[LevelMenuManager] Найдено {_partImages.Count} Image, {_partButtons.Count} Button, {_partObjects.Count} GameObject.");
    }

    /// <summary>
    /// Динамически создаёт кнопки уровней.
    /// </summary>
    private void CreateLevelButtonsDynamically(int count)
    {
        if (levelsTemplate == null)
        {
            Debug.LogError("[LevelMenuManager] levelsTemplate не указан! Невозможно создать кнопки.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject buttonGO;
            
            if (levelButtonPrefab != null)
            {
                buttonGO = Instantiate(levelButtonPrefab, levelsTemplate);
            }
            else
            {
                // Создаём простой GameObject с Image и Button
                buttonGO = new GameObject($"LevelButton_{i}");
                buttonGO.transform.SetParent(levelsTemplate, false);
                
                Image img = buttonGO.AddComponent<Image>();
                img.color = i % 2 == 0 ? Color.white : Color.black;
                
                Button btn = buttonGO.AddComponent<Button>();
                btn.onClick.AddListener(() => OnPartClicked(i));
                
                _partImages.Add(img);
                _partButtons.Add(btn);
                _partObjects.Add(buttonGO);
            }
        }
    }

    #endregion

    #region State Update

    /// <summary>
    /// Обновляет состояние всех 72 частей уровней.
    /// </summary>
    public void UpdateAllPartsState()
    {
        _completedPartsCount = 0;
        _totalStarsCount = 0;

        int totalParts = totalBaseLevels * 2;

        for (int i = 0; i < totalParts; i++)
        {
            // Проверяем, что элемент существует
            if (i >= _partObjects.Count)
            {
                Debug.LogWarning($"[LevelMenuManager] Элемент {i} не найден. Пропускаем.");
                continue;
            }

            int baseLevelIndex = i / 2; // 0,0->0; 1,1->1; 2,2->2; ...
            bool isSecondPart = i % 2 != 0; // 0->false (1 часть), 1->true (2 часть), 2->false, ...
            TimeOfDay timeOfDay = isSecondPart ? TimeOfDay.Evening : TimeOfDay.Morning;

            // Проверяем, разблокирован ли уровень целиком
            bool isLevelUnlocked = IsLevelUnlocked(baseLevelIndex);
            bool isPartUnlocked = isLevelUnlocked && !isSecondPart || isSecondPart && IsMorningCompleted(baseLevelIndex);
            bool isCompleted = isSecondPart
                ? IsEveningCompleted(baseLevelIndex)
                : IsMorningCompleted(baseLevelIndex);

            int stars = isSecondPart
                ? GetEveningStars(baseLevelIndex)
                : GetMorningStars(baseLevelIndex);

            if (isCompleted) _completedPartsCount++;
            _totalStarsCount += stars;

            ApplyPartState(i, isLevelUnlocked, isPartUnlocked, isCompleted, stars, isSecondPart);
        }

        UpdateProgressUI();
        Debug.Log($"[LevelMenuManager] Обновлены состояния {totalParts} частей. Пройдено: {_completedPartsCount}/{totalParts}, Звёзды: {_totalStarsCount}/{totalParts * 3}");
    }

    #endregion

    #region Progress Checking

    /// <summary>
    /// Проверяет доступность базового уровня.
    /// Использует LevelProgressManager для определения статуса.
    /// </summary>
    public bool IsLevelUnlocked(int baseLevelIndex)
    {
        if (baseLevelIndex < 0 || baseLevelIndex >= totalBaseLevels)
            return false;

        if (baseLevelIndex == 0)
            return true;

        // Используем LevelProgressManager
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.IsLevelUnlocked(baseLevelIndex);
        }

        // Fallback на внутреннюю логику
        return IsLevelFullyCompleted(baseLevelIndex - 1);
    }

    /// <summary>
    /// Асинхронная загрузка следующей части уровня.
    /// Part 1: Morning .1 (1→2→...→36)
    /// Part 2: Evening (1.3→2.2→...→18.2→19.3→20.2→...→36.2)
    /// После Part 2 уровня 36 → сертификат
    /// </summary>
    public System.Collections.IEnumerator LoadNextLevelPartAsync()
    {
        bool isPart2 = _currentPartIndex >= 2;
        int nextPartIndex = isPart2 ? _currentPartIndex + 1 : 2;
        
        bool isLastLevel = _currentBaseLevelIndex >= totalBaseLevels - 1;
        
        if (isPart2 && isLastLevel)
        {
            Debug.Log("🏆 Все уровни пройдены! Переход к сертификату...");
            LoadCertificateScene();
            yield break;
        }
        
        int nextBaseLevel;
        
        if (isPart2)
        {
            nextBaseLevel = _currentBaseLevelIndex + 1;
            if (nextBaseLevel >= totalBaseLevels)
            {
                Debug.Log("🏆 Все уровни пройдены! Переход к сертификату...");
                LoadCertificateScene();
                yield break;
            }
            
            _currentPartIndex = 1;
            int suffix = GetScenePartSuffix(nextBaseLevel, false);
            Debug.Log($"🔄 Переход: Уровень {nextBaseLevel + 1} Part 1 (сцена .{suffix})");
            LoadLevelWithTime(nextBaseLevel, TimeOfDay.Morning, suffix);
        }
        else
        {
            nextBaseLevel = _currentBaseLevelIndex;
            _currentPartIndex = 2;
            int suffix = GetScenePartSuffix(nextBaseLevel, true);
            Debug.Log($"🔄 Переход: Уровень {nextBaseLevel + 1} Part 2 (сцена .{suffix})");
            LoadLevelWithTime(nextBaseLevel, TimeOfDay.Evening, suffix);
        }
    }

    /// <summary>
    /// Проверяет доступность базового уровня (альяс).
    /// </summary>
    public bool CanAccessBaseLevel(int baseLevelIndex)
    {
        return IsLevelUnlocked(baseLevelIndex);
    }

    /// <summary>
    /// Проверяет полное завершение базового уровня (утро + вечер).
    /// </summary>
    public bool IsLevelFullyCompleted(int baseLevelIndex)
    {
        if (!levelProgress.ContainsKey(baseLevelIndex))
            return false;

        var progress = levelProgress[baseLevelIndex];
        return progress.MorningCompleted && progress.EveningCompleted;
    }

    /// <summary>
    /// Проверяет завершение утренней версии.
    /// Использует LevelProgressManager.
    /// </summary>
    public bool IsMorningCompleted(int baseLevelIndex)
    {
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.IsMorningCompleted(baseLevelIndex);
        }
        
        if (!levelProgress.ContainsKey(baseLevelIndex)) return false;
        return levelProgress[baseLevelIndex].MorningCompleted;
    }

    /// <summary>
    /// Проверяет завершение вечерней версии.
    /// Использует LevelProgressManager.
    /// </summary>
    public bool IsEveningCompleted(int baseLevelIndex)
    {
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.IsEveningCompleted(baseLevelIndex);
        }
        
        if (!levelProgress.ContainsKey(baseLevelIndex)) return false;
        return levelProgress[baseLevelIndex].EveningCompleted;
    }

    /// <summary>
    /// Получает звёзды за утро.
    /// Использует LevelProgressManager.
    /// </summary>
    public int GetMorningStars(int baseLevelIndex)
    {
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.GetMorningStars(baseLevelIndex);
        }
        
        if (!levelProgress.ContainsKey(baseLevelIndex)) return 0;
        return levelProgress[baseLevelIndex].MorningStars;
    }

    /// <summary>
    /// Получает звёзды за вечер.
    /// Использует LevelProgressManager.
    /// </summary>
    public int GetEveningStars(int baseLevelIndex)
    {
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.GetEveningStars(baseLevelIndex);
        }
        
        if (!levelProgress.ContainsKey(baseLevelIndex)) return 0;
        return levelProgress[baseLevelIndex].EveningStars;
    }

    /// <summary>
    /// Получает общее количество звёзд.
    /// </summary>
    public int GetTotalStars(int baseLevelIndex)
    {
        return GetMorningStars(baseLevelIndex) + GetEveningStars(baseLevelIndex);
    }

    #endregion

    #region Save/Load Progress

    /// <summary>
    /// Сохраняет прогресс уровня.
    /// Использует ProgressManager для сохранения сцены.
    /// </summary>
    public void SaveProgress(int baseLevelIndex, TimeOfDay timeOfDay, int stars, float time, bool perfectTiming)
    {
        if (!levelProgress.ContainsKey(baseLevelIndex))
        {
            levelProgress[baseLevelIndex] = new LevelProgress();
        }

        var progress = levelProgress[baseLevelIndex];
        int calculatedStars = Mathf.Clamp(stars, 0, 3);

        if (timeOfDay == TimeOfDay.Morning)
        {
            progress.MorningCompleted = true;
            progress.MorningStars = calculatedStars;
            progress.MorningTime = time;
        }
        else
        {
            progress.EveningCompleted = true;
            progress.EveningStars = calculatedStars;
            progress.EveningTime = time;
        }

        // Сохраняем через ProgressManager (148 сцен)
        string sceneName = GetSceneName(baseLevelIndex, timeOfDay, GetMaxParts(baseLevelIndex));
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SaveSceneCompletion(sceneName, calculatedStars, time);
        }

        // Обновляем LevelProgressManager
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.UpdateLevelProgress(baseLevelIndex, sceneName, calculatedStars, time);
        }

        // Проверяем полное завершение уровня и разблокируем следующий
        if (IsLevelFullyCompleted(baseLevelIndex))
        {
            UnlockNextBaseLevel(baseLevelIndex);
        }
    }

    /// <summary>
    /// Разблокирует следующий базовый уровень.
    /// </summary>
    private void UnlockNextBaseLevel(int completedBaseLevelIndex)
    {
        int nextBaseLevel = completedBaseLevelIndex + 1;
        if (nextBaseLevel < totalBaseLevels)
        {
            Debug.Log($"🎉 Уровень {completedBaseLevelIndex + 1} завершён! Уровень {nextBaseLevel + 1} разблокирован!");
        }
    }

    /// <summary>
    /// Завершает уровень.
    /// </summary>
    public void CompleteLevel(bool success, int stars, bool perfectTiming, TimeOfDay timeOfDay, int baseLevelIndex, float elapsedTime)
    {
        if (!success) return;

        SaveProgress(baseLevelIndex, timeOfDay, stars, elapsedTime, perfectTiming);

        string partName = timeOfDay == TimeOfDay.Morning ? "Утро" : "Вечер";
        Debug.Log($"{(success ? "✅" : "❌")} Уровень {baseLevelIndex + 1} ({partName}) | {stars}/3 ★ | {elapsedTime:F0}s");
    }

    #endregion

    #region Level Loading

    private int _currentBaseLevelIndex = 0;
    private TimeOfDay _currentTimeOfDay = TimeOfDay.Morning;
    private int _currentPartIndex = 1;

    /// <summary>
    /// Текущий базовый уровень (0-based).
    /// </summary>
    public int CurrentBaseLevelIndex => _currentBaseLevelIndex;

    /// <summary>
    /// Текущее время суток.
    /// </summary>
    public TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;

    /// <summary>
    /// Получает максимальное количество частей для уровня.
    /// Уровни 1 и 19 имеют 3 части, остальные — 2.
    /// </summary>
    private int GetMaxParts(int baseLevelIndex)
    {
        int level = baseLevelIndex + 1;
        return (level == 1 || level == 19) ? 3 : 2;
    }

    /// <summary>
    /// Загружает утреннюю версию базового уровня.
    /// </summary>
    public bool LoadLevelMorning(int baseLevelIndex)
    {
        return LoadLevelWithTime(baseLevelIndex, TimeOfDay.Morning, GetRandomPartIndex(baseLevelIndex, TimeOfDay.Morning));
    }

    /// <summary>
    /// Загружает вечернюю версию базового уровня.
    /// </summary>
    public bool LoadLevelEvening(int baseLevelIndex)
    {
        return LoadLevelWithTime(baseLevelIndex, TimeOfDay.Evening, GetRandomPartIndex(baseLevelIndex, TimeOfDay.Evening));
    }

    /// <summary>
    /// Загружает базовый уровень с указанным временем суток и частью.
    /// </summary>
    public bool LoadLevelWithTime(int baseLevelIndex, TimeOfDay timeOfDay, int partIndex)
    {
        // Проверка доступа к уровню
        if (!CanAccessBaseLevel(baseLevelIndex))
        {
            Debug.LogWarning($"🔒 Уровень {baseLevelIndex + 1} закрыт!");
            return false;
        }

        // Проверка валидности индекса
        if (baseLevelIndex < 0 || baseLevelIndex >= totalBaseLevels)
        {
            Debug.LogError($"❌ Уровень {baseLevelIndex} не найден!");
            return false;
        }

        // Обновляем текущий уровень
        _currentBaseLevelIndex = baseLevelIndex;
        _currentTimeOfDay = timeOfDay;
        _currentPartIndex = partIndex;

        string partName = timeOfDay == TimeOfDay.Morning ? "Утро" : "Вечер";
        Debug.Log($"🎮 Уровень {baseLevelIndex + 1} ({partName}, часть {partIndex}) начат");

        // Генерируем имя сцены и загружаем
        string sceneName = GetSceneName(baseLevelIndex, timeOfDay, partIndex);
        Debug.Log($"📺 Загрузка сцены: {sceneName}");
        SceneManager.LoadScene(sceneName);

        return true;
    }

    /// <summary>
    /// Получает случайную доступную часть уровня (1, 2 или 3).
    /// </summary>
    private int GetRandomPartIndex(int baseLevelIndex, TimeOfDay timeOfDay)
    {
        // Проверяем, есть ли сохранение для этого уровня
        string savePath = System.IO.Path.Combine(Application.persistentDataPath, "GameSaves", $"level_{baseLevelIndex + 1:000}.json");
        if (System.IO.File.Exists(savePath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(savePath);
                var data = JsonUtility.FromJson<LevelSaveData>(json);
                // Возвращаем часть из сохранения, если она существует
                if (data.partIndex >= 1 && data.partIndex <= 3)
                {
                    Debug.Log($"💾 Загружена часть {data.partIndex} из сохранения для уровня {baseLevelIndex + 1}");
                    return data.partIndex;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"⚠️ Ошибка загрузки сохранения: {e.Message}");
            }
        }

        // Если сохранения нет или часть не найдена — выбираем случайную (1-3)
        int randomPart = UnityEngine.Random.Range(1, 4);
        Debug.Log($"🎲 Выбрана случайная часть {randomPart} для уровня {baseLevelIndex + 1}");
        return randomPart;
    }

    /// <summary>
    /// Генерирует имя сцены на основе индекса уровня, времени суток и части.
    /// Формат: "Level {N}/{TimeOfDay}/TimeOfDay {N}.{partIndex}"
    /// Пример: "Level 1/Morning/Morning 1.1"
    /// </summary>
    private string GetSceneName(int baseLevelIndex, TimeOfDay timeOfDay, int partIndex)
    {
        string levelName = $"Level {baseLevelIndex + 1}";
        string folder = timeOfDay == TimeOfDay.Morning ? "Morning" : "Evening";
        string sceneFile = $"{(timeOfDay == TimeOfDay.Morning ? "Morning" : "Evening")} {baseLevelIndex + 1}.{partIndex}";
        return $"{levelName}/{folder}/{sceneFile}";
    }

    /// <summary>
    /// Данные сохранения для уровня.
    /// </summary>
    [Serializable]
    private class LevelSaveData
    {
        public int partIndex;
    }

    /// <summary>
    /// Загружает сцену сертификата.
    /// </summary>
    public void LoadCertificateScene()
    {
        Debug.Log($"📺 Загрузка сцены сертификата: Game completion certificate");
        SceneManager.LoadScene("Game completion certificate");
    }

    /// <summary>
    /// Загружает сцену StartMenu.
    /// </summary>
    public void LoadStartMenuScene()
    {
        Debug.Log($"📺 Загрузка сцены StartMenu");
        SceneManager.LoadScene("StartMenu");
    }

    /// <summary>
    /// Асинхронная загрузка уровня с указанным временем суток.
    /// </summary>
    public System.Collections.IEnumerator LoadLevelWithTimeAsync(int baseLevelIndex, TimeOfDay timeOfDay)
    {
        int partIndex = GetRandomPartIndex(baseLevelIndex, timeOfDay);
        LoadLevelWithTime(baseLevelIndex, timeOfDay, partIndex);
        yield return null;
    }

    /// <summary>
    /// Асинхронная загрузка уровня с автоматическим временем суток.
    /// </summary>
    public System.Collections.IEnumerator LoadLevelAsync(int baseLevelIndex)
    {
        yield return LoadLevelWithTimeAsync(baseLevelIndex, _currentTimeOfDay);
    }

    #endregion

    #region Reset Progress

    /// <summary>
    /// Сбрасывает прогресс базового уровня.
    /// </summary>
    public void ResetLevelProgress(int baseLevelIndex)
    {
        if (levelProgress.ContainsKey(baseLevelIndex))
            levelProgress[baseLevelIndex] = new LevelProgress();

        Debug.Log($"🔄 Прогресс уровня {baseLevelIndex + 1} сброшен");
    }

    /// <summary>
    /// Сбрасывает прогресс для конкретного времени суток.
    /// </summary>
    public void ResetVariantProgress(int baseLevelIndex, TimeOfDay timeOfDay)
    {
        if (!levelProgress.ContainsKey(baseLevelIndex)) return;

        var progress = levelProgress[baseLevelIndex];
        if (timeOfDay == TimeOfDay.Morning)
        {
            progress.MorningCompleted = false;
            progress.MorningStars = 0;
            progress.MorningTime = 0f;
        }
        else
        {
            progress.EveningCompleted = false;
            progress.EveningStars = 0;
            progress.EveningTime = 0f;
        }

        Debug.Log($"🔄 {timeOfDay.GetDisplayName()} сброшен для уровня {baseLevelIndex + 1}");
    }

    #endregion

    #region Part State

    /// <summary>
    /// Применяет визуальное состояние к части уровня.
    /// Цвета:
    /// - Фон уровня открыт: белый, закрыт: серая тень
    /// - 1 часть открыта: белая, закрыта: серая тень
    /// - 2 часть открыта: чёрная, закрыта: тёмная тень
    /// </summary>
    private void ApplyPartState(int partIndex, bool isLevelUnlocked, bool isPartUnlocked, bool isCompleted, int stars, bool isSecondPart)
    {
        // Проверяем, что элемент существует
        if (partIndex >= _partObjects.Count)
        {
            Debug.LogWarning($"[LevelMenuManager] Элемент {partIndex} не найден. Пропускаем.");
            return;
        }

        Image partImage = null;
        Button partButton = null;

        if (partIndex < _partImages.Count) partImage = _partImages[partIndex];
        if (partIndex < _partButtons.Count) partButton = _partButtons[partIndex];

        // Определяем цвет
        Color targetColor;

        if (isSecondPart)
        {
            // 2 часть: чёрная (если 1 часть пройдена) / тёмная тень (если 1 часть не пройдена)
            targetColor = isCompleted ? secondPartUnlockedColor : secondPartLockedColor;
        }
        else
        {
            // 1 часть: белая (если уровень открыт) / серая тень (если уровень закрыт)
            targetColor = isLevelUnlocked ? firstPartUnlockedColor : firstPartLockedColor;
        }

        // Применяем Image
        if (partImage != null)
        {
            partImage.color = targetColor;
        }

        // Применяем Button
        if (partButton != null)
        {
            partButton.interactable = isPartUnlocked && !isCompleted;
        }

        // Обновляем фон уровня (первый дочерний элемент — фон уровня)
        UpdateLevelBackground(_partObjects[partIndex], isLevelUnlocked);

        // Обновляем звёзды и метки на GameObject части
        UpdatePartLabels(_partObjects[partIndex], isCompleted, stars, isSecondPart);
    }

    /// <summary>
    /// Обновляет цвет фона уровня (первый Image на GameObject).
    /// </summary>
    private void UpdateLevelBackground(GameObject partGO, bool isLevelUnlocked)
    {
        if (partGO == null) return;

        // Ищем Image фона уровня (обычно первый child с компонентом Image)
        Image[] images = partGO.GetComponentsInChildren<Image>();
        if (images.Length > 0)
        {
            // Первый Image — это фон уровня
            Color bgColor = isLevelUnlocked ? levelBackgroundUnlockedColor : levelBackgroundLockedColor;
            images[0].color = bgColor;
        }
    }

    #endregion

    #region Part Labels

    /// <summary>
    /// Обновляет метки (1 часть / 2 часть) и звёзды на GameObject части.
    /// Звёзды: все изначально неактивны, активируются первые N штук (N = starsCollected).
    /// Цвета управляются скриптом TheDisplayingOfStars.
    /// </summary>
    private void UpdatePartLabels(GameObject partGO, bool isCompleted, int stars, bool isSecondPart)
    {
        if (partGO == null) return;

        // Определяем "фон" части: false = белый (1 часть), true = чёрный (2 часть)
        bool isDarkBackground = isSecondPart;

        // Ищем TextMeshPro для метки части
        var existingLabels = partGO.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI partLabel = null;

        foreach (var text in existingLabels)
        {
            if (text.name.Contains("PartLabel") || text.name.Contains("Label"))
                partLabel = text;
        }

        // Обновляем текст метки части
        if (partLabel != null)
        {
            partLabel.text = isSecondPart ? "Part 2" : "Part 1";
            partLabel.gameObject.SetActive(true);
        }

        // Обновляем звёзды (Image спрайты)
        UpdateStarImages(partGO, isCompleted, stars, isDarkBackground);
    }

    /// <summary>
    /// Обновляет видимость звёзд на GameObject части.
    /// Все звёзды изначально неактивны. Активируются первые N звёзд (N = starsCollected).
    /// Цвета управляются скриптом TheDisplayingOfStars.
    /// </summary>
    private void UpdateStarImages(GameObject partGO, bool isCompleted, int stars, bool isSecondPart)
    {
        // Определяем шаблон в зависимости от части
        Transform starsTemplate = isSecondPart ? starsPart2Template : starsPart1Template;

        if (partGO == null) return;

        // Ищем существующие GameObject звёзд (Star0, Star1, Star2)
        GameObject[] starGOs = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            string starName = $"Star{i}";
            var children = partGO.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name.Contains(starName))
                {
                    starGOs[i] = child.gameObject;
                    break;
                }
            }
        }

        // Создаём звёзды, если их нет
        bool needsCreation = starGOs[0] == null;
        if (needsCreation && starsTemplate != null)
        {
            // Создаём 3 звезды из шаблона
            for (int i = 0; i < 3; i++)
            {
                GameObject starGO = Instantiate(starsTemplate.GetChild(i).gameObject, partGO.transform, false);
                starGO.name = $"Star{i}";
                starGO.SetActive(false); // Изначально все неактивны

                starGOs[i] = starGO;
            }
        }

        // Активируем первые N звёзд (N = starsCollected)
        for (int i = 0; i < 3; i++)
        {
            if (starGOs[i] != null)
            {
                // Показываем звезду, если уровень пройден и индекс меньше количества звёзд
                starGOs[i].SetActive(isCompleted && i < stars);
            }
        }
    }

    #endregion

    #region Progress UI

    /// <summary>
    /// Обновляет тексты прогресса.
    /// </summary>
    private void UpdateProgressUI()
    {
        // Текст прогресса теперь управляется вручную или через внешнюю UI систему
    }

    #endregion

    #region Part Actions

    /// <summary>
    /// Вызывается при нажатии на кнопку части уровня.
    /// Запускает нужную часть (1 или 2) нужного уровня.
    /// </summary>
    public void OnPartClicked(int partIndex)
    {
        int baseLevelIndex = partIndex / 2;
        bool isSecondPart = partIndex % 2 != 0;
        TimeOfDay timeOfDay = isSecondPart ? TimeOfDay.Evening : TimeOfDay.Morning;

        // Проверяем, разблокирована ли часть
        bool isLevelUnlocked = IsLevelUnlocked(baseLevelIndex);
        bool isPartUnlocked = !isSecondPart && isLevelUnlocked || isSecondPart && IsMorningCompleted(baseLevelIndex);
        if (!isPartUnlocked)
        {
            Debug.LogWarning($"[LevelMenuManager] Часть {partIndex} (Уровень {baseLevelIndex + 1}, {timeOfDay}) заблокирована!");
            return;
        }

        // Проверяем, не пройдена ли уже (если пройдена — можно запустить для перезаказа звёзд)
        bool isCompleted = isSecondPart
            ? IsEveningCompleted(baseLevelIndex)
            : IsMorningCompleted(baseLevelIndex);

        string partName = isSecondPart ? "2 часть" : "1 часть";
        Debug.Log($"[LevelMenuManager] Запуск: Уровень {baseLevelIndex + 1} — {partName} (partIndex={partIndex}, completed={isCompleted})");

        // Загружаем часть уровня
        if (!isSecondPart)
        {
            LoadLevelMorning(baseLevelIndex);
        }
        else
        {
            LoadLevelEvening(baseLevelIndex);
        }
    }

    #endregion

    #region Debug

    /// <summary>
    /// Выводит детальную информацию о состоянии всех частей в консоль.
    /// </summary>
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

            // Проверяем, разблокирована ли часть
            bool isLevelUnlocked = IsLevelUnlocked(baseLevelIndex);
            bool isPartUnlocked = !isSecondPart && isLevelUnlocked || isSecondPart && IsMorningCompleted(baseLevelIndex);
            bool completed = isSecondPart
                ? IsEveningCompleted(baseLevelIndex)
                : IsMorningCompleted(baseLevelIndex);
            int stars = isSecondPart
                ? GetEveningStars(baseLevelIndex)
                : GetMorningStars(baseLevelIndex);

            string status = isPartUnlocked
                ? (completed ? $"✅ {stars}/3" : "🔓")
                : "🔒";

            Debug.Log($"  Ч. {i,2} | Уровень {baseLevelIndex + 1,2} {partName,-7} | {status}");
        }

        Debug.Log($"═══════════════════════════════════════");
        Debug.Log($"Итого пройдено: {_completedPartsCount}/{totalParts} | Звёзды: {_totalStarsCount}/{totalParts * 3}");
        Debug.Log("═══════════════════════════════════════");
    }

    #endregion
}
