using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using HourPeak.Settings;

/// <summary>
/// Индикатор успеха для игрового процесса.
/// Отображает прогресс сбора индикаторов с цветовой индикацией в зависимости от времени.
/// Ползунок двигается слева направо, оставляя за собой чёрный след.
/// </summary>
public class IndicatorSuccessForGameplay : MonoBehaviour
{
    #region Constants

    private const string TIME_FORMAT = "{0:00}:{1:00}";

    #endregion

    #region Fields

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image progressFill;
    [SerializeField] private Image sliderBackground;
    [SerializeField] private Image sliderFill;
    [SerializeField] private RectTransform sliderHandle;

    [Header("Colors")]
    [Tooltip("Превосходно (шустро) - зелёный")]
    [SerializeField] private Color perfectColor = new Color(0f, 0.8f, 0f);
    
    [Tooltip("Супер (быстро) - синий")]
    [SerializeField] private Color superColor = new Color(0f, 0.5f, 1f);
    
    [Tooltip("Отлично (хорошо) - жёлтый")]
    [SerializeField] private Color excellentColor = new Color(1f, 0.8f, 0f);
    
    [Tooltip("Вовремя (нормально) - красный")]
    [SerializeField] private Color onTimeColor = new Color(1f, 0f, 0f);
    
    [Tooltip("Провал (опоздал) - чёрный")]
    [SerializeField] private Color failColor = Color.black;
    
    [Tooltip("След ползунка (чёрный)")]
    [SerializeField] private Color trailColor = Color.black;

    [Header("Settings")]
    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
    [SerializeField] private int targetIndicators = 5;
    [SerializeField] private bool autoStart = true;

    [Header("Prefab References")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorParent;

    #endregion

    #region Private State

    private int collectedIndicators;
    private float currentLevelTime;
    private float levelTimeLimit;
    private bool isLevelActive;
    private List<IndicatorData> activeIndicators = new List<IndicatorData>();

    #endregion

    #region Properties

    public int CollectedIndicators => collectedIndicators;
    public float RemainingTime => levelTimeLimit - currentLevelTime;
    public bool IsLevelActive => isLevelActive;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (indicatorParent == null)
        {
            indicatorParent = transform;
        }
    }

    private void Start()
    {
        if (autoStart)
        {
            StartLevel();
        }
    }

    private void Update()
    {
        if (!isLevelActive)
            return;

        UpdateLevel();
    }

    #endregion

    #region Public Methods

    public void StartLevel()
    {
        LoadDifficultySettings();
        
        collectedIndicators = 0;
        currentLevelTime = 0f;
        isLevelActive = true;
        
        ClearIndicators();
        SpawnIndicators();
        UpdateUI();
        
        Debug.Log($"🎮 Уровень запущен: {GetDifficultyName()} | Цель: {targetIndicators} индикаторов");
    }

    public void CollectIndicator(GameObject indicator)
    {
        if (!isLevelActive || indicator == null)
            return;

        IndicatorData indicatorData = activeIndicators.Find(data => data.indicator == indicator);
        if (indicatorData == null)
            return;

        collectedIndicators++;
        activeIndicators.Remove(indicatorData);
        Destroy(indicator);
        UpdateUI();
        
        Debug.Log($"✅ Собран индикатор! {collectedIndicators}/{targetIndicators}");
        
        if (collectedIndicators >= targetIndicators)
        {
            LevelCompleted();
        }
    }

    public void SetDifficulty(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        StartLevel();
    }

    public void ResetLevel()
    {
        StartLevel();
    }

    #endregion

    #region Level Logic

    private void LoadDifficultySettings()
    {
        SettingDifficulty difficultyManager = SettingDifficulty.Instance;
        
        if (difficultyManager != null)
        {
            DifficultySettings settings = difficultyManager.GetSettingsByLevel(currentDifficulty);
            levelTimeLimit = settings.TimeLimit;
            Debug.Log($"📋 Настройки сложности: {GetDifficultyName()} | Время: {FormatTime(levelTimeLimit)}");
        }
        else
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Beginner:
                    levelTimeLimit = 240f;
                    break;
                case DifficultyLevel.Professional:
                    levelTimeLimit = 120f;
                    break;
                case DifficultyLevel.Extremal:
                    levelTimeLimit = 60f;
                    break;
                default:
                    levelTimeLimit = 240f;
                    break;
            }
            
