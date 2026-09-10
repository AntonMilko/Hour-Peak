using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;
using HourPeak.Samples.Runtime;

namespace HourPeak.Samples.Runtime
{
    /// <summary>
    /// Управление перезапуском уровня для финального уровня.
    /// При успешном завершении последнего уровня кнопка "Заново" скрывается.
    /// При неудачном завершении последнего уровня кнопка "Заново" отображается.
    /// </summary>
    public class AgainLevel2 : MonoBehaviour
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
    
    [Tooltip("Название сцены текущего уровня")]
    [SerializeField] private string currentLevelSceneName;

    [Header("UI References")]
    [Tooltip("Кнопка 'Заново' (скрыта при успехе последнего уровня)")]
    [SerializeField] private Button againLevelButton;
    
    [Tooltip("Кнопка 'Заново' (отображается при неудаче/время вышло)")]
    [SerializeField] private Button againLevelButtonSuccess;
    
    [Tooltip("Панель успешного завершения уровня")]
    [SerializeField] private GameObject successPanel;
    
    [Tooltip("Панель неудачи (время вышло)")]
    [SerializeField] private GameObject failurePanel;

    [Header("Settings")]
    [Tooltip("Автосохранение при перезапуске")]
    [SerializeField] private bool autoSaveOnRetry = true;

    [Header("Continue Manager")]
    [Tooltip("Менеджер настроек сложности")]
    [SerializeField] private Continue continueManager;

    #endregion

    #region Private State

    private int retryCount;
    private string currentSaveFilePath;
    private SettingDifficulty difficultyManager;

    #endregion

    #region Properties

    public int RetryCount => retryCount;
    public bool IsLevelActive => SceneManager.GetActiveScene().name == currentLevelSceneName;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        difficultyManager = SettingDifficulty.Instance;
        CreateSaveFilePath();
        
        retryCount = 0;
    }

    private void Start()
    {
        LoadPreviousProgress();
        SetupButtons();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Обработчик клика по кнопке "Заново".
    /// </summary>
    public void OnAgainButtonClick()
    {
        retryCount++;
        
        Debug.Log($"🔄 Перезапуск уровня: Уровень {currentLevelNumber} Часть {currentPartIndex} | Попытка: {retryCount}");
        
        // Автосохранение перед перезапуском (особенно важно для финального уровня)
        if (autoSaveOnRetry)
        {
            SaveRetryProgress();
            Debug.Log("💾 Автосохранение выполнено перед перезапуском.");
        }
        
        // Для финального уровня 36 Part 2 — рестарт с 36.1
        if (currentLevelNumber == 36 && currentPartIndex >= 2)
        {
            Debug.Log("🔄 Финальный уровень — рестарт с Part 1 (36.1)");
            currentPartIndex = 1;
        }
        
        LoadCurrentLevel();
    }

    /// <summary>
    /// Загружает текущий уровень.
    /// </summary>
    public void LoadCurrentLevel()
    {
        if (!string.IsNullOrEmpty(currentLevelSceneName))
        {
            Debug.Log($"🔄 Перезагрузка сцены: {currentLevelSceneName}");
            SceneManager.LoadScene(currentLevelSceneName);
        }
        else
        {
            Debug.Log($"🔄 Перезагрузка уровня: Level{currentLevelNumber}");
            SceneManager.LoadScene($"Level{currentLevelNumber}");
        }
    }

    /// <summary>
    /// Получает название сцены текущего уровня.
    /// </summary>
    public string GetCurrentLevelSceneName()
    {
        return currentLevelSceneName;
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

    /// <summary>
    /// Сохраняет прогресс перезапуска.
    /// </summary>
    public void SaveRetryProgress()
    {
        GameData data = new GameData
        {
            lastUnlockedLevel = GetLastUnlockedLevel(),
            currentLevel = currentLevelNumber,
            currentPart = currentPartIndex,
            retryCount = retryCount,
            saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(currentSaveFilePath, json);
            
            SaveGlobalProgress(data.lastUnlockedLevel);
            
            Debug.Log($"💾 Сохранение перезапуска: {currentSaveFilePath} | Попыток: {retryCount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
        }
    }

    /// <summary>
    /// Загружает предыдущий прогресс.
    /// </summary>
    private void LoadPreviousProgress()
    {
        if (File.Exists(currentSaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(currentSaveFilePath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                
                retryCount = data.retryCount;
                Debug.Log($"💾 Загружено сохранение: Уровень {data.currentLevel} Часть {data.currentPart} | Попыток: {retryCount}");
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

    /// <summary>
    /// Сохраняет глобальный прогресс.
    /// </summary>
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

    /// <summary>
    /// Получает последний разблокированный уровень.
    /// </summary>
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

    #region UI Management

    private void SetupButtons()
    {
        if (againLevelButton != null)
        {
            againLevelButton.onClick.RemoveAllListeners();
            againLevelButton.onClick.AddListener(OnAgainButtonClick);
        }
        
        if (againLevelButtonSuccess != null)
        {
            againLevelButtonSuccess.onClick.RemoveAllListeners();
            againLevelButtonSuccess.onClick.AddListener(OnAgainButtonClick);
        }
    }

    /// <summary>
    /// Показывает панель неудачи и кнопку "Заново".
    /// Кнопка againLevelButtonSuccess отображается при неудаче (время вышло).
    /// </summary>
    public void ShowFailureUI()
    {
        HideAllUI();
        
        SetUIActive(failurePanel, true);
        SetAgainLevelButtonVisible(false);
        SetAgainLevelButtonSuccessVisible(true);
        
        Debug.Log($"❌ Уровень не пройден. Кнопка 'Заново' показана.");
    }

    /// <summary>
    /// Показывает панель успеха.
    /// Кнопка "Заново" скрыта при успешном прохождении последнего уровня.
    /// </summary>
    public void ShowSuccessUI()
    {
        HideAllUI();
        
        SetUIActive(successPanel, true);
        
        Debug.Log($"✅ Уровень пройден! Кнопка 'Заново' скрыта.");
    }

    /// <summary>
    /// Скрывает все UI элементы.
    /// </summary>
    private void HideAllUI()
    {
        SetUIActive(successPanel, false);
        SetUIActive(failurePanel, false);
        SetAgainLevelButtonVisible(false);
    }

    /// <summary>
    /// Управляет видимостью кнопки "Заново".
    /// </summary>
    private void SetAgainLevelButtonVisible(bool visible)
    {
        if (againLevelButton != null)
        {
            againLevelButton.gameObject.SetActive(visible);
            againLevelButton.interactable = visible;
        }
    }

    /// <summary>
    /// Управляет видимостью кнопки "Заново" (успех).
    /// </summary>
    private void SetAgainLevelButtonSuccessVisible(bool visible)
    {
        if (againLevelButtonSuccess != null)
        {
            againLevelButtonSuccess.gameObject.SetActive(visible);
            againLevelButtonSuccess.interactable = visible;
        }
    }

    private void SetUIActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// Данные сохранения уровня.
    /// </summary>
    [Serializable]
    public class GameData
    {
        public int lastUnlockedLevel;
        public int currentLevel;
        public int currentPart;
        public int retryCount;
        public string saveTimestamp;
    }

    /// <summary>
    /// Глобальные данные сохранения.
    /// </summary>
    [Serializable]
    public class GlobalGameData
    {
        public int lastUnlockedLevel;
        public string saveTimestamp;
    }

    #endregion
    }
}
