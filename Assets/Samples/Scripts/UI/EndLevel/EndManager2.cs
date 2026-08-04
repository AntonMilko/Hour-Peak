using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using HourPeak;

public class EndManager2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenuPanel;
    public Button nextButton;
    public Button againButton;
    public Button exitButton;
    
    [Header("Time Evaluation")]
    public TextMeshProUGUI timeText;
    
    [Header("Scene Names")]
    public string sceneAgainLevel;
    public string sceneMainMenu;

    private float elapsedTime;
    private float timeLimit;
    private float timeRatio;

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        
        if (againButton != null)
            againButton.onClick.AddListener(AgainLevel);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitLevel);
    }

    /// <summary>
    /// Вызывается при достижении финиша для инициализации меню.
    /// </summary>
    public void Initialize(float passedTime, float limit)
    {
        elapsedTime = passedTime;
        timeLimit = limit;
        timeRatio = limit > 0 ? elapsedTime / limit : 1f;
        
        EvaluateAndShowResult();
    }

    private void EvaluateAndShowResult()
    {
        Color color;

        if (timeRatio <= 0.25f)
            color = new Color(0f, 0.8f, 0f);
        else if (timeRatio <= 0.5f)
            color = new Color(0f, 0.5f, 1f);
        else if (timeRatio <= 0.75f)
            color = new Color(1f, 0.8f, 0f);
        else if (timeRatio <= 1f)
            color = new Color(1f, 0.3f, 0f);
        else
            color = Color.black;

        if (timeText != null)
        {
            timeText.text = $"Время: {FormatTime(elapsedTime)} / {FormatTime(timeLimit)}";
            timeText.color = color;
        }

        if (endMenuPanel != null)
            endMenuPanel.SetActive(true);
        
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Кнопка "Далее" — сохраняет прогресс и переходит к следующей части уровня.
    /// </summary>
    public void OnNextClicked()
    {
        // Определяем номер уровня и части из имени сцены
        string currentScene = SceneManager.GetActiveScene().name;
        int levelIndex = 0;
        int partIndex = 0;
        
        if (currentScene.Contains("Morning"))
        {
            levelIndex = ExtractLevelFromScene(currentScene, true);
            partIndex = 0; // Morning
        }
        else if (currentScene.Contains("Evening"))
        {
            levelIndex = ExtractLevelFromScene(currentScene, false);
            partIndex = 1; // Evening
        }
        
        // Вычисляем звёзды
        int stars = timeRatio <= 0.25f ? 3 : (timeRatio <= 0.5f ? 3 : (timeRatio <= 0.75f ? 2 : 1));
        
// Сохраняем прогресс статически (работает без экземпляра LevelMenuManager)
        LevelMenuManager.SaveProgressStatic(levelIndex, partIndex, stars);
        Debug.Log($"💾 Сохранён прогресс: Уровень {levelIndex + 1}, Часть {partIndex + 1}, Звёзды: {stars}");

        // Восстанавливаем время перед загрузкой новой сцены
        Time.timeScale = 1f;

        // Переход к следующей части уровня
        StartCoroutine(LevelMenuManager.LoadNextLevelPartStatic());
    }

    /// <summary>
    /// Извлекает номер уровня из имени сцены.
    /// </summary>
    private int ExtractLevelFromScene(string sceneName, bool isMorning)
    {
        int level = 1;
        
        for (int i = 0; i < sceneName.Length; i++)
        {
            if (sceneName[i] >= '0' && sceneName[i] <= '9')
            {
                int start = i;
                while (i < sceneName.Length && sceneName[i] >= '0' && sceneName[i] <= '9')
                {
                    i++;
                }
                string numStr = sceneName.Substring(start, i - start);
                if (int.TryParse(numStr, out int parsedNum) && parsedNum > 0 && parsedNum <= 36)
                {
                    level = parsedNum - 1; // 0-based
                    break;
                }
            }
        }
        
        return level;
    }

    public void AgainLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(sceneAgainLevel))
            SceneManager.LoadScene(sceneAgainLevel);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenu");
    }

    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes}:{seconds:00}";
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextClicked);
        
        if (againButton != null)
            againButton.onClick.RemoveListener(AgainLevel);
        
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitLevel);
    }
}