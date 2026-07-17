using UnityEngine;
using UnityEngine.UI;
using System;

namespace HourPeak.Settings
{
    /// <summary>
    /// Менеджер сохранения настроек игры.
    /// Автоматически сохраняет все настройки при выходе из меню настроек.
    /// </summary>
    public class SavingSettingsGame : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Версия сохранения (для миграции при изменениях структуры).
        /// </summary>
        private const int SAVE_VERSION = 1;

        /// <summary>
        /// Ключ для версии сохранения.
        /// </summary>
        private const string PREFS_KEY_VERSION = "SettingsVersion";

        #endregion

        #region Configuration

        [Header("UI References")]
        [SerializeField] private Button exitSettingsButton;
        [SerializeField] private Button saveSettingsButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private Button confirmExitButton;

        #endregion

        #region Properties

        /// <summary>
        /// Флаг выхода из настроек.
        /// </summary>
        private bool IsExiting { get; set; }

        #endregion

        #region Singleton

        private static SavingSettingsGame _instance;

        /// <summary>
        /// Глобальный экземпляр.
        /// </summary>
        public static SavingSettingsGame Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SavingSettingsGame>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SavingSettingsGame");
                        _instance = go.AddComponent<SavingSettingsGame>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие при успешном сохранении настроек.
        /// </summary>
        public event Action OnSettingsSaved;

        /// <summary>
        /// Событие при выходе из настроек.
        /// </summary>
        public event Action OnExitSettings;

        /// <summary>
        /// Событие при отмене выхода.
        /// </summary>
        public event Action OnCancelExit;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            InitializeUI();
            Debug.Log("✅ SavingSettingsGame инициализирован");
        }

        private void OnDestroy()
        {
            RemoveUIListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует UI элементы и слушатели событий.
        /// </summary>
        private void InitializeUI()
        {
            if (saveSettingsButton != null)
            {
                saveSettingsButton.onClick.AddListener(OnSaveSettingsClicked);
            }

            Debug.Log("✅ UI слушатели инициализированы");
        }

        /// <summary>
        /// Удаляет слушатели UI для предотвращения утечек памяти.
        /// </summary>
        private void RemoveUIListeners()
        {
            if (saveSettingsButton != null)
                saveSettingsButton.onClick.RemoveListener(OnSaveSettingsClicked);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Сохраняет все настройки игры.
        /// </summary>
        public void SaveAllSettings()
        {
            try
            {
                Debug.Log("💾 Сохранение настроек...");

                // Сохраняем сложность через Singleton
                SettingDifficulty.Instance.SaveDifficulty();
                Debug.Log($"✅ Сложность: {SettingDifficulty.Instance.GetCurrentDifficultyName()}");

                // Сохраняем чувствительность через Singleton
                SettingSensitivity.Instance.SaveSettings();
                Debug.Log("✅ Чувствительность сохранена");

                // Сохраняем версию
                PlayerPrefs.SetInt(PREFS_KEY_VERSION, SAVE_VERSION);
                PlayerPrefs.Save();

                OnSettingsSaved?.Invoke();
                Debug.Log("✅ Все настройки успешно сохранены");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает все настройки игры.
        /// </summary>
        public void LoadAllSettings()
        {
            try
            {
                Debug.Log("📂 Загрузка настроек...");

                // Настройки загружаются автоматически в Singleton
                Debug.Log($"✅ Сложность загружена: {SettingDifficulty.Instance.GetCurrentDifficultyName()}");
                Debug.Log("✅ Чувствительность загружена");

                Debug.Log("✅ Все настройки успешно загружены");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Сбрасывает все настройки к значениям по умолчанию.
        /// </summary>
        public void ResetAllSettingsToDefault()
        {
            try
            {
                Debug.Log("🔄 Сброс настроек к значениям по умолчанию...");

                // Сброс сложности через Singleton
                SettingDifficulty.Instance.SetDifficulty(DifficultyLevel.Beginner);
                Debug.Log("✅ Сложность сброшена на: Новичок");

                // Сброс чувствительности через Singleton
                SettingSensitivity.Instance.ResetToDefaults();
                Debug.Log("✅ Чувствительность сброшена");

                // Сохраняем сброс
                SaveAllSettings();

                Debug.Log("✅ Все настройки сброшены");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Ошибка при сбросе настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Очищает все сохранённые настройки.
        /// </summary>
        public void ClearAllSettings()
        {
            try
            {
                Debug.Log("🗑️ Очистка всех настроек...");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("✅ Все настройки очищены");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Ошибка при очистке настроек: {ex.Message}");
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Обработчик нажатия на кнопку сохранения настроек.
        /// </summary>
        private void OnSaveSettingsClicked()
        {
            Debug.Log("💾 Нажата кнопка сохранения настроек");
            SaveAllSettings();
            OnExitSettings?.Invoke();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку отмены.
        /// </summary>
        private void OnCancelClicked()
        {
            Debug.Log("❌ Нажата кнопка отмены");
            OnCancelExit?.Invoke();
        }

        #endregion

        #region Debug

        /// <summary>
        /// Выводит все сохранённые настройки в консоль.
        /// </summary>
        public void DebugLogAllSettings()
        {
            Debug.Log("═══════════════════════════════");
            Debug.Log("📊 Все сохранённые настройки");
            Debug.Log("═══════════════════════════════");

            Debug.Log($"Версия сохранения: {PlayerPrefs.GetInt(PREFS_KEY_VERSION, 0)}");

            // Сложность через Singleton
            SettingDifficulty.Instance.DebugLogAllDifficulties();

            // Чувствительность через Singleton
            SettingSensitivity.Instance.DebugLogSettings();

            Debug.Log("═══════════════════════════════");
        }

        #endregion
    }
}