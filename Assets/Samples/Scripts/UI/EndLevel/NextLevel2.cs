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
    public class NextLevel2 : MonoBehaviour
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

        private string certificateSceneName;
        private bool isCertificateActive;
        private bool lastLevelSuccess;
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
            FailedLastLevel();
            OnFailed();
        }

        private void Update()
        {
            if (!isCertificateActive)
                return;
            
            LastLevel();
            OnStarCollected();
            OnReachDestination();
            InitializeLevelState();
            CreateSaveFilePath();
            SaveProgress();
        }

        #endregion

        #region Public Methods

        public void LastLevel()
        {
            InitializeLevelState();
        }

        public void OnStarCollected()
        {
            if (!isCertificateActive)
                return;
            
            starsCollected++;
            Debug.Log($"⭐ Звезда собрана! Всего: {starsCollected}/{requiredStars}");
        }

        public void OnReachDestination()
        {
            if (!isCertificateActive || reachedDestination)
                return;
            
            reachedDestination = true;
            isCertificateActive = false;
            
            if (requireStarsForSuccess)
            {
                lastLevelSuccess = starsCollected >= requiredStars;
            }
            else
            {
                lastLevelSuccess = true;
            }
            
            if (lastLevelSuccess)
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
            if (!lastLevelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя перейти на следующий уровень без успешного завершения!");
                return;
            }
            
            SaveProgress();
            LoadNextLevel();
        }

        public void OnNextPart()
        {
            if (!lastLevelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя перейти на следующую часть без успешного завершения!");
                return;
            }
            
            SaveProgress();
            LoadNextPart();
        }

        public void OnFailed()
        {
            if (lastLevelSuccess)
            {
                Debug.LogWarning("⚠️ Нельзя вызвать неудачу после успешного завершения!");
                return;
            }

            SaveProgress();
            FailedLastLevel();
        }

        #endregion

        #region Timer Logic

        private void LoadNextPart()
        {            
            if (isCertificateActive)
            {
                // Финальная часть финального уровня — переход к сертификату
                Debug.Log("🏆 Финальная часть пройдена! Переход к сертификату...");
                
                // Автосохранение перед переходом
                SaveProgress();
                
                // Переход к сцене сертификата
                if (!string.IsNullOrEmpty(certificateSceneName))
                {
                    SceneManager.LoadScene(certificateSceneName);
                }
                else
                {
                    // Если nextLevelSceneName не задана — используем стандартное имя
                    SceneManager.LoadScene("Game completion certificate");
                }
                return;
            }
            
            lastLevelSuccess = true;
            
            Debug.Log($"🔄 Переход на часть: Сертификат {certificateSceneName}");
            
            CreateSaveFilePath();
            LastLevel();
        }

        private void FailedLastLevel()
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
            isCertificateActive = true;
            lastLevelSuccess = false;
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
            
            currentSaveFilePath = Path.Combine(folderPath, $"Last Level_{isCertificateActive:Game completion certificate}{SAVE_FILE_EXTENSION}");
        }

        public void SaveProgress()
        {
            GameData data = new GameData
            {
                lastUnlockedLevel = lastLevelSuccess ? Mathf.Max(GetLastUnlockedLevel() + (lastLevelSuccess ? 1 : 0)) : GetLastUnlockedLevel(),
                certificateSceneName = lastLevelSuccess,
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

        private void SaveGlobalProgress(object lastUnlockedLevel)
        {
            throw new NotImplementedException();
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
            if (!string.IsNullOrEmpty(certificateSceneName))
            {
                Debug.Log($"🔄 Переход на сертификат: {certificateSceneName}");
                SceneManager.LoadScene(certificateSceneName);
            }
        }

        #endregion

        #region Data Structures

        [Serializable]
        public class GameData
        {
            public float remainingTime;
            public float levelTimeLimit;
            public bool isCertificateActive;
            public bool lastLevelSuccess;
            public bool reachedDestination;
            public string currentSaveFilePath;
            public SettingDifficulty difficultyManager;
            internal object certificateSceneName;
            internal string saveTimestamp;
            internal int starsCollected;
            internal object lastUnlockedLevel;
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