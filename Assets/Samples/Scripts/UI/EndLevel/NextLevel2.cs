using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;
using HourPeak.Samples.Runtime;

/// <summary>
/// Финальный уровень с переходом в "Game completion certificate".
/// Основан на NextLevel, но с логикой завершения игры.
/// </summary>
public class NextLevel2 : MonoBehaviour
{
    #region Constants

    private const string SAVE_FOLDER_NAME = "GameSaves";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const string GLOBAL_SAVE_FILENAME = "global_save.json";

    #endregion

    #region Fields

    [Header("Level Settings")]
    [Tooltip("Название сцены 'Game Completion Certificate'")]
    [SerializeField] private string certificateSceneName = "Game completion certificate";

    [Header("UI References")]
    [Tooltip("Кнопка 'Далее' (успех)")]
    [SerializeField] private Button nextLevelButton;
    
    [Tooltip("Кнопка 'Далее' (при неудаче/время вышло)")]
    [SerializeField] private Button nextLevelButtonFailed;
    
    [Tooltip("Панель успешного завершения уровня")]
    [SerializeField] private GameObject successPanel;
    
    [Tooltip("Панель неудачи (время вышло)")]
    [SerializeField] private GameObject failurePanel;

    [Header("Settings")]
    [Tooltip("Автосохранение при переходе")]
    [SerializeField] private bool autoSaveOnTransition = true;
    
    [Tooltip("Требовать сбор звёзд для успеха")]
    [SerializeField] private bool requireStarsForSuccess = false;
    
    [Tooltip("Требуемое количество звёзд")]
    [SerializeField] private int requiredStars = 0;

    [Header("Continue Manager")]
    [Tooltip("Менеджер настроек сложности")]
    [SerializeField] private Continue continueManager;

    #endregion

    #region Private State

    private float remainingTime;
    private float levelTimeLimit;
    private int starsCollected;
    private bool isLevelActive;
    private bool levelSuccess;
    private bool reachedDestination;
    private string currentSaveFilePath;
    private SettingDifficulty difficultyManager;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        difficultyManager = SettingDifficulty.Instance;
        LoadDifficultySettings();
        
