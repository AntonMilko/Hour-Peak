using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndManager : MonoBehaviour
{
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

    [Tooltip("Имя сцены, куда начинают текущую часть текущего уровеня с 0.")]
    [SerializeField] private string sceneAgainLevel;

    [Tooltip("Имя части сцены, куда переходят в главное меню.")]
    [SerializeField] private string partSceneMainMenu;

    [Tooltip("Имя сцены, куда переходят в главное меню.")]
    [SerializeField] private string sceneMainMenu;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (endMenuPanel != null) 
            endMenuPanel.SetActive(false);
    }

    private void Update()
    {
        ShowCurrentSuccessScreen();
        ShowCurrentFailureScreen();
    }

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

        if (!string.IsNullOrEmpty(sceneNextLevel))
        {
            SceneManager.LoadScene(sceneNextLevel);
        }

        if (!string.IsNullOrEmpty(sceneAgainLevel))
        {
            SceneManager.LoadScene(sceneAgainLevel);
        }

        if (!string.IsNullOrEmpty(sceneMainMenu))
        {
            SceneManager.LoadScene(partSceneMainMenu);
        }
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

        if (!string.IsNullOrEmpty(sceneAgainLevel))
        {
            SceneManager.LoadScene(sceneAgainLevel);
        }

        if (!string.IsNullOrEmpty(sceneMainMenu))
        {
            SceneManager.LoadScene(partSceneMainMenu);
        }
    }

    #endregion
}