using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Samples.Runtime;

namespace HourPeak
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
        private const string START_MENU_SCENE_NAME = "StartMenu";
        private const string LEVEL_MENU_SCENE_NAME = "LevelMenu";

        #endregion

        #region Fields

        [Header("Exit Buttons")]
        [Tooltip("Кнопка успешного прохождения уровня")]
        [SerializeField] private Button exitToMainMenuButton;

        [Tooltip("Кнопка неуспешного прохождения уровня")]
        [SerializeField] private Button exitToMainMenuButtonFail;

        [Tooltip("Кнопка выхода из PauseMenu (показывается во время прохождения)")]
        [SerializeField] private Button pauseMenuExitButton;

        [Header("Continue Manager")]
        [Tooltip("Менеджер, который хранит настройки сложности")]
        [SerializeField] private Continue continuationManager;

        [Header("Settings")]
        [Tooltip("Сцена главного меню")]
        [SerializeField] private string mainMenuScene = "StartMenu";

        [Tooltip("Сцена выбора уровней")]
        [SerializeField] private string partMainMenuScene = "LevelMenu";

        [Tooltip("Автосохранение при выходе")]
        [SerializeField] private bool autoSaveOnExit = true;

        [Tooltip("Переходить в StartMenu")]
        [SerializeField] private bool goToStartMenu = false;

        [Tooltip("Оставаться в LevelMenu после StartMenu")]
        [SerializeField] private bool stayInLevelMenu = true;

        #endregion

        #region Private State

        private bool _levelPassedSuccessfully = false;
        private bool _levelFinished = false;

        #endregion

        #region Properties

        /// <summary>
        /// Прошёл ли уровень успешно.
        /// </summary>
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

        /// <summary>
        /// Завершён ли уровень (успешно или нет).
        /// </summary>
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
            SetupButtons();
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Вызывается при успешном прохождении уровня.
        /// Скрывает кнопку выхода из EndMenu.
        /// </summary>
        public void OnLevelPassedSuccessfully()
        {
            LevelPassedSuccessfully = true;
            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(false);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(false);
            Debug.Log("✅ Уровень пройден успешно — кнопка выхода скрыта");
        }

        /// <summary>
        /// Вызывается при неуспешном прохождении уровня.
        /// Показывает кнопку выхода из EndMenu.
        /// </summary>
        public void OnLevelPassedUnsuccessfully()
        {
            _levelPassedSuccessfully = false;
            if (exitToMainMenuButtonFail != null) exitToMainMenuButtonFail.gameObject.SetActive(true);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(true);
            LevelFinished = true;
            Debug.Log("❌ Уровень пройден неуспешно — кнопка выхода показана");
        }

        /// <summary>
        /// Вызывается когда уровень ещё не завершён (например, при открытии паузы).
        /// Показывает кнопку выхода из PauseMenu.
        /// </summary>
        public void OnLevelInProgress()
        {
            LevelFinished = false;
            Debug.Log("⏸️ Уровень в процессе — кнопка выхода показана");
        }

        /// <summary>
        /// Сбрасывает состояние (при загрузке нового уровня).
        /// </summary>
        public void ResetLevelState()
        {
            _levelPassedSuccessfully = false;
            LevelFinished = false;
            Debug.Log("🔄 Состояние уровня сброшено");
        }

        /// <summary>
        /// Переход в главное меню.
        /// </summary>
        public void ExitToMainMenu()
        {
            if (autoSaveOnExit && continuationManager != null)
            {
                continuationManager.SaveCurrentSettings();
                Debug.Log("💾 Настройки сохранены перед выходом");
            }

            if (goToStartMenu)
            {
                Debug.Log("🚪 Переход в главное меню");
                SceneManager.LoadScene(mainMenuScene);
            }
            else if (stayInLevelMenu)
            {
                Debug.Log("🚪 Переход в меню выбора уровней");
                SceneManager.LoadScene(partMainMenuScene);
            }
        }

        /// <summary>
        /// Переход в меню выбора уровней.
        /// </summary>
        public void ExitToLevelMenu()
        {
            if (autoSaveOnExit && continuationManager != null)
            {
                continuationManager.SaveCurrentSettings();
                Debug.Log("💾 Настройки сохранены перед выходом");
            }

            if (stayInLevelMenu)
            {
                Debug.Log("🚪 Переход в меню выбора уровней");
                SceneManager.LoadScene(partMainMenuScene);
            }
            else if (goToStartMenu)
            {
                Debug.Log("🚪 Переход в главное меню");
                SceneManager.LoadScene(mainMenuScene);
            }
        }

        #endregion

        #region UI Management

        private void SetupButtons()
        {
            if (exitToMainMenuButton != null)
            {
                exitToMainMenuButton.onClick.RemoveAllListeners();
                exitToMainMenuButton.onClick.AddListener(OnLevelPassedSuccessfully);
            }

            if (exitToMainMenuButtonFail != null)
            {
                exitToMainMenuButtonFail.onClick.RemoveAllListeners();
                exitToMainMenuButtonFail.onClick.AddListener(OnLevelPassedUnsuccessfully);
            }

            if (pauseMenuExitButton != null)
            {
                pauseMenuExitButton.onClick.RemoveAllListeners();
                pauseMenuExitButton.onClick.AddListener(ExitToLevelMenu);
            }
        }

        private void CleanupButtons()
        {
            if (exitToMainMenuButton != null)
            {
                exitToMainMenuButton.onClick.RemoveAllListeners();
            }

            if (exitToMainMenuButtonFail != null)
            {
                exitToMainMenuButtonFail.onClick.RemoveAllListeners();
            }

            if (pauseMenuExitButton != null)
            {
                pauseMenuExitButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Обновляет видимость кнопок выхода в зависимости от статуса уровня.
        /// </summary>
        private void UpdateExitButtonsVisibility()
        {
            // Кнопка выхода показывается, если:
            // 1. Уровень ещё не завершён (во время игры)
            // 2. Уровень завершён, но неуспешно
            // Кнопка выхода скрывается, только если уровень пройден успешно
            bool shouldShow = !_levelFinished || !_levelPassedSuccessfully;

            if (exitToMainMenuButton != null)
                exitToMainMenuButton.gameObject.SetActive(shouldShow);

            if (exitToMainMenuButtonFail != null)
                exitToMainMenuButtonFail.gameObject.SetActive(shouldShow);

            if (pauseMenuExitButton != null)
                pauseMenuExitButton.gameObject.SetActive(shouldShow);
        }

        #endregion
    }
}
