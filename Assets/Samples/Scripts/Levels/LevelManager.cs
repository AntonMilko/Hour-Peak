using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Levels
{
    /// <summary>
    /// Менеджер уровней с оптимизированной структурой данных.
    /// Управляет загрузкой уровней, прогрессом и разблокировкой.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        #region Singleton

        private static LevelManager _instance;

        /// <summary>
        /// Глобальный экземпляр.
        /// </summary>
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LevelManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("LevelManager");
                        _instance = go.AddComponent<LevelManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        [Header("Level Settings")]
        [Tooltip("Настройки уровней (сцена, название, время суток)")]
        [SerializeField] private LevelInfo[] levels;

        [Header("Current Session")]
        [Tooltip("Текущий базовый уровень (0, 1, 2, ...)")]
        [SerializeField] private int currentBaseLevelIndex = 0;

        [Tooltip("Индекс утренней части текущего уровня (всегда 0)")]
        [SerializeField] private int currentLevelMorningPartIndex = 0;

        [Tooltip("Индекс вечерней части текущего уровня (всегда 1)")]
        [SerializeField] private int currentLevelEveningPartIndex = 1;

        [Header("UI")]
        [SerializeField] private Image levelBackgroundImage;

        /// <summary>
        /// Номер текущего базового уровня (0-based).
        /// </summary>
        public int CurrentBaseLevelIndex => currentBaseLevelIndex;

        /// <summary>
        /// Номер текущей части внутри базового уровня (0=Утро, 1=Вечер).
        /// </summary>
        public int CurrentLevelPartIndexInBase => IsMorningActive ? currentLevelMorningPartIndex : currentLevelEveningPartIndex;

        /// <summary>
        /// Активна ли утренняя часть (утро того же уровня, что и вечер, или вечер ещё не пройден).
        /// </summary>
        public bool IsMorningActive => currentBaseLevelIndex >= _currentMorningCompletedLevel;

        /// <summary>
        /// Текущее время суток.
        /// </summary>
        public TimeOfDay CurrentTimeOfDay => IsMorningActive ? TimeOfDay.Morning : TimeOfDay.Evening;

        /// <summary>
        /// Номер текущей части уровня (1-based для отображения).
        /// </summary>
        public int CurrentLevelPartNumber => IsMorningActive ? currentLevelMorningPartIndex + 1 : currentLevelEveningPartIndex + 1;

        /// <summary>
        /// Номер текущего базового уровня (1-based для отображения).
        /// </summary>
        public int CurrentBaseLevelNumber => CurrentBaseLevelIndex + 1;

        #endregion

        #region State

        private float _levelStartTime;
        private bool _isLevelActive;
        private DifficultyTimeSettings _currentDifficultyTimes;
        private int _currentMorningCompletedLevel = -1; // Уровень, у которого пройдено утро

        #endregion

        #region Level Data Structure

        /// <summary>
        /// Информация об уровне (сцена, название, время суток).
        /// </summary>
        [System.Serializable]
        public class LevelInfo
        {
            [Tooltip("Название сцены")]
            public string SceneName;

            [Tooltip("Отображаемое название уровня")]
            public string DisplayName;

            [Tooltip("Время суток для этого уровня")]
            public TimeOfDay TimeOfDay;

            public LevelInfo()
            {
                SceneName = "";
                DisplayName = "Unknown Level";
                TimeOfDay = TimeOfDay.Morning;
            }

            public LevelInfo(string scene, string name, TimeOfDay tod)
            {
                SceneName = scene;
                DisplayName = name;
                TimeOfDay = tod;
            }
        }

        #endregion

        #region Time Settings Structure

        /// <summary>
        /// Настройки времени для уровня сложности.
        /// </summary>
        [System.Serializable]
        public class DifficultyTimeSettings
        {
            [Tooltip("3 звезды - отличное время")]
            public float ExcellentTime;

            [Tooltip("2 звезды - хорошее время")]
            public float GoodTime;

            [Tooltip("1 звезда - базовое время")]
            public float BasicTime;

            [Tooltip("Лимит времени до проигрыша")]
            public float TimeLimit;

            /// <summary>
            /// Создаёт настройки времени для любой сложности.
            /// </summary>
            public DifficultyTimeSettings(float excellent, float good, float basic, float limit)
            {
                ExcellentTime = excellent;
                GoodTime = good;
                BasicTime = basic;
                TimeLimit = limit;
            }

            /// <summary>
            /// Предустановленные настройки для сложности "Новичок".
            /// 3★ = 30с, 2★ = 60с, 1★ = 120с, Лимит = 240с
            /// </summary>
            public static DifficultyTimeSettings Beginner => new(30f, 60f, 120f, 240f);

            /// <summary>
            /// Предустановленные настройки для сложности "Профессионал".
            /// 3★ = 15с, 2★ = 30с, 1★ = 60с, Лимит = 120с
            /// </summary>
            public static DifficultyTimeSettings Professional => new(15f, 30f, 60f, 120f);

            /// <summary>
            /// Предустановленные настройки для сложности "Экстремал".
            /// 3★ = 7.5с, 2★ = 15с, 1★ = 30с, Лимит = 60с
            /// </summary>
            public static DifficultyTimeSettings Extremal => new(7.5f, 15f, 30f, 60f);

            /// <summary>
            /// Получает настройки времени по сложности.
            /// </summary>
            public static DifficultyTimeSettings GetByDifficulty(DifficultyLevel difficulty)
            {
                return difficulty switch
                {
                    DifficultyLevel.Beginner => Beginner,
                    DifficultyLevel.Professional => Professional,
                    DifficultyLevel.Extremal => Extremal,
                    _ => Beginner
                };
            }
        }

        #endregion

        #region Progress Tracking

        [SerializeField] private Dictionary<int, LevelProgress> levelProgress = new Dictionary<int, LevelProgress>();

        #endregion

        #region Properties

        /// <summary>
        /// Активен ли текущий уровень.
        /// </summary>
        public bool IsLevelActive => _isLevelActive;

        /// <summary>
        /// Прошедшее время на уровне.
        /// </summary>
        public float ElapsedTime => _isLevelActive ? Time.time - _levelStartTime : 0f;

        /// <summary>
        /// Общее количество базовых уровней.
        /// </summary>
        public int TotalBaseLevels => levels?.Length ?? 0;

        /// <summary>
        /// Общее количество частей уровня (базовые уровни × 2: утро + вечер).
        /// </summary>
        public int TotalLevelParts => TotalBaseLevels * 2;

        /// <summary>
        /// Текущие настройки времени для сложности.
        /// </summary>
        public DifficultyTimeSettings CurrentDifficultyTimes => _currentDifficultyTimes;

        #endregion

        #region Events

        /// <summary>
        /// Событие при начале уровня.
        /// </summary>
        public event Action<int, TimeOfDay> OnLevelStarted;

        /// <summary>
        /// Событие при завершении уровня.
        /// </summary>
        public event Action<int, TimeOfDay, LevelResult> OnLevelCompleted;

        /// <summary>
        /// Событие при разблокировке нового уровня.
        /// </summary>
        public event Action<int> OnLevelUnlocked;

        #endregion

        #region Data Structures

        /// <summary>
        /// Прогресс прохождения уровня.
        /// </summary>
        [System.Serializable]
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

        /// <summary>
        /// Результат прохождения уровня.
        /// </summary>
        [System.Serializable]
        public struct LevelResult
        {
            public bool Success;
            public int Stars;
            public bool PerfectTiming;
            public float CompletionTime;
            public TimeOfDay TimeOfDay;

            public LevelResult(bool success, int stars, bool perfectTiming, float time, TimeOfDay tod)
            {
                Success = success;
                Stars = stars;
                PerfectTiming = perfectTiming;
                CompletionTime = time;
                TimeOfDay = tod;
            }

            public override string ToString()
            {
                string status = Success ? "✅" : "❌";
                return $"{status} {TimeOfDay.GetDisplayName()} | {Stars}/3 ★ | {CompletionTime:F0}s";
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            // Инициализируем настройки времени по умолчанию
            InitializeTimeSettings();
            
            InitializeProgress();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            if (_isLevelActive)
                CheckCompletion();
        }

        #endregion

        #region Time Settings Initialization

        /// <summary>
        /// Инициализирует настройки времени для каждой сложности.
        /// </summary>
        private void InitializeTimeSettings()
        {
            Debug.Log("⏱️ Настройки времени инициализированы для всех сложностей");
        }

        /// <summary>
        /// Применяет настройки времени для текущей сложности.
        /// </summary>
        private void ApplyDifficultyTimeSettings()
        {
            var difficultyManager = FindFirstObjectByType<SettingDifficulty>();
            
            if (difficultyManager != null)
            {
                var difficulty = difficultyManager.CurrentDifficulty;
                _currentDifficultyTimes = DifficultyTimeSettings.GetByDifficulty(difficulty);
                
                Debug.Log($"⏱️ Применены настройки времени для {difficultyManager.GetCurrentDifficultyName()}: " +
                         $"3★={_currentDifficultyTimes.ExcellentTime}s, " +
                         $"2★={_currentDifficultyTimes.GoodTime}s, " +
                         $"1★={_currentDifficultyTimes.BasicTime}s, " +
                         $"Лимит={_currentDifficultyTimes.TimeLimit}s");
            }
            else
            {
                // Если SettingDifficulty не найден, используем Новичок по умолчанию
                _currentDifficultyTimes = DifficultyTimeSettings.Beginner;
                Debug.LogWarning("⚠️ SettingDifficulty не найден! Используется сложность 'Новичок'");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует прогресс для всех уровней.
        /// </summary>
        private void InitializeProgress()
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogWarning("⚠️ Levels not configured!");
                return;
            }

            // Создаём прогресс для каждого базового уровня
            for (int i = 0; i < levels.Length; i++)
            {
                if (!levelProgress.ContainsKey(i))
                {
                    levelProgress[i] = new LevelProgress();
                }
            }

            Debug.Log($"✅ LevelManager инициализирован с {levels.Length} базовыми уровнями ({levels.Length * 2} частями)");
        }

        #endregion

        #region Level Loading

        /// <summary>
        /// Загружает базовый уровень с автоматическим временем суток.
        /// </summary>
        /// <param name="baseLevelIndex">Индекс базового уровня (0, 1, 2, ...)</param>
        /// <returns>Успешно ли загружен</returns>
        public bool LoadLevel(int baseLevelIndex)
        {
            return LoadLevelWithTime(baseLevelIndex, GetLevelTimeOfDay(baseLevelIndex));
        }

        /// <summary>
        /// Загружает утреннюю версию базового уровня.
        /// </summary>
        /// <param name="baseLevelIndex">Индекс базового уровня (0, 1, 2, ...)</param>
        /// <returns>Успешно ли загружен</returns>
        public bool LoadLevelMorning(int baseLevelIndex)
        {
            return LoadLevelWithTime(baseLevelIndex, TimeOfDay.Morning);
        }

        /// <summary>
        /// Загружает вечернюю версию базового уровня.
        /// </summary>
        /// <param name="baseLevelIndex">Индекс базового уровня (0, 1, 2, ...)</param>
        /// <returns>Успешно ли загружен</returns>
        public bool LoadLevelEvening(int baseLevelIndex)
        {
            return LoadLevelWithTime(baseLevelIndex, TimeOfDay.Evening);
        }

        /// <summary>
        /// Загружает базовый уровень с указанным временем суток.
        /// </summary>
        /// <param name="baseLevelIndex">Индекс базового уровня (0, 1, 2, ...)</param>
        /// <param name="timeOfDay">Время суток</param>
        /// <returns>Успешно ли загружен</returns>
        public bool LoadLevelWithTime(int baseLevelIndex, TimeOfDay timeOfDay)
        {
            // Проверка доступа к уровню
            if (!CanAccessBaseLevel(baseLevelIndex))
            {
                Debug.LogWarning($"🔒 Уровень {baseLevelIndex + 1} закрыт!");
                return false;
            }

            // Проверка валидности индекса
            if (baseLevelIndex < 0 || baseLevelIndex >= TotalBaseLevels)
            {
                Debug.LogError($"❌ Уровень {baseLevelIndex} не найден!");
                return false;
            }

            // Получаем информацию об уровне
            var levelInfo = levels[baseLevelIndex];
            string sceneName = levelInfo.SceneName;
            string levelName = levelInfo.DisplayName;

            // Устанавливаем индексы уровня и части
            currentBaseLevelIndex = baseLevelIndex;
            // Отмечаем, что утро этого уровня пройдено, если загружаем вечер
            if (timeOfDay == TimeOfDay.Evening && _currentMorningCompletedLevel < baseLevelIndex)
            {
                _currentMorningCompletedLevel = baseLevelIndex;
            }
            
            _isLevelActive = true;
            _levelStartTime = Time.time;

            // Применяем настройки времени для текущей сложности
            ApplyDifficultyTimeSettings();

            SceneManager.LoadScene(sceneName);
            ApplyBackground(timeOfDay);

            string partName = timeOfDay == TimeOfDay.Morning ? "Утро" : "Вечер";
            string partInfo = $"Уровень {baseLevelIndex + 1}, {partName}";
            Debug.Log($"🎮 {levelName} ({partName}) начат [Часть: {partInfo}]");

            OnLevelStarted?.Invoke(baseLevelIndex, timeOfDay);
            return true;
        }

        /// <summary>
        /// Применяет фон в зависимости от времени суток.
        /// </summary>
        private void ApplyBackground(TimeOfDay timeOfDay)
        {
            if (levelBackgroundImage == null) return;

            var settings = timeOfDay.GetSettings();
            if (settings.BackgroundImage != null)
            {
                levelBackgroundImage.sprite = settings.BackgroundImage;
                Debug.Log($"🖼️ Фон применён: {settings.BackgroundImage.name}");
            }
        }

        /// <summary>
        /// Загружает следующую часть уровня (Утро → Вечер → следующий уровень Утро).
        /// </summary>
        /// <returns>Успешно ли загружена следующая часть</returns>
        public bool LoadNextLevelPart()
        {
            // Если утренняя часть того же уровня ещё не пройдена (утро >= вечера)
            if (currentBaseLevelIndex > _currentMorningCompletedLevel)
            {
                // Загружаем вечернюю часть текущего уровня
                return LoadLevelWithTime(currentBaseLevelIndex, TimeOfDay.Evening);
            }
            // Иначе загружаем утру следующего уровня
            else
            {
                int nextBaseLevel = currentBaseLevelIndex + 1;
                if (nextBaseLevel >= TotalBaseLevels)
                {
                    Debug.Log("🎉 Все уровни пройдены!");
                    return false;
                }
                return LoadLevelWithTime(nextBaseLevel, TimeOfDay.Morning);
            }
        }

        #endregion

        #region Completion

        /// <summary>
        /// Проверяет завершение уровня.
        /// </summary>
        private void CheckCompletion()
        {
            if (CurrentBaseLevelIndex < 0) return;

            // Проверка лимита времени для текущей сложности
            if (_currentDifficultyTimes != null && ElapsedTime >= _currentDifficultyTimes.TimeLimit)
            {
                CompleteLevel(false, 0, false);
            }
        }

        /// <summary>
        /// Завершает уровень.
        /// </summary>
        /// <param name="success">Успешно ли завершён</param>
        /// <param name="stars">Полученные звёзды</param>
        /// <param name="perfectTiming">Идеальное ли время</param>
        public void CompleteLevel(bool success, int stars, bool perfectTiming)
        {
            if (!_isLevelActive || CurrentBaseLevelIndex < 0) return;

            _isLevelActive = false;
            float completionTime = ElapsedTime;

            // Сохраняем прогресс
            if (success)
            {
                SaveProgress(CurrentBaseLevelIndex, CurrentTimeOfDay, stars, completionTime, perfectTiming);
                
                // Проверяем полное завершение уровня (утро + вечер)
                if (IsLevelFullyCompleted(CurrentBaseLevelIndex))
                {
                    UnlockNextBaseLevel(CurrentBaseLevelIndex);
                }
            }

            var result = new LevelResult(success, stars, perfectTiming, completionTime, CurrentTimeOfDay);

            string partName = CurrentTimeOfDay == TimeOfDay.Morning ? "Утро" : "Вечер";
            Debug.Log($"{(success ? "✅" : "❌")} {GetLevelDisplayName(CurrentBaseLevelIndex)} ({partName}) | {stars}/3 ★ | {completionTime:F0}s");

            OnLevelCompleted?.Invoke(CurrentBaseLevelIndex, CurrentTimeOfDay, result);
        }

        /// <summary>
        /// Сохраняет прогресс уровня.
        /// </summary>
        private void SaveProgress(int baseLevelIndex, TimeOfDay timeOfDay, int stars, float time, bool perfectTiming)
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
        }

        /// <summary>
        /// Разблокирует следующий базовый уровень.
        /// </summary>
        private void UnlockNextBaseLevel(int completedBaseLevelIndex)
        {
            int nextBaseLevel = completedBaseLevelIndex + 1;
            if (nextBaseLevel < TotalBaseLevels)
            {
                Debug.Log($"🎉 Уровень {completedBaseLevelIndex + 1} завершён! Уровень {nextBaseLevel + 1} разблокирован!");
                OnLevelUnlocked?.Invoke(nextBaseLevel);
            }
        }

        #endregion

        #region Progress Management

        /// <summary>
        /// Проверяет доступность базового уровня.
        /// </summary>
        public bool CanAccessBaseLevel(int baseLevelIndex)
        {
            if (baseLevelIndex < 0 || baseLevelIndex >= TotalBaseLevels)
                return false;

            if (baseLevelIndex == 0)
                return true;

            return IsLevelFullyCompleted(baseLevelIndex - 1);
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
        /// </summary>
        public bool IsMorningCompleted(int baseLevelIndex)
        {
            if (!levelProgress.ContainsKey(baseLevelIndex)) return false;
            return levelProgress[baseLevelIndex].MorningCompleted;
        }

        /// <summary>
        /// Проверяет завершение вечерней версии.
        /// </summary>
        public bool IsEveningCompleted(int baseLevelIndex)
        {
            if (!levelProgress.ContainsKey(baseLevelIndex)) return false;
            return levelProgress[baseLevelIndex].EveningCompleted;
        }

        /// <summary>
        /// Получает звёзды за утро.
        /// </summary>
        public int GetMorningStars(int baseLevelIndex)
        {
            if (!levelProgress.ContainsKey(baseLevelIndex)) return 0;
            return levelProgress[baseLevelIndex].MorningStars;
        }

        /// <summary>
        /// Получает звёзды за вечер.
        /// </summary>
        public int GetEveningStars(int baseLevelIndex)
        {
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

        /// <summary>
        /// Получает список разблокированных базовых уровней.
        /// </summary>
        public List<int> GetUnlockedLevels()
        {
            var unlocked = new List<int>();
            for (int i = 0; i < TotalBaseLevels; i++)
            {
                if (CanAccessBaseLevel(i))
                    unlocked.Add(i);
            }
            return unlocked;
        }

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

        #region Helper Methods

        /// <summary>
        /// Получает информацию об уровне.
        /// </summary>
        public LevelInfo GetLevelInfo(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= TotalBaseLevels)
                return null;
            
            return levels[levelIndex];
        }

        /// <summary>
        /// Получает название сцены для уровня.
        /// </summary>
        public string GetLevelSceneName(int levelIndex)
        {
            var levelInfo = GetLevelInfo(levelIndex);
            return levelInfo?.SceneName;
        }

        /// <summary>
        /// Получает отображаемое название уровня.
        /// </summary>
        public string GetLevelDisplayName(int levelIndex)
        {
            var levelInfo = GetLevelInfo(levelIndex);
            return levelInfo?.DisplayName ?? $"Уровень {levelIndex + 1}";
        }

        /// <summary>
        /// Получает время суток для уровня.
        /// </summary>
        public TimeOfDay GetLevelTimeOfDay(int levelIndex)
        {
            var levelInfo = GetLevelInfo(levelIndex);
            return levelInfo?.TimeOfDay ?? TimeOfDay.Morning;
        }

        /// <summary>
        /// Рассчитывает звёзды на основе времени прохождения.
        /// </summary>
        public int CalculateStars(float completionTime)
        {
            if (_currentDifficultyTimes == null)
            {
                // Fallback на настройки Новичка
                _currentDifficultyTimes = DifficultyTimeSettings.Beginner;
            }

            if (completionTime <= _currentDifficultyTimes.ExcellentTime)
                return 3;
            else if (completionTime <= _currentDifficultyTimes.GoodTime)
                return 2;
            else if (completionTime <= _currentDifficultyTimes.BasicTime)
                return 1;
            else
                return 0; // Провал
        }

        /// <summary>
        /// Получает настройки времени для указанной сложности.
        /// </summary>
        public DifficultyTimeSettings GetDifficultyTimeSettings(DifficultyLevel difficulty)
        {
            return DifficultyTimeSettings.GetByDifficulty(difficulty);
        }

        /// <summary>
        /// Получает награду опытом.
        /// </summary>
        public int GetXPReward()
        {
            var settings = CurrentTimeOfDay.GetSettings();
            return settings.XPReward;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Выводит весь прогресс в консоль.
        /// </summary>
        public void LogAllProgress()
        {
            Debug.Log("═══════════════════════════════");
            Debug.Log("📊 Отчёт о прогрессе");
            Debug.Log("═══════════════════════════════");

            for (int i = 0; i < TotalBaseLevels; i++)
            {
                var levelInfo = GetLevelInfo(i);
                string levelName = levelInfo?.DisplayName ?? $"Уровень {i + 1}";
                string morningStatus = IsMorningCompleted(i) ? "✅" : "❌";
                string eveningStatus = IsEveningCompleted(i) ? "✅" : "❌";
                int totalStars = GetTotalStars(i);
                
                Debug.Log($"{levelName}: Утро {morningStatus} | Вечер {eveningStatus} | {totalStars}/6 ★");
            }

            Debug.Log("═══════════════════════════════");
            string todName = CurrentTimeOfDay.GetDisplayName();
            string partInfo = IsMorningActive 
                ? $"Утро #{currentBaseLevelIndex + 1}" 
                : $"Вечер #{currentBaseLevelIndex + 1}";
            string partIndex = IsMorningActive ? currentLevelMorningPartIndex.ToString() : currentLevelEveningPartIndex.ToString();
            Debug.Log($"📍 Текущая часть: {partInfo} (Базовый уровень {CurrentBaseLevelNumber}, {todName}, часть #{partIndex})");
            Debug.Log("═══════════════════════════════");
            Debug.Log("⏱️ Настройки времени по сложности:");
            Debug.Log($"Новичок: 3★={DifficultyTimeSettings.Beginner.ExcellentTime}s, 2★={DifficultyTimeSettings.Beginner.GoodTime}s, 1★={DifficultyTimeSettings.Beginner.BasicTime}s, Лимит={DifficultyTimeSettings.Beginner.TimeLimit}s");
            Debug.Log($"Профессионал: 3★={DifficultyTimeSettings.Professional.ExcellentTime}s, 2★={DifficultyTimeSettings.Professional.GoodTime}s, 1★={DifficultyTimeSettings.Professional.BasicTime}s, Лимит={DifficultyTimeSettings.Professional.TimeLimit}s");
            Debug.Log($"Экстремал: 3★={DifficultyTimeSettings.Extremal.ExcellentTime}s, 2★={DifficultyTimeSettings.Extremal.GoodTime}s, 1★={DifficultyTimeSettings.Extremal.BasicTime}s, Лимит={DifficultyTimeSettings.Extremal.TimeLimit}s");
            Debug.Log("═══════════════════════════════");
        }

        #endregion
    }
}
