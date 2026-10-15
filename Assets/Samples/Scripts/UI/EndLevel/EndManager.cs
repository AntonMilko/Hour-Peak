using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class EndManager : MonoBehaviour
{
    #region Constants

    private const string SAVE_FOLDER_NAME = "GameSaves";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const string GLOBAL_SAVE_FILENAME = "global_save.json";

    #endregion
    
    #region Fields

    [Header("UI References")]
    [Tooltip("Панель окончания игры.")]
    [SerializeField] private GameObject endMenuPanel;
    
    [Header("Time Evaluation")]
    [Tooltip("Текст отображающий текущее время и ограничение.")]
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Scene Names")]
    [Tooltip("Имя сцены, куда переходят на следующую часть текущего (следующего) уровеня.")]
    [SerializeField] private string sceneNextLevel;

    [Tooltip("Имя части сцены, куда переходят в главное меню.")]
    [SerializeField] private string sceneAgainLevel;

    [Tooltip("Имя сцены, куда переходят в главное меню.")]
    [SerializeField] private string sceneMainMenu;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (timeText != null) 
            timeText.text = "Время идёт! Торопись.";
        
        if (endMenuPanel != null) 
            endMenuPanel.SetActive(false);
    }

    private void Update()
    {
        ShowCurrentSuccessScreen();
        ShowCurrentFailureScreen();
        ChangeScene();
        NextLevel();
        CreateSaveFilePath();
        SaveProgress();
    }

    #endregion

    #region Private State

    private bool isShowSuccessScreen = false;
    private bool isShowFailureScreen = false;
    private bool loadSceneMainMenu = false;
    private bool loadSceneAgainLevel = false;
    private bool levelSuccess;
    private string currentSaveFilePath;
    private int currentLevelNumber;
    private int currentPartIndex;
    private int starsCollected;

    #endregion

    #region Public Methods

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПОБЕДЕ НА ТЕКУЩЕЙ ЧАСТИ ТЕКУЩЕГО УРОВНЯ.
    /// ПРАВИЛО: Горит ТОЛЬКО nextButton. Все остальные также горят.
    /// </summary>
    public void ShowCurrentSuccessScreen()
    {
        Debug.Log("ЧАСТЬ УРОВНЯ ПРОЙДЕНА УСПЕШНО!");

        if (timeText != null) 
            timeText.text = "Время: Отлично! Уровень пройден.";

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);

        isShowSuccessScreen = true;
    }

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПРОВАЛЕ НА ТЕКУЩЕЙ ЧАСТИ ТЕКУЩЕГО УРОВНЯ.
    /// ПРАВИЛО: Скрыт только nextButton. Все остальные горят.
    /// </summary>
    public void ShowCurrentFailureScreen()
    {
        Debug.Log("ЧАСТЬ УРОВНЯ НЕ ПРОЙДЕНА!");

        if (timeText != null) 
            timeText.text = "Время: Попробуйте ещё раз!";

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);

        isShowFailureScreen = true;
    }

    #endregion

    #region Transition Management

    /// <summary>
    /// Менять сцену на главное меню или на текущую часть текущего уровня.
    /// </summary>
    private void ChangeScene()
    {
        if (loadSceneMainMenu)
        {
            SceneManager.LoadScene(sceneMainMenu);
        }
        else if (loadSceneAgainLevel)
        {
            SceneManager.LoadScene(sceneAgainLevel);
        }
    }

    /// <summary>
    /// Перейти на следующую часть текущего (следующего) уровня.
    /// </summary>
    private void NextLevel()
    {
        SceneManager.LoadScene(sceneNextLevel);
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

    #region Data Structures

    [Serializable]
    public class GameData
    {
        public int lastUnlockedLevel;
        public int currentLevel;
        public int currentPart;
        public int starsCollected;
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