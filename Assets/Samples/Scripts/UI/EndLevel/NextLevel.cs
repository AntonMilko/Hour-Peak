using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    /// <summary>
    /// Управление переходом на следующий уровень.
    /// Кнопка "Далее":
    /// - Скрыта при старте уровня
    /// - Скрыта при истечении времени (неудача)
    /// - Показана только после успешного достижения пункта назначения
    /// Автосохранение при переходе между частями уровня и уровнями.
    /// </summary>
    public class NextLevel : MonoBehaviour
    {
        #region Constants

        private const string SAVE_FOLDER_NAME = "GameSaves";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string GLOBAL_SAVE_FILENAME = "global_save.json";

        #endregion

        #region Fields

        [Header("UI References")]
        [Tooltip("Кнопка 'Далее' (успех)")]
        [SerializeField] private Button nextLevelButton;
        
        [Tooltip("Кнопка 'Далее' (при неудаче/время вышло)")]
        [SerializeField] private Button nextLevelButtonFailed;

        [Header("Settings")]
        [Tooltip("Автосохранение при переходе")]
        [SerializeField] private bool autoSaveOnTransition = true;
        
        [Tooltip("Требовать сбор звёзд для успеха")]
        [SerializeField] private bool requireStarsForSuccess = true;
        
        [Tooltip("Требуемое количество звёзд")]
        [SerializeField] private int requiredStars = 3;

        [Tooltip("Собранное количество звёзд")]
        [SerializeField] private int starsCollected = 0;

        #endregion

        #region Private State

        private int currentLevelNumber;
        private int currentPartIndex;
        private string nextLevelSceneName;
        private bool isLevelActive;
        private bool isCurrentLevel;
        private bool isCurrentPart;
        private bool levelSuccess;
        private bool reachedDestination;
        private string currentSaveFilePath;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            OnNextLevelButtonClick();
            OnNextPart();
            LoadNextPart();
            GetLastUnlockedLevel();
            FailedCurrentPart();
            OnFailed();
        }

        private void Update()
        {
            if (!isLevelActive)
                return;
            
            StartLevel();
            OnStarCollected();
            OnReachDestination();
            InitializeLevelState();
            CreateSaveFilePath();
            SaveProgress();
        }

        #endregion

        #region Public Methods

        public void StartLevel()
        {
            InitializeLevelState();
        }

        public void OnStarCollected()
        {
            if (!isLevelActive)
                return;
            
            starsCollected++;
            Debug.Log($"⭐ Звезда собрана! Всего: {starsCollected}/{requiredStars}");
        }

        public void OnReachDestination()
        {
            if (!isLevelActive || reachedDestination)
                return;
            
            reachedDestination = true;
            isLevelActive = false;
            
            if (requireStarsForSuccess)
            {
                levelSuccess = starsCollected >= requiredStars;
            }
            else
            {
                levelSuccess = true;
            }
            
            if (levelSuccess)
            {
                Debug.Log($"🏁 Пункт назначения достигнут! Звёзд: {starsCollected}/{requiredStars}");
            }
            else
            {
                Debug.Log($"❌ Не собрали звёзд! Собрано: {0}/{requiredStars}");
            }
            
            if (autoSaveOnTransition)
            {
                SaveProgress();
            }
        }

        public void OnNextLevelButtonClick()
        {
            if (!levelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя перейти на следующий уровень без успешного завершения!");
                return;
            }
            
            SaveProgress();
            LoadNextLevel();
        }

        public void OnNextPart()
        {
            if (!levelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя перейти на следующую часть без успешного завершения!");
                return;
            }
            
            SaveProgress();
            LoadNextPart();
        }

        public void OnFailed()
        {
            if (levelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя вызвать неудачу после успешного завершения!");
                return;
            }

            SaveProgress();
            FailedCurrentPart();
        }

        #endregion

        #region Timer Logic

        private void LoadNextPart()
        {
            // Проверяем, основной ли это уровень и часть
            bool isCurrentLevel = currentLevelNumber >= 1;
            bool isCurrentPart = currentPartIndex >= 1;
            
            if (isCurrentLevel && isCurrentPart)
            {
                // Основная часть основного уровня — переход к следующей части текущего (следующего) уровня
                Debug.Log("Основная часть пройдена! Переход к следующему");
                
                // Автосохранение перед переходом
                SaveProgress();
                
                // Переход к сцене следующей части текущего (следующего) уровня
                if (!string.IsNullOrEmpty(nextLevelSceneName))
                {
                    SceneManager.LoadScene(nextLevelSceneName);
                }
                else
                {
                    // Если nextLevelSceneName не задана — используем стандартное имя
                    SceneManager.LoadScene("Level Scene Index");
                }
                return;
            }
            
            currentPartIndex++;
            
            Debug.Log($"🔄 Переход на часть: Уровень {currentLevelNumber} Часть {currentPartIndex}");
            
            CreateSaveFilePath();
            StartLevel();
        }

        private void FailedCurrentPart()
        {
            Debug.LogWarning("Неудача! Отсутствие отображения кнопки Далее.");
            
            // Кнопка "Далее" скрыта при неудаче
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(false);
                nextLevelButton.interactable = false;
            }
            else
            {
                nextLevelButtonFailed.gameObject.SetActive(true);
                nextLevelButtonFailed.interactable = true;
            }
        }

        private void InitializeLevelState()
        {
            isLevelActive = true;
            levelSuccess = false;
            reachedDestination = false;
            starsCollected = 0;
        }

        #endregion

        #region UI Management

        private void SetNextLevelButtonVisible(bool visible)
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(visible);
                nextLevelButton.interactable = visible;
            }

            else
            {
                nextLevelButtonFailed.gameObject.SetActive(visible);
                nextLevelButtonFailed.interactable = visible;
            }
        }

        private void SetUIActive(GameObject obj, bool active)
        {
            if (obj != null)
                obj.SetActive(active);
        }

        #endregion

        #region Save/Load Logic

        private void CreateSaveFilePath()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            currentSaveFilePath = Path.Combine(folderPath, $"level_{currentLevelNumber:000}{SAVE_FILE_EXTENSION}");
        }

        public void SaveProgress()
        {
            GameData data = new GameData
            {
                lastUnlockedLevel = levelSuccess ? Mathf.Max(currentLevelNumber, GetLastUnlockedLevel() + (levelSuccess ? 1 : 0)) : GetLastUnlockedLevel(),
                currentLevel = currentLevelNumber,
                currentPart = currentPartIndex,
                starsCollected = starsCollected,
                saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
            };

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(currentSaveFilePath, json);
                
                SaveGlobalProgress(data.lastUnlockedLevel);
                
                Debug.Log($"💾 Сохранение: {currentSaveFilePath} | Уровень разблокирован: {data.lastUnlockedLevel}");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
            }
        }

        private void SaveGlobalProgress(int lastUnlockedLevel)
        {
            string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
            
            GlobalGameData data = new GlobalGameData
            {
                lastUnlockedLevel = lastUnlockedLevel,
                saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
            };

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(globalSavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка сохранения глобального прогресса: {e.Message}");
            }
        }

        private int GetLastUnlockedLevel()
        {
            string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
            
            if (File.Exists(globalSavePath))
            {
                try
                {
                    string json = File.ReadAllText(globalSavePath);
                    GlobalGameData data = JsonUtility.FromJson<GlobalGameData>(json);
                    return data.lastUnlockedLevel;
                }
                catch
                {
                    return 1;
                }
            }
            
            return 1;
        }

        #endregion

        #region Level Transition

        private void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelNumber + 1;
            
            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                Debug.Log($"🔄 Переход на уровень: {nextLevelSceneName}");
                SceneManager.LoadScene(nextLevelSceneName);
            }
            else
            {
                Debug.Log($"🔄 Переход на уровень: Level{nextLevelIndex}");
                SceneManager.LoadScene($"Level{nextLevelIndex}");
            }
        }

        #endregion

        #region Data Structures

        [Serializable]
        public class GameData
        {
            public int lastUnlockedLevel;
            public int currentLevel;
            public int currentPart;
            public int starsCollected;
            public float remainingTime;
            public string saveTimestamp;
        }

        [Serializable]
        public class GlobalGameData
        {
            public int lastUnlockedLevel;
            public string saveTimestamp;
        }

        #endregion
    }
}