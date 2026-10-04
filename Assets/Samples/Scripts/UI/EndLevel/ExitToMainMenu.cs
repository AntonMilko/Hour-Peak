using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Samples.Scripts.UI.StartMenu
{
    public class ExitToMainMenu : MonoBehaviour
    {
        #region Fields

        [Header("Exit Buttons")]
        [Tooltip("Кнопка успешного прохождения уровня (вызывает OnLevelPassedSuccessfully)")]
        [SerializeField] private Button exitToMainMenuButton;

        [Tooltip("Кнопка выхода из PauseMenu (показывается во время прохождения)")]
        [SerializeField] private Button pauseMenuExitButton;

        [Header("References")]
        [Tooltip("Менеджер настроек сложности (GameContinuationManager)")]
        [SerializeField] private Continue gameContinuationManager;

        [Header("Settings")]
        [Tooltip("Сцена LevelMenu (раздел выбора уровней)")]
        [SerializeField] private string levelMenuSceneName = "LevelMenu";

        [Tooltip("Сцена StartMenu (главное меню)")]
        [SerializeField] private string startMenuSceneName = "StartMenu";

        [Header("Options")]
        [Tooltip("Переходить в главное меню (StartMenu)")]
        [SerializeField] private bool moveToMainMenu = false;

        [Tooltip("Оставаться в меню главы (LevelMenu)")]
        [SerializeField] private bool stayInChapterMainMenu = true;

        [Tooltip("Автосохранять настройки сложности перед выходом?")]
        [SerializeField] private bool autoSaveOnExit = true;

        #endregion

        #region Private State

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

        #region Unity Editor Validation

        internal class Continue
        {
            internal void SaveCurrentSettings()
            {
                throw new NotImplementedException();
            }
        }

        internal class ContinueButton
        {
            internal void SaveCurrentSettings()
            {
                throw new NotImplementedException();
            }
        }

        internal class continuationManager
        {
            internal void SaveCurrentSettings()
            {
                throw new NotImplementedException();
            }

            internal void LoadSavedSettings()
            {
                throw new NotImplementedException();
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
        /// </summary>
        public void OnLevelPassedUnsuccessfully()
        {
            _levelPassedSuccessfully = false;
            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(true);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(true);
            LevelFinished = true;
            Debug.Log("❌ Уровень пройден неуспешно — кнопка выхода показана");
        }

        /// <summary>
        /// Вызывается когда уровень ещё не завершён (например, при открытии паузы).
        /// </summary>
        public void OnLevelInProgress()
        {
            LevelFinished = false;
            Debug.Log("⏸️ Уровень в процессе — кнопка выхода показана");
        }

        /// <summary>
        /// ВЫХОД В ГЛАВНОЕ МЕНЮ.
        /// Сохраняет настройки сложности перед переходом.
        /// </summary>
        public void OnExitToMainMenu()
        {
            Debug.Log("🚪 Выход в главное меню");

            // 1️⃣ АВТОСОХРАНЕНИЕ НАСТРОЕК СЛОЖНОСТИ
            if (autoSaveOnExit)
            {
                if (gameContinuationManager != null)
                {
                    gameContinuationManager.SaveCurrentSettings();
                    Debug.Log("💾 Настройки сложности сохранены перед выходом.");
                }
                else
                {
                    Debug.LogWarning("⚠️ GameContinuationManager не найден! Автосохранение не сработает.");
                }
            }

            // 2️⃣ ВЫБОР СЦЕНЫ ДЛЯ ПЕРЕХОДА
            if (stayInChapterMainMenu)
            {
                Debug.Log("📂 Переход в меню главы (LevelMenu)");
                SceneManager.LoadScene(levelMenuSceneName);
            }
            else if (moveToMainMenu)
            {
                Debug.Log("📂 Переход в главное меню (StartMenu)");
                SceneManager.LoadScene(startMenuSceneName);
            }
            else
            {
                // По умолчанию, если ни один флаг не установлен
                Debug.Log("📂 Переход в меню главы по умолчанию (LevelMenu)");
                SceneManager.LoadScene(levelMenuSceneName);
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

            if (pauseMenuExitButton != null)
            {
                pauseMenuExitButton.onClick.RemoveAllListeners();
                pauseMenuExitButton.onClick.AddListener(OnExitToMainMenu);
            }
        }

        private void CleanupButtons()
        {
            if (exitToMainMenuButton != null) exitToMainMenuButton.onClick.RemoveAllListeners();
            if (pauseMenuExitButton != null) pauseMenuExitButton.onClick.RemoveAllListeners();
        }

        private void UpdateExitButtonsVisibility()
        {
            // Показываем кнопку, если уровень НЕ пройден успешно
            bool shouldShow = !_levelFinished || !_levelPassedSuccessfully;

            if (exitToMainMenuButton != null) exitToMainMenuButton.gameObject.SetActive(shouldShow);
            if (pauseMenuExitButton != null) pauseMenuExitButton.gameObject.SetActive(shouldShow);
        }

        #endregion
    }
}