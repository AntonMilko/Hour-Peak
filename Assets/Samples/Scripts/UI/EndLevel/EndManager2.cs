using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    public string sceneGameCertificate;
    public string sceneAgainLevel;
    public string scenePartMainMenu;
    public string sceneMainMenu;

    private float elapsedTime;
    private float timeLimit;
    private float timeRatio;

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OpenGameCertificate);
        
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

    public void OpenGameCertificate()
    {
        if (!string.IsNullOrEmpty(sceneGameCertificate))
            SceneManager.LoadScene(sceneGameCertificate);
    }

    public void OpenPartMainMenu()
    {
        if (!string.IsNullOrEmpty(scenePartMainMenu))
            SceneManager.LoadScene(scenePartMainMenu);
    }

    public void AgainLevel()
    {
        if (!string.IsNullOrEmpty(sceneAgainLevel))
            SceneManager.LoadScene(sceneAgainLevel);
    }

    public void ExitLevel()
    {
        if (!string.IsNullOrEmpty(sceneMainMenu))
            SceneManager.LoadScene(sceneMainMenu);
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
            nextButton.onClick.RemoveListener(OpenGameCertificate);
        
        if (againButton != null)
            againButton.onClick.RemoveListener(AgainLevel);
        
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitLevel);
    }
}