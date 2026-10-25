using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;

public class EndManager2 : MonoBehaviour
{
    #region Constants

    private const string SAVE_FOLDER_NAME = "GameSaves";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const string GLOBAL_SAVE_FILENAME = "global_save.json";

    #endregion

    #region fields

    [Header("UI References")]
    [Tooltip("Панель окончания уровня.")]
    [SerializeField] private GameObject endMenuPanel;

    [Header("Time Evaluation")]
    [Tooltip("Текст отображающий текущее время и ограничение.")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Scene Names")]
    [Tooltip("Имя сцены, куда переходят на сертификат.")]
    [SerializeField] private string sceneCertificate;
    
    [Tooltip("Имя сцены, куда начинают финальную часть финального уровня с 0.")]
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
        ChangeScene2();
        NextLevel2();
        CreateSaveFilePath();
        SaveProgress();
    }

    #endregion

    #region Private State
    
    private bool isShowSuccessScreen = false;
    private bool isShowFailureScreen = false;
    private bool loadSceneMainMenu = false;
    private bool loadSceneAgainLevel = false;
    private string currentSaveFilePath;
    private object isCertificateActive;
    private bool lastLevelSuccess;
    private int starsCollected;

    #endregion

    #region Public Methods

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПОБЕДЕ НА ТЕКУЩЕЙ ЧАСТИ ТЕКУЩЕГО УРОВНЯ.
    /// ПРАВИЛО: Горит ТОЛЬКО nextButton. Все остальные скрыты.
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
    private void ChangeScene2()
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
    /// Перейти на сцену сертификата.
    /// </summary>
    private void NextLevel2()
    {
        SceneManager.LoadScene(sceneCertificate);
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
            
        currentSaveFilePath = Path.Combine(folderPath, $"Last Level_{isCertificateActive:Game completion certificate}{SAVE_FILE_EXTENSION}");
    }

    public void SaveProgress()
    {
        GameData data = new GameData
        {
            lastUnlockedLevel = lastLevelSuccess ? Mathf.Max(GetLastUnlockedLevel() + (lastLevelSuccess ? 1 : 0)) : GetLastUnlockedLevel(),
            certificateSceneName = lastLevelSuccess,
            starsCollected = starsCollected,
            saveTimestamp = DateTime.Now.ToString("yyyy.MM.dd_HH:mm:ss")
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(currentSaveFilePath, json);
                
            SaveGlobalProgress(data.lastUnlockedLevel);
                
            Debug.Log($"💾 Сохранение: {currentSaveFilePath} | Уровень разблокирован: {data.lastUnlockedLevel}");}

            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка сохранения: {e.Message}");
            }
        }

        private void SaveGlobalProgress(object lastUnlockedLevel)
        {
            throw new NotImplementedException();
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
        public object certificateSceneName;
        public string saveTimestamp;
        public int starsCollected;
        public object lastUnlockedLevel;
    }

    [Serializable]
    public class GlobalGameData
    {
        public int lastUnlockedLevel;
        public string saveTimestamp;
    }

    #endregion
}