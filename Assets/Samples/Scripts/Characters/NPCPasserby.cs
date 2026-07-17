using UnityEngine;
using System;
using HourPeak.Settings;

namespace HourPeak.Characters
{
    /// <summary>
    /// NPC-прохожий, который адаптируется под уровень сложности.
    /// - Новичок: 20% толпы, медленные, осторожные
    /// - Профессионал: 50% толпы, средние
    /// - Экстремал: 80% толпы, быстрые, агрессивные
    /// </summary>
    public class NPCPasserby : MonoBehaviour
    {
        #region Fields

        [Header("Movement Settings")]
        [Tooltip("Базовая скорость движения")]
        [SerializeField] private float baseSpeed = 3f;

        [Tooltip("Множитель скорости для Новичка")]
        [SerializeField] private float beginnerSpeedMultiplier = 0.8f;

        [Tooltip("Множитель скорости для Профессионала")]
        [SerializeField] private float professionalSpeedMultiplier = 1.0f;

        [Tooltip("Множитель скорости для Экстремала")]
        [SerializeField] private float extremalSpeedMultiplier = 1.3f;

        [Header("Behavior Settings")]
        [Tooltip("Базовое время остановки")]
        [SerializeField] private float baseStopTime = 2f;

        [Tooltip("Множитель времени остановки для Новичка")]
        [SerializeField] private float beginnerStopMultiplier = 1.5f;

        [Tooltip("Множитель времени остановки для Профессионала")]
        [SerializeField] private float professionalStopMultiplier = 1.0f;

        [Tooltip("Множитель времени остановки для Экстремала")]
        [SerializeField] private float extremalStopMultiplier = 0.5f;

        [Header("Spawn Settings")]
        [Tooltip("Минимальная дистанция спавна")]
        [SerializeField] private float minSpawnDistance = 5f;

        [Tooltip("Максимальная дистанция спавна")]
        [SerializeField] private float maxSpawnDistance = 20f;

        public NPCPasserby(float maxSpawnDistance, float minSpawnDistance)
        {
            this.maxSpawnDistance = maxSpawnDistance;
            this.minSpawnDistance = minSpawnDistance;

        }

        #endregion

        #region Properties


        /// <summary>
        /// Текущая скорость движения.
        /// </summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>
        /// Текущее время остановки.
        /// </summary>
        public float CurrentStopTime { get; private set; }

        /// <summary>
        /// Текущий уровень сложности.
        /// </summary>
        public DifficultyLevel CurrentDifficulty
        {
            get
            {
                if (DifficultyManager == null)
                {
                    return DifficultyLevel.Beginner;
                }
                return DifficultyManager.CurrentDifficulty;
            }
        }

        /// <summary>
        /// Ссылка на SettingDifficulty.
        /// </summary>
        private SettingDifficulty DifficultyManager
        {
            get
            {
                if (_difficultyManager == null)
                {
                    _difficultyManager = SettingDifficulty.Instance;
                }
                return _difficultyManager;
            }
        }

        private SettingDifficulty _difficultyManager;

        /// <summary>
        /// Ссылка на GameSettings.
        /// </summary>
        private GameSettings GameSettings => GetComponent<GameSettings>();

        /// <summary>
        /// Активен ли NPC.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        #region Unity Lifecycle


        private void Awake()
        {
            // Получаем менеджер сложности
            _difficultyManager = SettingDifficulty.Instance;

            Debug.Log("✅ NPCPasserby инициализирован");
        }

        private void Start()
        {
            // Применяем начальные настройки
            ApplyDifficultySettings();

            // Подписываемся на изменения сложности
            if (DifficultyManager != null)
            {
                DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
            }

            // Активируем NPC
            IsActive = true;
            gameObject.SetActive(true);

            Debug.Log($"🎮 NPC активирован. Сложность: {GetDifficultyName()}");
        }

        private void Update()
        {
            // Движение NPC
            if (IsActive)
            {
                Move();
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (DifficultyManager != null)
            {
                DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Применяет настройки текущего уровня сложности.
        /// </summary>
        public void ApplyDifficultySettings()
        {
            float crowdMultiplier = DifficultyManager.CurrentSettings.CrowdMultiplier;

            // Устанавливаем скорость в зависимости от сложности
            CurrentSpeed = baseSpeed * GetSpeedMultiplier();
            CurrentStopTime = baseStopTime * GetStopMultiplier();

            Debug.Log($"🎯 Настройки применены: {GetDifficultyName()} | " +
                      $"Скорость: {CurrentSpeed:F2} | Остановка: {CurrentStopTime:F1}s");
        }

        /// <summary>
        /// Получает множитель скорости для текущей сложности.
        /// </summary>
        private float GetSpeedMultiplier()
        {
            return CurrentDifficulty switch
            {
                DifficultyLevel.Beginner => beginnerSpeedMultiplier,     // 0.8x (медленнее)
                DifficultyLevel.Professional => professionalSpeedMultiplier, // 1.0x (нормально)
                DifficultyLevel.Extremal => extremalSpeedMultiplier,     // 1.3x (быстрее)
                _ => professionalSpeedMultiplier
            };
        }

        /// <summary>
        /// Получает множитель времени остановки для текущей сложности.
        /// </summary>
        private float GetStopMultiplier()
        {
            return CurrentDifficulty switch
            {
                DifficultyLevel.Beginner => beginnerStopMultiplier,     // 1.5x (чаще останавливаются)
                DifficultyLevel.Professional => professionalStopMultiplier, // 1.0x (нормально)
                DifficultyLevel.Extremal => extremalStopMultiplier,     // 0.5x (реже останавливаются)
                _ => professionalStopMultiplier
            };
        }

        /// <summary>
        /// Получает название текущего уровня сложности.
        /// </summary>
        private string GetDifficultyName()
        {
            return CurrentDifficulty switch
            {
                DifficultyLevel.Beginner => "Новичок",
                DifficultyLevel.Professional => "Профессионал",
                DifficultyLevel.Extremal => "Экстремал",
                _ => "Неизвестно"
            };
        }

        /// <summary>
        /// Спавнит NPC на случайной позиции.
        /// </summary>
        public void Spawn(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            IsActive = true;
            gameObject.SetActive(true);

            Debug.Log($"📍 NPC спавнен на позиции: {spawnPosition}");
        }

        /// <summary>
        /// Деактивирует NPC.
        /// </summary>
        public void Despawn()
        {
            IsActive = false;
            gameObject.SetActive(false);

            Debug.Log("❌ NPC деактивирован");
        }

        #endregion

        #region Movement

        /// <summary>
        /// Движение NPC.
        /// </summary>
        private void Move()
        {
            // Простое движение вперёд (можно заменить на навигацию)
            Vector3 moveDirection = transform.forward;
            transform.position += moveDirection * CurrentSpeed * Time.deltaTime;

            // Здесь можно добавить логику столкновений и обхода препятствий
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Обработчик изменения уровня сложности.
        /// </summary>
        private void OnDifficultyChanged(DifficultyLevel level, DifficultySettings settings)
        {
            Debug.Log($"🔄 NPC: Сложность изменена на {settings.DisplayName}");

            // Применяем новые настройки
            ApplyDifficultySettings();

            // Визуальный эффект (опционально)
            // OnDifficultyChangedVisual();
        }

        #endregion
    }
}