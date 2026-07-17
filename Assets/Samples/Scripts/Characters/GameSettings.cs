using UnityEngine;
using HourPeak.Settings;
using System;

namespace HourPeak.Characters
{
    /// <summary>
    /// Менеджер настроек NPC, адаптирующийся под уровень сложности.
    /// Читает CrowdMultiplier из SettingDifficulty и вычисляет количество NPC.
    /// 
    /// Проценты толпы:
    /// - Новичок:    20% (минимальная видимость)
    /// - Профессионал: 50% (средняя видимость)
    /// - Экстремал:   80% (максимальная видимость)
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Базовое количество NPC-прохожих (до применения множителя).
        /// </summary>
        private const int BaseNPCCount = 100000;

        #endregion

        #region Properties

        /// <summary>
        /// Множитель толпы из текущего уровня сложности.
        /// </summary>
        public float CrowdMultiplier => _currentSettings?.CrowdMultiplier ?? 0.2f;

        /// <summary>
        /// Процент толпы в читаемом виде (20, 50 или 80).
        /// </summary>
        public float CrowdDensityPercentage => CrowdMultiplier * 100f;

        /// <summary>
        /// Текущее количество NPC на уровне.
        /// </summary>
        public int CurrentNPCCount { get; private set; }

        #endregion

        #region Private Fields

        private SettingDifficulty _difficultyManager;
        private DifficultySettings _currentSettings;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _difficultyManager = SettingDifficulty.Instance;
            if (_difficultyManager == null)
            {
                Debug.LogError("❌ SettingDifficulty не найден!");
                return;
            }

            ApplyDifficultySettings();
        }

        private void Start()
        {
            if (_difficultyManager != null)
                _difficultyManager.OnDifficultyChanged += OnDifficultyChanged;
        }

        private void OnDestroy()
        {
            if (_difficultyManager != null)
                _difficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Применяет настройки сложности из SettingDifficulty.
        /// </summary>
        public void ApplyDifficultySettings()
        {
            if (_difficultyManager == null) return;

            _currentSettings = _difficultyManager.CurrentSettings;
            CurrentNPCCount = CalculateNPCCount(CrowdMultiplier);
        }

        /// <summary>
        /// Вычисляет количество NPC на основе множителя толпы.
        /// </summary>
        public int CalculateNPCCount(float multiplier)
        {
            return Mathf.RoundToInt(BaseNPCCount * multiplier);
        }

        /// <summary>
        /// Переключает сложность на следующий уровень.
        /// </summary>
        public void CycleDifficulty()
        {
            _difficultyManager?.SetDifficultyChange();
        }

        #endregion

        #region Event Handlers

        private void OnDifficultyChanged(DifficultyLevel level, DifficultySettings settings)
        {
            _currentSettings = settings;
            CurrentNPCCount = CalculateNPCCount(CrowdMultiplier);

            Debug.Log($"📊 NPC обновлено: {settings.DisplayName} — {CurrentNPCCount} ({CrowdDensityPercentage:F0}%)");
        }

        #endregion

        #region Debug

        public void DebugLogSettings()
        {
            var name = _currentSettings?.DisplayName ?? "Неизвестно";
            Debug.Log($"📊 NPC Settings | {name} | NPC: {CurrentNPCCount} ({CrowdDensityPercentage:F0}%)");
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Debug Log Settings")]
        private void DebugLogMenu()
        {
            DebugLogSettings();
        }
#endif

        #endregion
    }
}