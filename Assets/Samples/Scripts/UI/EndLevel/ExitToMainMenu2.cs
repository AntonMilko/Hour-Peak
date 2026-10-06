using System;
using System.IO;
using HourPeak.Samples.Runtime;
using HourPeak.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = System.Object;

namespace Samples.Scripts.UI.StartMenu
{
    /// <summary>
    /// Управление кнопкой выхода в главное меню.
    /// Скрывает кнопку при успешном прохождении уровня.
    /// Показывает кнопку при неуспешном прохождении или во время прохождения.
    /// </summary>
    public class ExitToMainMenu2 : MonoBehaviour
    {
        #region Constants

        private const string SAVE_FOLDER_NAME = "GameSaves";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string GLOBAL_SAVE_FILENAME = "global_save.json";

        #endregion

        #region Fields

        [Header("Exit Buttons")]
        [Tooltip("Кнопка успешного прохождения уровня")]
        [SerializeField] private Button exitToMainMenuButton;

        [Tooltip("Кнопка неуспешного прохождения уровня")]
        [SerializeField] private Button exitToMainMenuButtonFailed;

        [Tooltip("Кнопка выхода из PauseMenu (показывается во время прохождения)")]
        [SerializeField] private Button pauseMenuExitButton;

        [Header("Options")]
        [Tooltip("Переходить в главное меню (StartMenu)")]
        [SerializeField] private bool moveToMainMenu = false;

        [Tooltip("Оставаться в меню главы (LevelMenu)")]
        [SerializeField] private bool stayInChapterMainMenu = true;

        [Tooltip("Автосохранять текущее прохождение перед выходом?")]
        [SerializeField] private bool autoSaveOnExit = true;

        #endregion

        #region Private State

        private Object exitLevel;
        private string currentSaveFilePath;
        private string currentStartMenuSceneName;
        private string currentChapterStartMenuSceneName;
        private bool _levelPassedSuccessfully = false;
        private bool _levelFinished = false;

        #endregion

        #region Properties

        public bool LevelPassedSuccessfully
        {
            get => _levelPassedSuccessfully;
            set
            {
                _levelPassedSuccessfully = value;
                _levelFinished = true;
                UpdateExitButtonsVisibility();
            }
        }

