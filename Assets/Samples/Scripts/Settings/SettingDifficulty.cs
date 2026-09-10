using UnityEngine;
using System;
using HourPeak.Settings;

namespace HourPeak.Settings
{
    /// <summary>
    /// Уровни сложности игры.
    /// </summary>
    public enum DifficultyLevel
    {
        Beginner = 0,      // Новичок
        Professional = 1,  // Профессионал
        Extremal = 2       // Экстремал
    }

    /// <summary>
    /// Настройки сложности для отображения в UI.
    /// </summary>
    [Serializable]
    public class DifficultySettings
    {
        [Tooltip("Уровень сложности")]
        public DifficultyLevel Level;

        [Tooltip("Название для отображения")]
        public string DisplayName;

        [Tooltip("Множитель толпы (0.2 = 20%, 0.5 = 50%, 0.8 = 80%)")]
        [Range(0f, 1f)]
        public float CrowdMultiplier;

        [Tooltip("Ограничение времени (секунды)")]
        public float TimeLimit;

        [Tooltip("Допустимое время (секунды)")]
        public float AcceptableTime;

        /// <summary>
        /// Форматирует время в формат ЧЧ:ММ:СС.
        /// </summary>
        public static string FormatTime(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.Hours > 0)
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }
            return timeSpan.ToString(@"mm\:ss");
        }
    }

    /// <summary>
    /// Singleton менеджер для управления уровнем сложности.
    /// Автоматически инициализируется при первом обращении.
    /// </summary>
    public class SettingDifficulty : MonoBehaviour
    {
        #region Constants

        private const string PREFS_KEY_DIFFICULTY = "DifficultyLevel";

        #endregion

        #region Static Instance

        /// <summary>
        /// Singleton экземпляр.
        /// </summary>
        public static SettingDifficulty Instance { get; private set; }

        /// <summary>
        /// Все доступные уровни сложности.
        /// </summary>
        public static DifficultySettings[] AllDifficulties { get; private set; }

        #endregion

        #region Fields

        [Header("Difficulty Settings")]
        [SerializeField] private DifficultySettings beginnerSettings = new DifficultySettings
        {
            Level = DifficultyLevel.Beginner,
            DisplayName = "Новичок",
            CrowdMultiplier = 0.2f,
            TimeLimit = 120f,
            AcceptableTime = 240f
        };

        [SerializeField] private DifficultySettings professionalSettings = new DifficultySettings
        {
            Level = DifficultyLevel.Professional,
            DisplayName = "Профессионал",
            CrowdMultiplier = 0.5f,
            TimeLimit = 60f,
            AcceptableTime = 120f
        };

        [SerializeField] private DifficultySettings extremalSettings = new DifficultySettings
        {
            Level = DifficultyLevel.Extremal,
            DisplayName = "Экстремал",
            CrowdMultiplier = 0.8f,
            TimeLimit = 30f,
            AcceptableTime = 60f
        };

        #endregion

        #region Private State

        private DifficultyLevel _currentDifficulty;
        private DifficultySettings _currentSettings;

        #endregion

        #region Events

        /// <summary>
        /// Событие вызывается при изменении уровня сложности.
        /// </summary>
        public event Action<DifficultyLevel, DifficultySettings> OnDifficultyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Текущий уровень сложности.
        /// </summary>
        public DifficultyLevel CurrentDifficulty => _currentDifficulty;

        /// <summary>
        /// Текущие настройки сложности.
        /// </summary>
        public DifficultySettings CurrentSettings => _currentSettings;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDifficulties();
                LoadDifficulty();
                Debug.Log("✅ SettingDifficulty инициализирован");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует массив всех уровней сложности.
        /// </summary>
        private void InitializeDifficulties()
        {
            AllDifficulties = new DifficultySettings[]
            {
                beginnerSettings,
                professionalSettings,
                extremalSettings
            };

            // Устанавливаем текущую сложность по умолчанию
            _currentDifficulty = DifficultyLevel.Beginner;
            _currentSettings = GetSettingsByLevel(_currentDifficulty);

            Debug.Log("📊 Доступные сложности:");
            DebugLogAllDifficulties();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Получает настройки для указанного уровня сложности.
        /// </summary>
        public DifficultySettings GetSettingsByLevel(DifficultyLevel level)
        {
            for (int i = 0; i < AllDifficulties.Length; i++)
            {
                if (AllDifficulties[i].Level == level)
                    return AllDifficulties[i];
            }
            return beginnerSettings;
        }

        /// <summary>
        /// Получает текущие настройки сложности.
        /// </summary>
        public DifficultySettings GetCurrentSettings()
        {
            return _currentSettings;
        }

        /// <summary>
        /// Получает название текущего уровня сложности.
        /// </summary>
        public string GetCurrentDifficultyName()
        {
            return _currentSettings.DisplayName;
        }

        /// <summary>
        /// Устанавливает конкретный уровень сложности.
        /// </summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            if (_currentDifficulty == level)
            {
                Debug.Log($"⚠️ Уже установлена сложность: {GetCurrentDifficultyName()}");
                return;
            }

            _currentDifficulty = level;
            _currentSettings = GetSettingsByLevel(level);
            
            SaveDifficulty();
            NotifyDifficultyChanged();
            
            Debug.Log($"🎯 Сложность изменена на: {GetCurrentDifficultyName()}");
        }

        /// <summary>
        /// Переключает сложность на следующий уровень (циклически).
        /// </summary>
        public void SetDifficultyChange()
        {
            int nextIndex = ((int)_currentDifficulty + 1) % AllDifficulties.Length;
            SetDifficulty((DifficultyLevel)nextIndex);
        }

        /// <summary>
        /// Устанавливает сложность по индексу (0=Новичок, 1=Профессионал, 2=Экстремал).
        /// </summary>
        public void SetDifficultyByIndex(int index)
        {
            index = Mathf.Clamp(index, 0, AllDifficulties.Length - 1);
            SetDifficulty((DifficultyLevel)index);
        }

        /// <summary>
        /// Сохраняет текущую сложность.
        /// </summary>
        public void SaveDifficulty()
        {
            PlayerPrefs.SetInt(PREFS_KEY_DIFFICULTY, (int)_currentDifficulty);
            PlayerPrefs.Save();
            Debug.Log($"💾 Сложность сохранена: {GetCurrentDifficultyName()}");
        }

        /// <summary>
        /// Загружает сохранённую сложность.
        /// </summary>
        public void LoadDifficulty()
        {
            int savedIndex = PlayerPrefs.GetInt(PREFS_KEY_DIFFICULTY, 0);
            DifficultyLevel savedDifficulty = (DifficultyLevel)Mathf.Clamp(savedIndex, 0, AllDifficulties.Length - 1);
            
            _currentDifficulty = savedDifficulty;
            _currentSettings = GetSettingsByLevel(_currentDifficulty);
            
            Debug.Log($"📂 Сложность загружена: {GetCurrentDifficultyName()}");
        }

        /// <summary>
        /// Уведомляет слушателей об изменении сложности.
        /// </summary>
        private void NotifyDifficultyChanged()
        {
            OnDifficultyChanged?.Invoke(_currentDifficulty, _currentSettings);
        }

        /// <summary>
        /// Выводит все доступные сложности в консоль.
        /// </summary>
        public void DebugLogAllDifficulties()
        {
            Debug.Log("═══════════════════════════════");
            Debug.Log("📊 Доступные уровни сложности");
            Debug.Log("═══════════════════════════════");
            
            for (int i = 0; i < AllDifficulties.Length; i++)
            {
                var settings = AllDifficulties[i];
                Debug.Log($"{i}. {settings.DisplayName}");
                Debug.Log($"   Толпа: {settings.CrowdMultiplier * 100:F0}% | " +
                         $"Время: {DifficultySettings.FormatTime(settings.TimeLimit)} | " +
                         $"Допустимое: {DifficultySettings.FormatTime(settings.AcceptableTime)}");
            }
            
            Debug.Log("═══════════════════════════════");
            Debug.Log($"🎯 Текущая сложность: {GetCurrentDifficultyName()}");
            Debug.Log("═══════════════════════════════");
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Debug Log All Difficulties")]
        private void DebugLogMenu()
        {
            DebugLogAllDifficulties();
        }

        [ContextMenu("Set to Beginner")]
        private void SetToBeginner()
        {
            SetDifficulty(DifficultyLevel.Beginner);
        }

        [ContextMenu("Set to Professional")]
        private void SetToProfessional()
        {
            SetDifficulty(DifficultyLevel.Professional);
        }

        [ContextMenu("Set to Extremal")]
        private void SetToExtremal()
        {
            SetDifficulty(DifficultyLevel.Extremal);
        }

        [ContextMenu("Cycle Difficulty")]
        private void CycleDifficulty()
        {
            SetDifficultyChange();
        }

        internal int GetDifficultyMode()
        {
            throw new NotImplementedException();
        }
#endif

        #endregion
    }
}