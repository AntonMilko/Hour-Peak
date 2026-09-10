using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <-- ЭТОГО НЕ ХВАТАЛО (для List<>)
using HourPeak.Settings;

namespace HourPeak.Characters
{
    public class CrowdSpawner : MonoBehaviour
    {
        [Header("NPC Prefab")]
        [SerializeField] private GameObject npcPrefab;

        [Header("Spawn Area")]
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float spawnY = 0f;
        [SerializeField] private float maxZ = 20f;
        [SerializeField] private float minZ = -20f;

        [Header("Performance")]
        [SerializeField] private int spawnBatchSize = 100; // Спавним по 100 NPC за кадр

        private List<NPCPasserby> activeNPCCs = new List<NPCPasserby>();
        private int targetCount = 0;

        private void Awake()
        {
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
            
            // ПРИМЕР (раскомментируйте и измените под себя):
            /*
            switch (difficulty.CurrentDifficulty) 
            {
                case DifficultyLevel.Beginner: percentage = 0.2f; break;
                case DifficultyLevel.Professional: percentage = 0.5f; break;
                case DifficultyLevel.Extremal: percentage = 0.8f; break;
            }
            */

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
            if (npcPrefab == null)
            {
                Debug.LogError("[CrowdSpawner] NPC Prefab не назначен в инспекторе!");
                return;
            }

            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 spawnPos = new Vector3(randomX, spawnY, randomZ);

            GameObject npcObj = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            NPCPasserby npcScript = npcObj.GetComponent<NPCPasserby>();
            
            if (npcScript != null)
            {
                activeNPCCs.Add(npcScript);
                // npcScript.Spawn(spawnPos); 
            }
            else
            {
                Debug.LogWarning("[CrowdSpawner] На префабе NPC нет скрипта NPCPasserby!");
            }
        }

        private void OnDestroy()
        {
            foreach (var npc in activeNPCCs)
            {
                if (npc != null)
                {
                    Destroy(npc.gameObject);
                }
            }
            activeNPCCs.Clear();
        }
    }
}