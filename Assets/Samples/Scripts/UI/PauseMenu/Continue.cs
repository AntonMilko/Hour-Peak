using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    public class Continue : MonoBehaviour
    {
        #region Fields

        [Header("Продолжить прохождение уровня")]
        [Tooltip("Продолжить уровень")]
        [SerializeField] private bool continueLevel = true;

        [Tooltip("Продолжить с контрольной точки")]
        [SerializeField] private bool continueFromCheckpoint = true;

        [Tooltip("Продолжить уровень с сохранённой сложностью")]
        [SerializeField] private bool continueLevelWithSavedSettings = true;

        [Tooltip("Текущие настройки")]
        [SerializeField] private bool currentSettings = true;

        #endregion

        #region Private State

        private const string KEY_SETTINGS = "CurrentSettings";
        private const string KEY_IS_SET = "SettingsIsSet";

        #endregion

        #region Settings Management

        private bool _continueLevel;
        private bool _continueFromCheckpoint;
        private bool _continueLevelWithSavedSettings;
        private bool _currentSettings;

        public bool ContinueLevel { get => _continueLevel; set => _continueLevel = value; }
        public bool ContinueFromCheckpoint { get => _continueFromCheckpoint; set => _continueFromCheckpoint = value; }
        public bool ContinueLevelWithSavedSettings { get => _continueLevelWithSavedSettings; set => _continueLevelWithSavedSettings = value; }
        public bool CurrentSettings { get => _currentSettings; set => _currentSettings = value; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            LoadSettings();
            _continueLevel = continueLevel;
            _continueFromCheckpoint = continueFromCheckpoint;
            _currentSettings = currentSettings;
        }

        private void Update()
        {
            ApplySettings();
            SaveCurrentSettings();
            _continueLevelWithSavedSettings = continueLevelWithSavedSettings;
        }

        #endregion

        #region Public Methods

        public void ApplySettings()
        {
            // Изменения настроек
            _continueLevelWithSavedSettings = continueLevelWithSavedSettings;

            if (!continueLevelWithSavedSettings)
            {
                continueLevel = true;
                continueFromCheckpoint = true;
                continueLevelWithSavedSettings = true;
                currentSettings = false;
            }

            else
            {
                continueLevel = true;
                continueFromCheckpoint = true;
                continueLevelWithSavedSettings = false;
                currentSettings = true;
            }
        }

        public void SaveCurrentSettings()
        {
            PlayerPrefs.SetInt(KEY_SETTINGS, currentSettings ? 1 : 0);
            PlayerPrefs.SetInt(KEY_IS_SET, continueLevelWithSavedSettings ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log("💾 Настройки сохранены перед перезагрузкой.");
        }

        #endregion

        #region Private Methods

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(KEY_SETTINGS))
            {
                _currentSettings = currentSettings;
                continueLevelWithSavedSettings = PlayerPrefs.GetInt(KEY_IS_SET) == 1;
                
                // 🔥 Автоматически синхронизируем изменения настроек
                _continueLevelWithSavedSettings = continueLevelWithSavedSettings;
            }
            
            ApplySettings();
        }

        #endregion
    }
}