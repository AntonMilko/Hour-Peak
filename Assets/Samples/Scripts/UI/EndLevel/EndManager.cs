using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenuPanel;
    public Button nextButton;
    public Button againButton;
    public Button exitButton;
    
    [Header("Time Evaluation")]
    public TextMeshProUGUI timeText;
    
    [Header("Scene Names")]
    public string sceneNextLevel;
    public string sceneAgainLevel;
    public string scenePartMainMenu;
    public string sceneMainMenu;

    private float elapsedTime;
    private float timeLimit;
    private float timeRatio;

    private enum EvaluationResult
    {
        Perfect,
        Super,
        Excellent,
        OnTime,
        Failed
    }

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextLevel);
        
        if (againButton != null)
            againButton.onClick.AddListener(AgainLevel);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitLevel);
    }

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
        {
            color = new Color(0f, 0.8f, 0f);
        }
        else if (timeRatio <= 0.5f)
        {
            color = new Color(0f, 0.5f, 1f);
        }
        else if (timeRatio <= 0.75f)
        {
            color = new Color(1f, 0.8f, 0f);
        }
        else if (timeRatio <= 1f)
        {
            color = new Color(1f, 0.3f, 0f);
        }
        else
        {
            color = Color.black;
        }

        if (timeText != null)
        {
            timeText.text = $"Время: {FormatTime(elapsedTime)} / {FormatTime(timeLimit)}";
            timeText.color = color;
        }

        if (endMenuPanel != null)
            endMenuPanel.SetActive(true);
        
        Time.timeScale = 0f;
    }

    public void NextLevel()
    {
        if (!string.IsNullOrEmpty(sceneNextLevel))
            SceneManager.LoadScene(sceneNextLevel);
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

    public void OpenPartMainMenu()
    {
        if (!string.IsNullOrEmpty(scenePartMainMenu))
            SceneManager.LoadScene(scenePartMainMenu);
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
            nextButton.onClick.RemoveListener(NextLevel);
        
        if (againButton != null)
            againButton.onClick.RemoveListener(AgainLevel);
        
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitLevel);
    }
}