using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndManager2 : MonoBehaviour
{
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
        ShowFinalSuccessScreen();
        ShowFinalFailureScreen();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПОБЕДЕ НА ФИНАЛЬНОЙ ЧАСТИ ФИНАЛЬНОГО УРОВНЯ.
    /// ПРАВИЛО: Горит ТОЛЬКО nextButton. Все остальные скрыты.
    /// </summary>
    public void ShowFinalSuccessScreen()
    {
        Debug.Log("ФИНАЛЬНАЯ ЧАСТЬ ФИНАЛЬНОГО УРОВНЯ ПРОЙДЕНА УСПЕШНО!");

        if (timeText != null) 
            timeText.text = "Время: Отлично! Уровень пройден.";

        if (endMenuPanel != null) 
            endMenuPanel.SetActive(true);

        if (!string.IsNullOrEmpty(sceneCertificate))
        {
            SceneManager.LoadScene(sceneCertificate);
        }
    }

    /// <summary>
    /// ВЫЗЫВАТЬ ПРИ ПРОВАЛЕ НА ФИНАЛЬНОЙ ЧАСТИ ФИНАЛЬНОГО УРОВНЯ.
    /// ПРАВИЛО: Скрыт только nextButton. Все остальные горят.
    /// </summary>
    public void ShowFinalFailureScreen()
    {
        Debug.Log("ФИНАЛЬНАЯ ЧАСТЬ ФИНАЛЬНОГО УРОВНЯ НЕ ПРОЙДЕНА!");

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