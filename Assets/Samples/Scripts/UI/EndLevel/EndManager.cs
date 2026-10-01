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

    [Header("Button References")]
    [Tooltip("Кнопка перехода к следующему уровню.")]
    [SerializeField] private Button nextButton;

    [Tooltip("Кнопка повтора уровня.")]
    [SerializeField] private Button againButton;

    [Tooltip("Кнопка выхода в главное меню.")]
    [SerializeField] private Button exitButton;
    
    [Header("Time Evaluation")]
    [Tooltip("Текст отображающий текущее время и ограничение.")]
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Scene Names")]
    [Tooltip("Имя сцены, куда переходят на следующий уровень.")]
    [SerializeField] private string sceneNextLevel;

    [Tooltip("Имя сцены, куда начинают текущий уровень с 0.")]
    [SerializeField] private string sceneAgainLevel;

    [Tooltip("Имя части сцены, куда переходят в главное меню.")]
    [SerializeField] private string scenePartMainMenu;

    [Tooltip("Имя сцены, куда переходят в главное меню.")]
    [SerializeField] private string sceneMainMenu;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (endMenuPanel != null) 
            endMenuPanel.SetActive(false);
        
        if (nextButton != null) 
            nextButton.gameObject.SetActive(false);
            
        if (againButton != null) 
            againButton.gameObject.SetActive(false);
            
        if (exitButton != null) 
            exitButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        ShowFinalSuccessScreen();
        SetupNextButtonLogic();
        RestartLevel();
        ExitToMenu();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПОБЕДЕ НА ФИНАЛЬНОМ УРОВНЕ.
    /// ПРАВИЛО: Горит ТОЛЬКО nextButton. Все остальные скрыты.
    /// ВЫЗВАТЬ ПРИ ПРОВАЛЕ НА ФИНАЛЬНОМ УРОВНЕ.
    /// ПРАВИЛО: Скрыт только nextButton. Все остальные горят.
    /// </summary>
    public void ShowFinalSuccessScreen()
    {
        Debug.Log("ФИНАЛ ПРОЙДЕН УСПЕШНО!");

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            againButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            Debug.LogError("Пройдена последняя часть последнего уровня!");
        }
        else
        {
            nextButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(true);
            againButton.onClick.RemoveAllListeners();
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.RemoveAllListeners();
            Debug.LogError("Не пройдена последняя часть последнего уровня!");
        }

        if (timeText != null) 
            timeText.text = "Время: Отлично! Уровень пройден.";

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Переход на следующую часть текущего (следующего) уровня на случай успешного прохождения текущей части текущего уровня.
    /// </summary>
    public void SetupNextButtonLogic()
    {
        if (!string.IsNullOrEmpty(sceneNextLevel))
        {
            SceneManager.LoadScene(sceneNextLevel);
        }
    }

    /// <summary>
    /// Старт текущей части текущего уровня с 0 на случай неуспешного прохождения.
    /// </summary>
    public void RestartLevel()
    {
        if (!string.IsNullOrEmpty(sceneAgainLevel))
        {
            SceneManager.LoadScene(sceneAgainLevel);
        }
    }

    /// <summary>
    /// Выход в главное меню.
    /// </summary>
    public void ExitToMenu()
    {
        if (!string.IsNullOrEmpty(sceneMainMenu))
        {
            SceneManager.LoadScene(scenePartMainMenu);
        }
    }

    #endregion
}