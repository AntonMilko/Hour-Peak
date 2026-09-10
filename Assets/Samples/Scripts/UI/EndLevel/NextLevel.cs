using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    /// <summary>
    /// Управление переходом на следующий уровень.
    /// Кнопка "Далее":
    /// - Скрыта при старте уровня
    /// - Скрыта при истечении времени (неудача)
    /// - Показана только после успешного достижения пункта назначения
    /// Автосохранение при переходе между частями уровня и уровнями.
    /// </summary>
    public class NextLevel : MonoBehaviour
    {
    #region Constants

    private const string SAVE_FOLDER_NAME = "GameSaves";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const string GLOBAL_SAVE_FILENAME = "global_save.json";

    #endregion

    #region Fields

    [Header("Level Settings")]
    [Tooltip("Текущий уровень (номер)")]
    [SerializeField] private int currentLevelNumber = 1;
    
    [Tooltip("Текущая часть уровня (1, 2)")]
    [SerializeField] private int currentPartIndex = 1;
    
    [Tooltip("Название сцены следующего уровня")]
    [SerializeField] private string nextLevelSceneName;

    [Header("UI References")]
    [Tooltip("Кнопка 'Далее'")]
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

    #region Properties

    public float RemainingTime => remainingTime;
    public int StarsCollected => starsCollected;
    public bool IsLevelActive => isLevelActive;
    public bool HasReachedDestination => reachedDestination;

    public Button NextLevelButtonFailed { get => nextLevelButtonFailed; set => nextLevelButtonFailed = value; }

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
        HideAllUI();  // Кнопка "Далее" скрыта при старте
        
        Debug.Log($"🎮 Уровень запущен: Уровень {currentLevelNumber} Часть {currentPartIndex} | Время: {FormatTime(levelTimeLimit)}");
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

    public void OnNextLevelButtonClick()
    {
        if (!levelSuccess)
        {
            Debug.LogWarning("⚠️ Нельзя перейти на следующий уровень без успешного завершения!");
            return;
        }
        
        SaveProgress();
        LoadNextLevel();
    }

    public void OnNextPart()
    {
        if (!levelSuccess)
        {
            Debug.LogWarning("⚠️ Нельзя перейти на следующую часть без успешного завершения!");
            return;
        }
        
        SaveProgress();
        LoadNextPart();
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
        
        if (continueManager != null)
        {
            currentDifficulty = continueManager.CurrentDifficulty;
        }
        
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
            
            Debug.LogWarning($"⚠️ SettingDifficulty не найден! Используем: {levelTimeLimit}с");
        }
    }

    private void LoadNextPart()
    {
        // Проверяем, последний ли это уровень и часть
        bool isLastLevel = currentLevelNumber >= 36;
        bool isPart2 = currentPartIndex >= 2;
        
        if (isLastLevel && isPart2)
        {
            // Финальная часть финального уровня — переход к сертификату
            Debug.Log("🏆 Финальная часть пройдена! Переход к сертификату...");
            
            // Автосохранение перед переходом
            SaveProgress();
            
            // Переход к сцене сертификата
            if (!string.IsNullOrEmpty(nextLevelSceneName))
            {
                SceneManager.LoadScene(nextLevelSceneName);
            }
            else
            {
                // Если nextLevelSceneName не задана — используем стандартное имя
                SceneManager.LoadScene("Game completion certificate");
            }
            return;
        }
        
        currentPartIndex++;
        
        Debug.Log($"🔄 Переход на часть: Уровень {currentLevelNumber} Часть {currentPartIndex}");
        
        CreateSaveFilePath();
        StartLevel();
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
            EndLevelFailed();  // Кнопка "Далее" будет скрыта
        }
    }

    private void EndLevelFailed()
    {
        isLevelActive = false;
        levelSuccess = false;
        reachedDestination = false;
        
        ShowFailureUI();
        Debug.Log($"⏰ Время вышло! Уровень не пройден. Кнопка 'Далее' скрыта.");
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
        
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }
    }

    private void ShowFailureUI()
    {
        HideAllUI();
        
        SetUIActive(failurePanel, true);
        SetNextLevelButtonVisible(false);  // Кнопка "Далее" скрыта при неудаче (время вышло)
    }

    /// <summary>
    /// Управляет видимостью кнопки "Далее".
    /// Кнопка отображается ТОЛЬКО при успешном достижении пункта назначения.
    /// При истечении времени кнопка полностью скрывается.
    /// </summary>
    private void SetNextLevelButtonVisible(bool visible)
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(visible);
            nextLevelButton.interactable = visible;
        }
    }

    private void SetUIActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    private void SetUIActive(Button button, bool active)
    {
        if (button != null)
            button.gameObject.SetActive(active);
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
        
        currentSaveFilePath = Path.Combine(folderPath, $"level_{currentLevelNumber:000}{SAVE_FILE_EXTENSION}");
    }

    public void SaveProgress()
    {
        GameData data = new GameData
        {
            lastUnlockedLevel = levelSuccess ? Mathf.Max(currentLevelNumber, GetLastUnlockedLevel() + (levelSuccess ? 1 : 0)) : GetLastUnlockedLevel(),
            currentLevel = currentLevelNumber,
            currentPart = currentPartIndex,
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
                
                Debug.Log($"💾 Загружено сохранение: Уровень {data.currentLevel} Часть {data.currentPart} | Звёзд: {data.starsCollected} | Время: {FormatTime(data.remainingTime)}");
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

    #region Level Transition

    private void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelNumber + 1;
        
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            Debug.Log($"🔄 Переход на уровень: {nextLevelSceneName}");
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.Log($"🔄 Переход на уровень: Level{nextLevelIndex}");
            SceneManager.LoadScene($"Level{nextLevelIndex}");
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
        public int currentPart;
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
}