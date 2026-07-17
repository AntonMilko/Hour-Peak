using UnityEngine;
using System;
using System.Collections.Generic;
using HourPeak.Settings;

namespace HourPeak.Characters
{
    /// <summary>
    /// Система спавна толпы NPC-прохожих.
    /// Адаптирует количество прохожих под уровень сложности:
    /// - Новичок: 20% толпы (20 NPC)
    /// - Профессионал: 50% толпы (50 NPC)
    /// - Экстремал: 80% толпы (80 NPC)
    /// </summary>
    public class CrowdSpawner : MonoBehaviour
    {
        #region Fields

        [Header("NPC Prefab")]
        [Tooltip("Префоб NPC-прохожего")]
        [SerializeField] private GameObject npcPrefab;

        [Header("Spawn Area")]
        [Tooltip("Минимальная позиция спавна по X")]
        [SerializeField] private float minX = -10f;

        [Tooltip("Максимальная позиция спавна по X")]
        [SerializeField] private float maxX = 10f;

        [Tooltip("Минимальная позиция спавна по Z")]
        [SerializeField] private float minZ = -10f;

        [Tooltip("Максимальная позиция спавна по Z")]
        [SerializeField] private float maxZ = 10f;

        [Tooltip("Высота спавна (Y)")]
        [SerializeField] private float spawnY = 0f;

        [Header("Base Settings")]
        [Tooltip("Базовое количество NPC (100000)")]
        [SerializeField] private int baseCrowdNPC = 100000;

        [Header("Crowd Area")]
        [Tooltip("Процент толпы для сложности 'Новичок' (20% = 20000 NPC)")]
        [SerializeField] private float beginnerCrowdPercentage = 0.2f;

        [Tooltip("Процент толпы для сложности 'Профессионал' (50% = 50000 NPC)")]
        [SerializeField] private float professionalCrowdPercentage = 0.5f;

        [Tooltip("Процент толпы для сложности 'Экстремал' (80% = 80000 NPC)")]
        [SerializeField] private float extremalCrowdPercentage = 0.8f;

        #endregion

        #region Properties

        /// <summary>
        /// Список всех активных NPC.
        /// </summary>
        public List<NPCPasserby> ActiveNPCs { get; private set; }

        /// <summary>
        /// Текущее количество NPC.
        /// </summary>
        public int CurrentNPCCount => ActiveNPCs.Count;

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

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ActiveNPCs = new List<NPCPasserby>();
            _difficultyManager = SettingDifficulty.Instance;

            Debug.Log("✅ CrowdSpawner инициализирован");
        }

        private void Start()
        {
            // Спавним NPC в зависимости от сложности
            SpawnCrowd();

            // Подписываемся на изменения сложности
            if (DifficultyManager != null)
            {
                DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
            }

            Debug.Log($"🎮 Толпа спавнена: {CurrentNPCCount} NPC");
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (DifficultyManager != null)
            {
                DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
            }

            // Очищаем всех NPC
            DespawnAll();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Получает процент толпы для текущей сложности.
        /// </summary>
        public float GetCrowdPercentage()
        {
            if (DifficultyManager == null) return beginnerCrowdPercentage;

            return DifficultyManager.CurrentDifficulty switch
            {
                DifficultyLevel.Beginner => beginnerCrowdPercentage,
                DifficultyLevel.Professional => professionalCrowdPercentage,
                DifficultyLevel.Extremal => extremalCrowdPercentage,
                _ => beginnerCrowdPercentage
            };
        }

        /// <summary>
        /// Получает процент толпы по уровню сложности.
        /// </summary>
        public float GetCrowdPercentageForDifficulty(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Beginner => beginnerCrowdPercentage,
                DifficultyLevel.Professional => professionalCrowdPercentage,
                DifficultyLevel.Extremal => extremalCrowdPercentage,
                _ => beginnerCrowdPercentage
            };
        }

        /// <summary>
        /// Получает количество NPC для текущей сложности.
        /// </summary>
        public int GetCrowdCount()
        {
            return Mathf.FloorToInt(baseCrowdNPC * GetCrowdPercentage());
        }

        /// <summary>
        /// Получает количество NPC для указанной сложности.
        /// </summary>
        public int GetCrowdCountForDifficulty(DifficultyLevel difficulty)
        {
            float percentage = GetCrowdPercentageForDifficulty(difficulty);
            return Mathf.FloorToInt(baseCrowdNPC * percentage);
        }

        /// <summary>
        /// Спавнит толпу NPC в зависимости от текущего уровня сложности.
        /// </summary>
        public void SpawnCrowd()
        {
            int npcCount = GetCrowdCount();
            float percentage = GetCrowdPercentage() * 100f;

            Debug.Log($"📊 Спавн толпы: {npcCount} NPC ({percentage:F0}%)");

            // Спавним NPC
            for (int i = 0; i < npcCount; i++)
            {
                SpawnNPC();
            }

            Debug.Log($"✅ Спавнено: {CurrentNPCCount} NPC");
        }

