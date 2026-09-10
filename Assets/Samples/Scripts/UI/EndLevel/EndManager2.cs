using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndManager2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endMenuPanel;
    
    // --- ГРУППА УСПЕХА ---
    public Button nextButton; 
    public Button againButton; 
    public Button exitButton; 

    // --- ГРУППА НЕУДАЧИ ---
    public Button nextFailedButton; 
    public Button againFailedButton; 
    public Button exitFailedButton; 

    [Header("Time Evaluation")]
    public TextMeshProUGUI timeText;

    [Header("Scene Names")]
    public string sceneAgainLevel;      
    public string sceneMainMenuLevel;   

    void Start()
    {
        if (endMenuPanel != null) 
            endMenuPanel.SetActive(false);
    }

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПОБЕДЕ НА ФИНАЛЬНОМ УРОВНЕ.
    /// ПРАВИЛО: Горит ТОЛЬКО nextButton. Все остальные скрыты.
    /// </summary>
    public void ShowFinalSuccessScreen()
    {
        Debug.Log("[END] ФИНАЛ ПРОЙДЕН!");
        
        HideAllButtons();

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            SetupNextButtonLogic();
        }
        else
        {
            Debug.LogError("[ERROR] Не назначена кнопка 'nextButton'!");
        }

        if (timeText != null) 
            timeText.text = "Время: Отлично! Уровень пройден.";

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);
    }

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПРОИГРЫШЕ НА ФИНАЛЬНОМ УРОВНЕ.
    /// ПРАВИЛО: Горят ТОЛЬКО againFailedButton и exitFailedButton.
    /// </summary>
    public void ShowFailureScreen()
    {
        Debug.Log("[END] УРОВЕНЬ НЕ ПРОЙДЕН.");

        HideAllButtons();

        if (againFailedButton != null)
        {
            againFailedButton.gameObject.SetActive(true);
            againFailedButton.onClick.RemoveAllListeners();
            againFailedButton.onClick.AddListener(RestartLevel);
        }

        if (exitFailedButton != null)
        {
            exitFailedButton.gameObject.SetActive(true);
            exitFailedButton.onClick.RemoveAllListeners();
            exitFailedButton.onClick.AddListener(ExitToMenu);
        }

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);
    }

    private void SetupNextButtonLogic()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            Debug.Log("[TRANSITION] Переход на сертификат...");
            
            // ТОЛЬКО стандартная загрузка сцены. Никаких контроллеров.
            SceneManager.LoadScene("Game completion certificate");
        });
    }

    public void RestartLevel()
    {
        if (!string.IsNullOrEmpty(sceneAgainLevel))
        {
            SceneManager.LoadScene(sceneAgainLevel);
        }
    }

    public void ExitToMenu()
    {
        if (!string.IsNullOrEmpty(sceneMainMenuLevel))
        {
            SceneManager.LoadScene(sceneMainMenuLevel);
        }
    }

    private void HideAllButtons()
    {
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (nextFailedButton != null) nextFailedButton.gameObject.SetActive(false); 
        if (againButton != null) againButton.gameObject.SetActive(false);
        if (againFailedButton != null) againFailedButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);
        if (exitFailedButton != null) exitFailedButton.gameObject.SetActive(false);
    }
}