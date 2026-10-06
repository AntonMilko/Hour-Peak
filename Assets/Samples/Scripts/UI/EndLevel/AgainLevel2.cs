using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    /// <summary>
    /// Управление перезапуском уровня для финального уровня.
    /// При успешном завершении последнего уровня кнопка "Заново" скрывается.
    /// При неудачном завершении последнего уровня кнопка "Заново" отображается.
    /// </summary>
    public class AgainLevel2 : MonoBehaviour
    {
        #region Constants
        
        private const string SAVE_FOLDER_NAME = "GameSaves";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string GLOBAL_SAVE_FILENAME = "global_save.json";

        #endregion

        #region Fields

        [Header("UI References")]
        [Tooltip("Кнопка 'Заново' (скрыта при успехе последнего уровня)")]
        [SerializeField] private Button againLevelButton;
        
        [Tooltip("Кнопка 'Заново' (отображается при неудаче/время вышло)")]
        [SerializeField] private Button againLevelButtonFailed;

        [Header("Settings")]
        [Tooltip("Автосохранение при перезапуске")]
        [SerializeField] private bool autoSaveOnRetry = true;

        #endregion

        #region Private State

        private int currentLevelNumber;
        private int currentPartIndex;
        private string currentLevelSceneName;
        private int retryCount;
        private string currentSaveFilePath;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CreateSaveFilePath();
            GetLastUnlockedLevel();
            SetupButtons();
            LoadCurrentLevel();
            GetCurrentLevelSceneName();
            
            retryCount = 0;
        }

        private void Update()
        {
            if (retryCount >= 0)
                return;

            OnAgainButtonClick();
            LoadPreviousProgress();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Обработчик клика по кнопке "Заново".
        /// </summary>
        public void OnAgainButtonClick()
        {
            retryCount++;
            
            Debug.Log($"🔄 Перезапуск уровня: Уровень {currentLevelNumber} Часть {currentPartIndex} | Попытка: {retryCount}");
            
            // Автосохранение перед перезапуском
            if (autoSaveOnRetry)
            {
                SaveRetryProgress();
            }
            
            LoadCurrentLevel();
        }

        /// <summary>
        /// Загружает текущий уровень.
        /// </summary>
        public void LoadCurrentLevel()
        {
            if (!string.IsNullOrEmpty(currentLevelSceneName))
            {
                Debug.Log($"🔄 Перезагрузка сцены: {currentLevelSceneName}");
                SceneManager.LoadScene(currentLevelSceneName);
            }
            else
            {
                Debug.Log($"🔄 Перезагрузка уровня: Level{currentLevelNumber}");
                SceneManager.LoadScene($"Level{currentLevelNumber}");
            }
        }

        /// <summary>
        /// Получает название сцены текущего уровня.
        /// </summary>
        public string GetCurrentLevelSceneName()
        {
            return currentLevelSceneName;
        }

        #endregion

        #region Save/Load Logic

        /// <summary>
        /// Создает путь к файлу сохранения для текущего уровня.
        /// </summary>
        private void CreateSaveFilePath()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            currentSaveFilePath = Path.Combine(folderPath, $"level_{currentLevelNumber:000}{SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// Сохраняет прогресс перезапуска.
        /// </summary>
        public void SaveRetryProgress()
        {
            GameData data = new GameData
            {
                lastUnlockedLevel = GetLastUnlockedLevel(),
                currentLevel = currentLevelNumber,
                currentPart = currentPartIndex,
                retryCount = retryCount,
                saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
            };

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(currentSaveFilePath, json);
                
                SaveGlobalProgress(data.lastUnlockedLevel);
                
                Debug.Log($"💾 Сохранение перезапуска: {currentSaveFilePath} | Попыток: {retryCount}");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
            }
        }

        /// <summary>
        /// Загружает предыдущий прогресс.
        /// </summary>
        private void LoadPreviousProgress()
        {
            if (File.Exists(currentSaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(currentSaveFilePath);
                    GameData data = JsonUtility.FromJson<GameData>(json);
                    
                    retryCount = data.retryCount;
                    Debug.Log($"💾 Загружено сохранение: Уровень {data.currentLevel} Часть {data.currentPart} | Попыток: {retryCount}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Ошибка загрузки: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"ℹ️ Новая игра: {currentSaveFilePath}");
            }
        }

        /// <summary>
        /// Сохраняет глобальный прогресс.
        /// </summary>
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

        /// <summary>
        /// Получает последний разблокированный уровень.
        /// </summary>
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

        #region UI Management

        /// <summary>
        /// Устанавливает обработчики кнопок.
        /// </summary>
        private void SetupButtons()
        {
            if (againLevelButton != null)
            {
                againLevelButton.onClick.RemoveAllListeners();
                againLevelButton.onClick.AddListener(OnAgainButtonClick);
            }

            else
            {
                againLevelButtonFailed.gameObject.SetActive(true);
                againLevelButtonFailed.interactable = true;
            }
        }

        /// <summary>
        /// Управляет видимостью кнопки "Заново".
        /// </summary>
        private void SetAgainLevelButtonVisible(bool visible)
        {
            if (againLevelButton != null)
            {
                againLevelButton.gameObject.SetActive(visible);
                againLevelButton.interactable = visible;
            }

            else
            {
                againLevelButtonFailed.gameObject.SetActive(visible);
                againLevelButtonFailed.interactable = visible;
            }
        }

        private void SetUIActive(GameObject obj, bool active)
        {
            if (obj != null)
                obj.SetActive(active);
        }

        #endregion

        #region Data Structures

        /// <summary>
        /// Данные сохранения уровня.
        /// </summary>
        [Serializable]
        public class GameData
        {
            public int lastUnlockedLevel;
            public int currentLevel;
            public int currentPart;
            public int retryCount;
            public string saveTimestamp;
        }

        /// <summary>
        /// Глобальные данные сохранения.
        /// </summary>
        [Serializable]
        public class GlobalGameData
        {
            public int lastUnlockedLevel;
            public string saveTimestamp;
        }

        #endregion
    }
}