        InitializeLevelState();
        HideAllUI();
    }

    private void Start()
    {
        CreateSaveFilePath();
        LoadPreviousProgress();
    }

    private void Update()
    {
        if (!isLevelActive)
            return;

        UpdateTimer();
    }

    #endregion

    #region Public Methods

    public void StartLevel()
    {
        LoadDifficultySettings();
        InitializeLevelState();
        HideAllUI(); // Кнопка "Далее" скрыта при старте
        
        Debug.Log($"🎮 Уровень запущен: Уровень 1 | Время: {FormatTime(levelTimeLimit)}");
    }

    public void OnStarCollected()
    {
        if (!isLevelActive)
            return;
        
        starsCollected++;
        Debug.Log($"⭐ Звезда собрана! Всего: {starsCollected}/{requiredStars}");
    }

    public void OnReachDestination()
    {
        if (!isLevelActive || reachedDestination)
            return;
        
        reachedDestination = true;
        isLevelActive = false;
        
        if (requireStarsForSuccess)
        {
            levelSuccess = starsCollected >= requiredStars;
        }
        else
        {
            levelSuccess = true;
        }
        
        if (levelSuccess)
        {
            ShowSuccessUI();
            Debug.Log($"🏁 Пункт назначения достигнут! Звёзд: {starsCollected}");
        }
        else
        {
            ShowFailureUI();
            Debug.Log($"❌ Недостаточно звёзд! Собрано: {starsCollected}/{requiredStars}");
        }
        
        if (autoSaveOnTransition)
        {
            SaveProgress();
        }
    }

    /// <summary>
    /// Переход к сертификату и сохранение.
    /// </summary>
    public void OnNextLevelButtonClick()
    {
        if (!levelSuccess)
        {
            Debug.LogWarning("⚠️ Нельзя перейти к сертификату без успешного завершения!");
            return;
        }
        
        // Сохраняем настройки сложности
        if (continueManager != null)
        {
            continueManager.SaveCurrentSettings();
            Debug.Log("💾 Настройки сложности сохранены перед переходом.");
        }
        
        SaveProgress();
        LoadCertificateScene();
    }

    public void ResetLevel()
    {
        StartLevel();
    }

    #endregion

    #region Timer Logic

    private void LoadDifficultySettings()
    {
        DifficultyLevel currentDifficulty = DifficultyLevel.Beginner;
        
        if (difficultyManager != null)
        {
            DifficultySettings settings = difficultyManager.GetSettingsByLevel(currentDifficulty);
            levelTimeLimit = settings.TimeLimit;
        }
        else
        {
            levelTimeLimit = currentDifficulty switch
            {
                DifficultyLevel.Beginner => 240f,
                DifficultyLevel.Professional => 120f,
                DifficultyLevel.Extremal => 60f,
                _ => 240f
            };
        }
    }

    private void InitializeLevelState()
    {
        remainingTime = levelTimeLimit;
        isLevelActive = true;
        levelSuccess = false;
        reachedDestination = false;
        starsCollected = 0;
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0f;
            EndLevelFailed();
        }
    }

    private void EndLevelFailed()
    {
        isLevelActive = false;
        levelSuccess = false;
        reachedDestination = false;
        
        ShowFailureUI();
        Debug.Log($"⏰ Время вышло! Уровень не пройден.");
    }

    #endregion

    #region UI Management

    private void HideAllUI()
    {
        SetUIActive(successPanel, false);
        SetUIActive(failurePanel, false);
        SetNextLevelButtonVisible(false);
    }

    private void ShowSuccessUI()
    {
        HideAllUI();
        
        SetUIActive(successPanel, true);
        SetNextLevelButtonVisible(true);
        SetNextLevelButtonFailedVisible(false);
        
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }
    }

    private void ShowFailureUI()
    {
        HideAllUI();
        
        SetUIActive(failurePanel, true);
        SetNextLevelButtonVisible(false); 
        SetNextLevelButtonFailedVisible(true); 
    }

    private void SetNextLevelButtonVisible(bool visible)
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(visible);
            nextLevelButton.interactable = visible;
        }
    }

    private void SetNextLevelButtonFailedVisible(bool visible)
    {
        if (nextLevelButtonFailed != null)
        {
            nextLevelButtonFailed.gameObject.SetActive(visible);
            nextLevelButtonFailed.interactable = visible;
        }
    }

    private void SetUIActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    #endregion

    #region Save/Load Logic

    private void CreateSaveFilePath()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        currentSaveFilePath = Path.Combine(folderPath, $"level_001{SAVE_FILE_EXTENSION}");
    }

    public void SaveProgress()
    {
        GameData data = new GameData
        {
            lastUnlockedLevel = levelSuccess ? Mathf.Max(1, GetLastUnlockedLevel() + 1) : GetLastUnlockedLevel(),
            currentLevel = 1,
            starsCollected = starsCollected,
            remainingTime = remainingTime,
            saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(currentSaveFilePath, json);
            
            SaveGlobalProgress(data.lastUnlockedLevel);
            
            Debug.Log($"💾 Сохранение: {currentSaveFilePath} | Уровень разблокирован: {data.lastUnlockedLevel}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
        }
    }

    private void LoadPreviousProgress()
    {
        if (File.Exists(currentSaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(currentSaveFilePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                
                Debug.Log($"💾 Загружено сохранение: Уровень {data.currentLevel}");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка загрузки: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"ℹ️ Новая игра: {currentSaveFilePath}");
        }
    }

    private void SaveGlobalProgress(int lastUnlockedLevel)
    {
        string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
        
        GlobalGameData data = new GlobalGameData
        {
            lastUnlockedLevel = lastUnlockedLevel,
            saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(globalSavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка сохранения глобального прогресса: {e.Message}");
        }
    }

    private int GetLastUnlockedLevel()
    {
        string globalSavePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME, GLOBAL_SAVE_FILENAME);
        
        if (File.Exists(globalSavePath))
        {
            try
            {
                string json = File.ReadAllText(globalSavePath);
                GlobalGameData data = JsonUtility.FromJson<GlobalGameData>(json);
                return data.lastUnlockedLevel;
            }
            catch
            {
                return 1;
            }
        }
        
        return 1;
    }

    #endregion

    #region Scene Transition

    /// <summary>
    /// Загрузка сцены сертификата.
    /// </summary>
    private void LoadCertificateScene()
    {
        if (!string.IsNullOrEmpty(certificateSceneName))
        {
            Debug.Log($"🏆 Переход в сцену: {certificateSceneName}");
            SceneManager.LoadScene(certificateSceneName);
        }
        else
        {
            Debug.LogError("❌ Сцена сертификата не указана!");
        }
    }

    #endregion

    #region Utilities

    private static string FormatTime(float time)
    {
        if (time < 0f)
            time = 0f;
        
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #endregion

    #region Data Structures

    [Serializable]
    public class GameData
    {
        public int lastUnlockedLevel;
        public int currentLevel;
        public int starsCollected;
        public float remainingTime;
        public string saveTimestamp;
    }

    [Serializable]
    public class GlobalGameData
    {
        public int lastUnlockedLevel;
        public string saveTimestamp;
    }

    #endregion
}