            Debug.LogWarning($"⚠️ SettingDifficulty не найден! Используем: {levelTimeLimit}с");
        }
    }

    private void UpdateLevel()
    {
        currentLevelTime += Time.deltaTime;
        UpdateIndicators();
        UpdateUI();
        
        if (currentLevelTime >= levelTimeLimit)
        {
            TimeUp();
        }
    }

    private void TimeUp()
    {
        isLevelActive = false;
        Debug.Log("⏰ Время вышло! Провал.");
        
        if (sliderFill != null)
            sliderFill.color = failColor;
        
        if (timerText != null)
            timerText.color = failColor;
    }

    private void LevelCompleted()
    {
        isLevelActive = false;
        
        float timeRatio = currentLevelTime / levelTimeLimit;
        Color resultColor = GetColorForTimeRatio(timeRatio);
        
        if (sliderFill != null)
            sliderFill.color = resultColor;
        
        Debug.Log($"🎉 Уровень пройден! Время: {FormatTime(currentLevelTime)} | Цвет: {resultColor}");
    }

    #endregion

    #region Indicators

    private void ClearIndicators()
    {
        foreach (var indicatorData in activeIndicators)
        {
            if (indicatorData.indicator != null)
            {
                Destroy(indicatorData.indicator);
            }
        }
        activeIndicators.Clear();
    }

    private void SpawnIndicators()
    {
        if (indicatorPrefab == null)
        {
            Debug.LogWarning("⚠️ indicatorPrefab не назначен!");
            return;
        }

        int indicatorsToSpawn = Mathf.Max(2, targetIndicators / 3);
        
        for (int i = 0; i < indicatorsToSpawn; i++)
        {
            CreateIndicator();
        }
    }

    private void CreateIndicator()
    {
        if (indicatorPrefab == null)
            return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject newIndicator = Instantiate(indicatorPrefab, spawnPosition, Quaternion.identity);
        newIndicator.transform.SetParent(indicatorParent);

        IndicatorData indicatorData = new IndicatorData
        {
            indicator = newIndicator,
            createdTime = Time.time,
            lifetime = Random.Range(5f, 10f)
        };

        activeIndicators.Add(indicatorData);
    }

    private void UpdateIndicators()
    {
        foreach (var indicatorData in activeIndicators)
        {
            UpdateIndicatorColor(indicatorData);
        }
    }

    private void UpdateIndicatorColor(IndicatorData indicatorData)
    {
        if (indicatorData.indicator == null)
            return;

        float timeRatio = currentLevelTime / levelTimeLimit;
        Color color = GetColorForTimeRatio(timeRatio);
        
        Renderer renderer = indicatorData.indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(-25f, 25f),
            0.5f,
            Random.Range(-25f, 25f)
        );
    }

    #endregion

    #region UI Update

    private void UpdateUI()
    {
        if (timerText != null)
        {
            float remainingTime = levelTimeLimit - currentLevelTime;
            timerText.text = FormatTime(remainingTime);
            timerText.color = GetColorForRemainingTime(remainingTime);
        }

        if (progressFill != null)
        {
            float progress = (float)collectedIndicators / targetIndicators;
            progressFill.rectTransform.anchorMax = new Vector2(progress, 1f);
            progressFill.rectTransform.anchorMin = new Vector2(0f, 0f);
        }

        if (sliderBackground != null && sliderFill != null)
        {
            float progress = (float)collectedIndicators / targetIndicators;
            sliderFill.rectTransform.anchorMax = new Vector2(progress, 1f);
            sliderFill.rectTransform.anchorMin = new Vector2(0f, 0f);
            sliderBackground.color = trailColor;
        }

        if (sliderFill != null)
        {
            float timeRatio = currentLevelTime / levelTimeLimit;
            sliderFill.color = GetColorForTimeRatio(timeRatio);
        }

        if (sliderHandle != null)
        {
            float progress = (float)collectedIndicators / targetIndicators;
            sliderHandle.anchorMax = new Vector2(progress, 1f);
            sliderHandle.anchorMin = new Vector2(progress, 0f);
        }
    }

    private Color GetColorForTimeRatio(float timeRatio)
    {
        if (timeRatio < 0.25f)
        {
            return perfectColor;
        }
        else if (timeRatio < 0.5f)
        {
            return superColor;
        }
        else if (timeRatio < 0.75f)
        {
            return excellentColor;
        }
        else if (timeRatio < 1f)
        {
            return onTimeColor;
        }
        else
        {
            return failColor;
        }
    }

    private Color GetColorForRemainingTime(float remainingTime)
    {
        float timeRatio = remainingTime / levelTimeLimit;
        
        if (timeRatio > 0.75f)
        {
            return perfectColor;
        }
        else if (timeRatio > 0.5f)
        {
            return superColor;
        }
        else if (timeRatio > 0.25f)
        {
            return excellentColor;
        }
        else if (timeRatio > 0)
        {
            return onTimeColor;
        }
        else
        {
            return failColor;
        }
    }

    #endregion

    #region Utilities

    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format(TIME_FORMAT, minutes, seconds);
    }

    private string GetDifficultyName()
    {
        return currentDifficulty switch
        {
            DifficultyLevel.Beginner => "Новичок",
            DifficultyLevel.Professional => "Профессионал",
            DifficultyLevel.Extremal => "Экстремал",
            _ => "Неизвестно"
        };
    }

    #endregion

    #region Nested Classes

    private sealed class IndicatorData
    {
        public GameObject indicator;
        public float createdTime;
        public float lifetime;
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;

        GUILayout.BeginArea(new Rect(10, 250, 300, 200));
        GUILayout.BeginVertical("box");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label("🎯 IndicatorSuccess Debug");
        GUILayout.Label("═══════════════════════════════");
        GUILayout.Label($"Собрано: {collectedIndicators}/{targetIndicators}");
        GUILayout.Label($"Время: {FormatTime(currentLevelTime)}/{FormatTime(levelTimeLimit)}");
        GUILayout.Label($"Доля: {(levelTimeLimit > 0 ? (currentLevelTime / levelTimeLimit * 100f) : 0f):F1}%");
        GUILayout.Label($"Сложность: {GetDifficultyName()}");
        GUILayout.Label($"Активен: {isLevelActive}");
        GUILayout.Label("═══════════════════════════════");
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Старт")) StartLevel();
        if (GUILayout.Button("Сбор")) collectedIndicators++;
        if (GUILayout.Button("Сброс")) ResetLevel();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
#endif
    }

    #endregion
}