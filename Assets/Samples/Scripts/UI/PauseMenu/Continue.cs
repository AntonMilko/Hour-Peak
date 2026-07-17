using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    public class Continue : MonoBehaviour
    {
    [Header("⏱️ Настройки времени")]
    [SerializeField] private bool restoreGameTime = true;
    [SerializeField] private bool restorePhysicalTime = true;
    [SerializeField] private float timeScaleMultiplier = 1.0f;
    [SerializeField] private float physicsSpeedMultiplier = 1.0f;
    [SerializeField] private bool useCustomTimer = true;
    [SerializeField] private float customTimerValue = 0.0f;

    [Header("👥 Настройки сложности")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
    [SerializeField] private bool isDifficultySet = false;

    [Header("📊 Отображение статистики")]
    [SerializeField] private int maxCrowdCapacity = 50;
    [SerializeField] private TextMeshProUGUI statsText;

    public DifficultyLevel CurrentDifficulty => currentDifficulty;
    public bool IsDifficultySet => isDifficultySet;

    private const string KEY_DIFFICULTY = "DiffLevel";
    private const string KEY_IS_SET = "DiffIsSet";
    private const string KEY_TIME = "TimeScale";
    private const string KEY_PHYSICS = "PhysSpeed";

    private float savedGameTime;
    private bool isPaused;

    private void Awake()
    {
        LoadSettings();
    }

    public void ApplyDifficultySettings()
    {
        // 🔥 Ключевое изменение: таймер включается только если сложность настроена
        useCustomTimer = isDifficultySet;

        float crowdPercent = 0.2f; 

        if (!isDifficultySet)
        {
            timeScaleMultiplier = 1.0f;
            physicsSpeedMultiplier = 1.0f;
        }
        else
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Beginner:
                    timeScaleMultiplier = 1.0f; 
                    physicsSpeedMultiplier = 1.0f;
                    crowdPercent = 0.2f;
                    break;
                case DifficultyLevel.Professional:
                    timeScaleMultiplier = 1.2f; 
                    physicsSpeedMultiplier = 1.1f;
                    crowdPercent = 0.5f; 
                    break;
                case DifficultyLevel.Extremal:
                    timeScaleMultiplier = 1.5f; 
                    physicsSpeedMultiplier = 1.2f;
                    crowdPercent = 0.8f; 
                    break;
            }
        }
        
        if (restoreGameTime)
        {
            Time.timeScale = timeScaleMultiplier;
            
            // Кастомный таймер сработает только когда useCustomTimer == true
            if (useCustomTimer)
            {
                customTimerValue = savedGameTime;
            }
        }

        if (restorePhysicalTime)
        {
            Time.fixedDeltaTime = 0.02f * physicsSpeedMultiplier;
        }

        int targetCrowd = Mathf.RoundToInt(maxCrowdCapacity * crowdPercent);
        // CrowdSpawner.Instance.SetTargetCount(targetCrowd);
        Debug.Log($"👥 Установлено кол-во NPC: {targetCrowd}");

        if (statsText != null)
        {
            statsText.text = $"Время: x{timeScaleMultiplier:F1} | Толпа: {(crowdPercent * 100):F0}%";
            statsText.gameObject.SetActive(true);
        }

        if (useCustomTimer)
        {
            customTimerValue = savedGameTime;
            
            // ✅ ПРИМЕНИТЬ значение к таймеру
            // GameManager.Instance.SetGameTime(customTimerValue);
            Debug.Log($"🕒 Кастомное время: {customTimerValue}");
        }
    }

    public void SaveCurrentSettings()
    {
        PlayerPrefs.SetInt(KEY_DIFFICULTY, (int)currentDifficulty);
        PlayerPrefs.SetInt(KEY_IS_SET, isDifficultySet ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_TIME, timeScaleMultiplier);
        PlayerPrefs.SetFloat(KEY_PHYSICS, physicsSpeedMultiplier);
        PlayerPrefs.Save();
        
        Debug.Log("💾 Настройки сохранены перед перезагрузкой.");
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(KEY_DIFFICULTY))
        {
            currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt(KEY_DIFFICULTY);
            isDifficultySet = PlayerPrefs.GetInt(KEY_IS_SET) == 1;
            
            // 🔥 Автоматически синхронизируем useCustomTimer с состоянием сложности
            useCustomTimer = isDifficultySet;
            
            if (isDifficultySet)
            {
                timeScaleMultiplier = PlayerPrefs.GetFloat(KEY_TIME, 1.0f);
                physicsSpeedMultiplier = PlayerPrefs.GetFloat(KEY_PHYSICS, 1.0f);
            }
        }
        
        ApplyDifficultySettings();
    }
    }
}