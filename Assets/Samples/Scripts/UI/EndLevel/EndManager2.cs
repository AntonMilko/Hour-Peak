using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public string sceneCertificate;
    public string sceneAgainLevel;      
    public string scenePartMainMenu;
    public string sceneMainMenu;

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
        if (!string.IsNullOrEmpty(sceneMainMenu))
        {
            SceneManager.LoadScene(scenePartMainMenu);
        }
    }
}