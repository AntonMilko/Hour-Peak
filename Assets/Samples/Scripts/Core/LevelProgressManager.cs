using System;
using System.Collections.Generic;
using UnityEngine;

// Namespace убран — классы будут глобальными и видны везде

/// <summary>
/// Менеджер прогресса на уровне уровней (36 уровней × 2 части).
/// Связывает UI LevelMenuManager с ProgressManager (148 сцен).
/// </summary>
public class LevelProgressManager : MonoBehaviour
    {
        public static LevelProgressManager Instance { get; private set; }

        private const int TOTAL_LEVELS = 36;

        [Serializable]
        public class LevelProgress
        {
            public bool morningCompleted;
            public bool eveningCompleted;
            public int morningStars;
            public int eveningStars;
            public float morningTime;
            public float eveningTime;
        }

        private Dictionary<int, LevelProgress> _levelProgress = new Dictionary<int, LevelProgress>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Убедимся, что GameObject является root перед вызовом DontDestroyOnLoad
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[LevelProgressManager] GameObject не является root! Родитель: {transform.parent.name}");
        }
        
        InitializeProgress();
    }

        /// <summary>
        /// Инициализирует прогресс для всех 36 уровней.
        /// </summary>
        private void InitializeProgress()
        {
            for (int i = 0; i < TOTAL_LEVELS; i++)
            {
                if (!_levelProgress.ContainsKey(i))
                {
                    _levelProgress[i] = new LevelProgress();
                }
            }
            Debug.Log($"✅ LevelProgressManager инициализирован: {TOTAL_LEVELS} уровней");
        }

        /// <summary>
        /// Синхронизирует прогресс уровней с ProgressManager (148 сцен).
        /// Вызывается при загрузке LevelMenuManager.
        /// </summary>
        public void SyncFromProgressManager()
        {
            if (ProgressManager.Instance == null)
            {
                Debug.LogWarning("⚠️ ProgressManager не найден!");
                return;
            }

            for (int level = 1; level <= TOTAL_LEVELS; level++)
            {
                bool isSpecialLevel = (level == 1 || level == 19);
                int partsPerTime = isSpecialLevel ? 3 : 2;

                // Проверяем утро
                bool morningCompleted = true;
                int morningStars = 0;
                float morningTime = 0f;
                for (int part = 1; part <= partsPerTime; part++)
                {
                    string sceneName = $"Level {level}/Morning/Morning {level}.{part}";
                    if (!ProgressManager.Instance.IsSceneCompleted(sceneName))
                    {
                        morningCompleted = false;
                    }
                    morningStars += ProgressManager.Instance.GetSceneStars(sceneName);
                    morningTime += ProgressManager.Instance.GetSceneStars(sceneName) > 0 ? ProgressManager.Instance.GetSceneStars(sceneName) : 0f;
                }

                // Проверяем вечер
                bool eveningCompleted = true;
                int eveningStars = 0;
                float eveningTime = 0f;
                for (int part = partsPerTime; part >= 1; part--)
                {
                    string sceneName = $"Level {level}/Evening/Evening {level}.{part}";
                    if (!ProgressManager.Instance.IsSceneCompleted(sceneName))
                    {
                        eveningCompleted = false;
                    }
                    eveningStars += ProgressManager.Instance.GetSceneStars(sceneName);
                    eveningTime += ProgressManager.Instance.GetSceneStars(sceneName) > 0 ? ProgressManager.Instance.GetSceneStars(sceneName) : 0f;
                }

                // Обновляем прогресс уровня
                _levelProgress[level - 1].morningCompleted = morningCompleted;
                _levelProgress[level - 1].eveningCompleted = eveningCompleted;
                _levelProgress[level - 1].morningStars = morningStars;
                _levelProgress[level - 1].eveningStars = eveningStars;
            }

            Debug.Log("🔄 Прогресс уровней синхронизирован с ProgressManager");
        }

        /// <summary>
        /// Проверяет, разблокирован ли уровень.
        /// Уровень разблокирован, если предыдущий уровень полностью пройден.
        /// </summary>
        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= TOTAL_LEVELS) return false;
            if (levelIndex == 0) return true; // Первый уровень всегда разблокирован

            return _levelProgress[levelIndex - 1].morningCompleted && _levelProgress[levelIndex - 1].eveningCompleted;
        }

        /// <summary>
        /// Проверяет, пройдено ли утро уровня.
        /// </summary>
        public bool IsMorningCompleted(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < TOTAL_LEVELS && _levelProgress[levelIndex].morningCompleted;
        }

        /// <summary>
        /// Проверяет, пройдён ли вечер уровня.
        /// </summary>
        public bool IsEveningCompleted(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < TOTAL_LEVELS && _levelProgress[levelIndex].eveningCompleted;
        }

        /// <summary>
        /// Получает звёзды за утро.
        /// </summary>
        public int GetMorningStars(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < TOTAL_LEVELS ? _levelProgress[levelIndex].morningStars : 0;
        }

        /// <summary>
        /// Получает звёзды за вечер.
        /// </summary>
        public int GetEveningStars(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < TOTAL_LEVELS ? _levelProgress[levelIndex].eveningStars : 0;
        }

        /// <summary>
        /// Обновляет прогресс уровня после прохождения сцены.
        /// Вызывается из EndManager при сохранении прогресса.
        /// </summary>
        public void UpdateLevelProgress(int levelIndex, string sceneName, int stars, float time)
        {
            if (levelIndex < 0 || levelIndex >= TOTAL_LEVELS) return;

            bool isMorning = sceneName.Contains("Morning");
            bool isPart3 = sceneName.Contains($".3"); // Для уровней 1 и 19
            bool isLastPart = isPart3 || (!isPart3 && sceneName.Contains($".2"));

            if (isMorning)
            {
                _levelProgress[levelIndex].morningCompleted = true;
                _levelProgress[levelIndex].morningStars = stars;
                _levelProgress[levelIndex].morningTime = time;
            }
            else
            {
                _levelProgress[levelIndex].eveningCompleted = true;
                _levelProgress[levelIndex].eveningStars = stars;
                _levelProgress[levelIndex].eveningTime = time;
            }

            Debug.Log($"📊 Уровень {levelIndex + 1} обновлён: Утро={_levelProgress[levelIndex].morningCompleted}, Вечер={_levelProgress[levelIndex].eveningCompleted}");
        }

        /// <summary>
        /// Получает прогресс уровня.
        /// </summary>
        public LevelProgress GetLevelProgress(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < TOTAL_LEVELS ? _levelProgress[levelIndex] : null;
        }

        /// <summary>
        /// Получает общее количество звёзд.
        /// </summary>
        public int GetTotalStars()
        {
            int total = 0;
            foreach (var progress in _levelProgress.Values)
            {
                total += progress.morningStars + progress.eveningStars;
            }
            return total;
        }

        /// <summary>
        /// Получает количество пройденных уровней.
        /// </summary>
        public int GetCompletedLevels()
        {
            int completed = 0;
            foreach (var progress in _levelProgress.Values)
            {
                if (progress.morningCompleted && progress.eveningCompleted) completed++;
            }
            return completed;
        }
    }