        /// <summary>
        /// Спавнит одного NPC.
        /// </summary>
        private void SpawnNPC()
        {
            if (npcPrefab == null)
            {
                Debug.LogError("❌ NPCPrefab не назначен!");
                return;
            }

            // Генерируем случайную позицию
            Vector3 spawnPosition = new Vector3(
                UnityEngine.Random.Range(minX, maxX),
                spawnY,
                UnityEngine.Random.Range(minZ, maxZ)
            );

            // Создаём NPC
            GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
            NPCPasserby npc = npcObject.GetComponent<NPCPasserby>();

            if (npc == null)
            {
                npc = npcObject.AddComponent<NPCPasserby>();
            }

            // Добавляем в список
            ActiveNPCs.Add(npc);

            // Спавним NPC
            npc.Spawn(spawnPosition);
        }

        /// <summary>
        /// Удаляет всех NPC.
        /// </summary>
        public void DespawnAll()
        {
            foreach (var npc in ActiveNPCs)
            {
                if (npc != null)
                {
                    npc.Despawn();
                    Destroy(npc.gameObject);
                }
            }

            ActiveNPCs.Clear();

            Debug.Log("🗑️ Все NPC удалены");
        }

        /// <summary>
        /// Обновляет толпу в соответствии с текущей сложностью.
        /// </summary>
        public void UpdateCrowd()
        {
            int targetCount = GetCrowdCount();
            float percentage = GetCrowdPercentage() * 100f;
            int currentCount = ActiveNPCs.Count;

            Debug.Log($"🔄 Обновление толпы: {currentCount} → {targetCount} NPC ({percentage:F0}%)");

            if (targetCount > currentCount)
            {
                // Добавляем NPC
                int toSpawn = targetCount - currentCount;
                for (int i = 0; i < toSpawn; i++)
                {
                    SpawnNPC();
                }
            }
            else if (targetCount < currentCount)
            {
                // Удаляем NPC
                int toRemove = currentCount - targetCount;
                for (int i = 0; i < toRemove; i++)
                {
                    if (ActiveNPCs.Count > 0)
                    {
                        var npc = ActiveNPCs[ActiveNPCs.Count - 1];
                        npc.Despawn();
                        Destroy(npc.gameObject);
                        ActiveNPCs.RemoveAt(ActiveNPCs.Count - 1);
                    }
                }
            }

            Debug.Log($"✅ Новое количество: {CurrentNPCCount} NPC");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Обработчик изменения уровня сложности.
        /// </summary>
        private void OnDifficultyChanged(DifficultyLevel level, DifficultySettings settings)
        {
            float percentage = GetCrowdPercentage() * 100f;
            Debug.Log($"🔄 Толпа: сложность изменена на {settings.DisplayName} ({percentage:F0}%)");

            // Обновляем количество NPC
            UpdateCrowd();
        }

        #endregion

        #region Debug

        /// <summary>
        /// Выводит текущую статистику в консоль.
        /// </summary>
        public void DebugLogStats()
        {
            float currentPercentage = GetCrowdPercentage() * 100f;
            int currentCount = GetCrowdCount();
            int beginnerCount = GetCrowdCountForDifficulty(DifficultyLevel.Beginner);
            int professionalCount = GetCrowdCountForDifficulty(DifficultyLevel.Professional);
            int extremalCount = GetCrowdCountForDifficulty(DifficultyLevel.Extremal);

            Debug.Log("═══════════════════════════════");
            Debug.Log("📊 Статистика толпы");
            Debug.Log("═══════════════════════════════");
            Debug.Log($"Текущее количество: {CurrentNPCCount} NPC");
            Debug.Log($"Текущий процент: {currentPercentage:F0}%");
            Debug.Log($"Базовое количество: {baseCrowdNPC} NPC");
            Debug.Log("───────────────────────────────");
            Debug.Log("Характеристики толпы по сложности:");
            Debug.Log($"  Новичок:      {beginnerCrowdPercentage * 100:F0}% = {beginnerCount} NPC");
            Debug.Log($"  Профессионал: {professionalCrowdPercentage * 100:F0}% = {professionalCount} NPC");
            Debug.Log($"  Экстремал:    {extremalCrowdPercentage * 100:F0}% = {extremalCount} NPC");
            Debug.Log("───────────────────────────────");
            Debug.Log($"Сложность: {(_difficultyManager?.GetCurrentDifficultyName() ?? "Неизвестно")}");
            Debug.Log("═══════════════════════════════");
        }

        /// <summary>
        /// Получает краткую информацию о толпе.
        /// </summary>
        public string GetCrowdInfo()
        {
            float percentage = GetCrowdPercentage() * 100f;
            int count = GetCrowdCount();
            return $"{count} NPC ({percentage:F0}%)";
        }

        /// <summary>
        /// Получает полную информацию о толпе для всех сложностей.
        /// </summary>
        public string GetFullCrowdInfo()
        {
            return $"Новичок: {GetCrowdCountForDifficulty(DifficultyLevel.Beginner)} | " +
                   $"Профессионал: {GetCrowdCountForDifficulty(DifficultyLevel.Professional)} | " +
                   $"Экстремал: {GetCrowdCountForDifficulty(DifficultyLevel.Extremal)}";
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Тестовый спавн толпы.
        /// </summary>
        [ContextMenu("Test Spawn Crowd")]
        private void TestSpawnCrowd()
        {
            DespawnAll();
            SpawnCrowd();
        }

        /// <summary>
        /// Тестовое обновление толпы.
        /// </summary>
        [ContextMenu("Test Update Crowd")]
        private void TestUpdateCrowd()
        {
            UpdateCrowd();
        }
#endif

        #endregion
    }
}