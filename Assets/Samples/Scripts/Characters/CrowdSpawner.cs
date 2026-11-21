using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HourPeak.Settings;

namespace HourPeak.Characters
{
    public class CrowdSpawner : MonoBehaviour
    {
        #region Configuration

        [Header("NPC Prefab")]
        [SerializeField] private NoPlayerController npcPrefab;

        [Header("Performance")]
        [SerializeField] private int spawnBatchSize = 100;

        [Header("General Settings")]
        [SerializeField] private List <Transform> targetPositions;

        #endregion

        #region Fields

        public static List <Transform> _targetPositions;
        private List<NoPlayerController> activeNPCCs = new List<NoPlayerController>();
        private int targetCount = 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _targetPositions = targetPositions;
            var allSpawners = Object.FindObjectsByType<CrowdSpawner>(FindObjectsSortMode.None);
            
            if (allSpawners.Length > 1)
            {
                Debug.LogError($"[CrowdSpawner] На сцене найдено {allSpawners.Length} экземпляров! Это вызовет дублирование NPC.");
            }
        }

        private void Start()
        {
            targetCount = CalculateTargetCount();
            Debug.Log($"[CrowdSpawner] Начало спавна толпы. Цель: {targetCount} NPC.");

            StartCoroutine(SpawnCrowdOverTime());
        }

        #endregion

        #region Crowd Methods

        private int CalculateTargetCount()
        {
            var difficulty = SettingDifficulty.Instance;
            
            if (difficulty == null)
            {
                Debug.LogWarning("[CrowdSpawner] SettingDifficulty не найден, используем значение по умолчанию (20).");
                return 20;
            }

            int baseCount = 100;
            float percentage = 0.2f;

            switch (SettingDifficulty.CurrentDifficulty) 
            {
                case DifficultyLevel.Beginner: percentage = 0.2f; break;
                case DifficultyLevel.Professional: percentage = 0.5f; break;
                case DifficultyLevel.Extremal: percentage = 0.8f; break;
            }

            int mode = difficulty.GetDifficultyMode();
            
            if (mode == 0) percentage = 0.2f;
            else if (mode == 1) percentage = 0.5f;
            else if (mode == 2) percentage = 0.8f;

            return Mathf.RoundToInt(baseCount * percentage);
        }

        private IEnumerator SpawnCrowdOverTime()
        {
            int spawned = 0;

            while (spawned < targetCount)
            {
                int toSpawnNow = Mathf.Min(spawnBatchSize, targetCount - spawned);

                for (int i = 0; i < toSpawnNow; i++)
                {
                    SpawnSingleNPC();
                    spawned++;
                }

                yield return null; 
            }

            Debug.Log($"[CrowdSpawner] ✅ Толпа полностью заспавнена: {activeNPCCs.Count} NPC.");
        }

        private void SpawnSingleNPC()
        {
            Vector3 spawnPos = _targetPositions[Random.Range(0, _targetPositions.Count)].position;
            NoPlayerController npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            activeNPCCs.Add(npc);
        }

        #endregion
    }
}