        public bool LevelFinished
        {
            get => _levelFinished;
            private set
            {
                _levelFinished = value;
                UpdateExitButtonsVisibility();
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            OnExitToMainMenu();
            OnLevelPassedSuccessfully();
            SetupButtons();
            CleanupButtons();
            OnLevelPassedUnsuccessfully();
            
            exitLevel = Object();
        }

        private void Update()
        {
            if (_levelFinished)
                return;

            OnExitToMainMenu();
            OnLevelInProgress();
            LoadPreviousProgress();
            SaveRetryProgress();
            CreateSaveFilePath();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Вызывается при успешном прохождении уровня.
        /// </summary>
        public void OnLevelPassedSuccessfully()
        {
            LevelPassedSuccessfully = true;
            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(true);
            if (exitToMainMenuButtonFailed != null) exitToMainMenuButtonFailed.gameObject.SetActive(false);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(true);
            LevelFinished = true;
            Debug.Log("Уровень пройден успешно — кнопка выхода не показана");
        }

        /// <summary>
        /// Вызывается при неуспешном прохождении уровня.
        /// </summary>
        public void OnLevelPassedUnsuccessfully()
        {
            _levelPassedSuccessfully = false;
            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(false);
            if (exitToMainMenuButtonFailed != null) exitToMainMenuButtonFailed.gameObject.SetActive(true);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(true);
            LevelFinished = false;
            Debug.Log("Уровень пройден неуспешно — кнопка выхода показана");
        }

        /// <summary>
        /// Вызывается когда уровень ещё не завершён (например, при открытии паузы).
        /// </summary>
        public void OnLevelInProgress()
        {
            LevelFinished = true;
            Debug.Log("⏸️ Уровень в процессе — кнопка выхода показана");
        }

        /// <summary>
        /// ВЫХОД В ГЛАВНОЕ МЕНЮ.
        /// Сохраняет настройки сложности перед переходом.
        /// </summary>
        public void OnExitToMainMenu()
        {
            exitLevel = Object();
            Debug.Log("🚪 Выход в главное меню");

            // 1️⃣ АВТОСОХРАНЕНИЕ ТЕКУЩЕГО ПРОХОЖДЕНИЯ
            if (autoSaveOnExit)
            {
                SaveGlobalProgress(GetCurrentPlaythrough());
                Debug.Log("💾 Автосохранение текущего прохождения");
            }

            // 2️⃣ ПЕРЕХОД В ГЛАВНОЕ МЕНЮ
            if (moveToMainMenu)
            {
                if (!string.IsNullOrEmpty(currentStartMenuSceneName))
                {
                    SceneManager.LoadScene(currentStartMenuSceneName);
                    Debug.Log($"🏠 Переход в главное меню: {currentStartMenuSceneName}");
                }
                else
                {
                    Debug.LogWarning("⚠️ Имя сцены главного меню не задано!");
                }
            }
            else if (stayInChapterMainMenu)
            {
                if (!string.IsNullOrEmpty(currentChapterStartMenuSceneName))
                {
                    SceneManager.LoadScene(currentChapterStartMenuSceneName);
                    Debug.Log($"📂 Переход в меню главы: {currentChapterStartMenuSceneName}");
                }
                else
                {
                    Debug.LogWarning("⚠️ Имя сцены меню главы не задано!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Переход в главное меню не задан!");
            }
        }

        #endregion

        #region Save/Load Logic

        /// <summary>
        /// Создаёт путь к файлу сохранения для текущего прохождения.
        /// </summary>
        private void CreateSaveFilePath()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            currentSaveFilePath = Path.Combine(folderPath, $"StartMenu_{currentChapterStartMenuSceneName:LevelMenu}{SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// Сохраняет прогресс перезапуска.
        /// </summary>
        public void SaveRetryProgress()
        {
            GameData data = new GameData
            {
                currentPlaythrough = GetCurrentPlaythrough(),
                currentStartMenu = currentStartMenuSceneName,
                currentChapterStartMenu = currentChapterStartMenuSceneName,
                exitLevel = Object(),
                saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
            };

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(currentSaveFilePath, json);
                
                SaveGlobalProgress(data.currentPlaythrough);
                
                Debug.Log($"💾 Сохранение перезапуска: {currentSaveFilePath} | Выход из уровня: {exitLevel}");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
            }
        }

        /// <summary>
        /// Создаёт объект кнопки выхода в главное меню.
        /// </summary>
        private Button Object()
        {
            throw new NotImplementedException();
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
                    
                    exitLevel = data.ExitLevel();
                    Debug.Log($"💾 Загружено сохранение: Главное Меню {data.currentStartMenu} Часть Главного Меню {data.currentChapterStartMenu} | Выход из уровня: {exitLevel}");
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
        private void SaveGlobalProgress(int currentPlaythrough)
        {
            string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
            
            GlobalGameData data = new GlobalGameData
            {
                currentPlaythrough = currentPlaythrough,
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
        private int GetCurrentPlaythrough()
        {
            string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
            
            if (File.Exists(globalSavePath))
            {
                try
                {
                    string json = File.ReadAllText(globalSavePath);
                    GlobalGameData data = JsonUtility.FromJson<GlobalGameData>(json);
                    return data.currentPlaythrough;
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
        /// Настраивает кнопки выхода в главное меню.
        /// </summary>
        private void SetupButtons()
        {
            if (exitToMainMenuButton != null)
            {
                exitToMainMenuButton.onClick.RemoveAllListeners();
                exitToMainMenuButton.onClick.AddListener(OnLevelPassedSuccessfully);
            }

            if (exitToMainMenuButtonFailed != null)
            {
                exitToMainMenuButtonFailed.onClick.RemoveAllListeners();
                exitToMainMenuButtonFailed.onClick.AddListener(OnLevelPassedUnsuccessfully);
            }

            if (pauseMenuExitButton != null)
            {
                pauseMenuExitButton.onClick.RemoveAllListeners();
                pauseMenuExitButton.onClick.AddListener(OnExitToMainMenu);
            }
        }

        /// <summary>
        /// Удаляет кнопки выхода в главное меню.
        /// </summary>
        private void CleanupButtons()
        {
            // Не показываем кнопку, если уровень пройден успешно
            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(_levelFinished && _levelPassedSuccessfully);

            if (exitToMainMenuButton != null) exitToMainMenuButton.onClick.RemoveAllListeners();
            if (pauseMenuExitButton != null) pauseMenuExitButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Обновляет видимость кнопок выхода в главное меню.
        /// </summary>
        private void UpdateExitButtonsVisibility()
        {
            // Показываем кнопку, если уровень НЕ пройден успешно
            bool shouldShow = !_levelFinished || !_levelPassedSuccessfully;

            if (exitToMainMenuButtonFailed != null) exitToMainMenuButtonFailed.gameObject.SetActive(shouldShow);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(shouldShow);
        }

        #endregion

        #region Data Structures

        /// <summary>
        /// Данные сохранения уровня.
        /// </summary>
        [Serializable]
        public class GameData
        {
            public int currentPlaythrough;
            public string saveTimestamp;
            internal Button exitLevel;
            internal string currentStartMenu;
            internal string currentChapterStartMenu;

            internal Button ExitLevel()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Глобальные данные сохранения.
        /// </summary>
        [Serializable]
        public class GlobalGameData
        {
            public int currentPlaythrough;
            public string saveTimestamp;
        }

        #endregion
    }
}