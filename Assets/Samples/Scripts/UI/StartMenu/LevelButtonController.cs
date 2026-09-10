using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelButtonController : MonoBehaviour
{
    [Header("Данные уровня (заполняются генератором)")]
    public int levelIndex;
    public int stageIndex;

    [Header("Ссылки на UI (автопоиск)")]
    [SerializeField] private Image morningPanel;
    [SerializeField] private Image eveningPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TheDisplayingOfStars morningStarsScript;
    [SerializeField] private TheDisplayingOfStars eveningStarsScript;

    [Header("Анимация перехода")]
    [SerializeField] private GameObject crossFadeObject;

    private Button _button;

    void Awake()
    {
        // Автопоиск компонентов
        if (morningPanel == null) morningPanel = transform.Find("MorningPanel")?.GetComponent<Image>();
        if (eveningPanel == null) eveningPanel = transform.Find("EveningPanel")?.GetComponent<Image>();
        if (levelText == null) levelText = transform.Find("LevelNumberText")?.GetComponent<TextMeshProUGUI>();
        
        if (morningStarsScript == null) 
            morningStarsScript = transform.Find("MorningPanel")?.GetComponent<TheDisplayingOfStars>();
        if (eveningStarsScript == null) 
            eveningStarsScript = transform.Find("EveningPanel")?.GetComponent<TheDisplayingOfStars>();

        if (crossFadeObject == null)
            crossFadeObject = transform.Find("levelLoader/CrossFade")?.gameObject;

        _button = GetComponent<Button>();
    }

    void Start()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        int stars = ProgressManager.GetStars(levelIndex, stageIndex);
        bool isUnlocked = stars > 0;

        _button.interactable = isUnlocked;

        if (!isUnlocked)
        {
            // ЗАБЛОКИРОВАНО
            if(morningPanel) morningPanel.color = new Color(0.3f, 0.3f, 0.3f);
            if(eveningPanel) eveningPanel.color = new Color(0.3f, 0.3f, 0.3f);
            levelText.text = "Закрыто";
            
            // 🔥 ВАЖНО: Эти методы вызовутся ТОЛЬКО если они есть в скрипте TheDisplayingOfStars
            if(morningStarsScript) morningStarsScript.HideStars();
            if(eveningStarsScript) eveningStarsScript.HideStars();
        }
        else
        {
            // АКТИВНО
            if(morningPanel) morningPanel.color = Color.white;
            if(eveningPanel) eveningPanel.color = new Color(0.1f, 0.1f, 0.1f);
            
            string baseText = $"Ур. {levelIndex}";
            if (levelIndex == ProgressManager.GetLastLevel() && stageIndex == ProgressManager.GetLastStage())
            {
                baseText += " (Продолжить)";
            }
            levelText.text = baseText;

            if(morningStarsScript) morningStarsScript.SetStars(stars);
            if(eveningStarsScript) eveningStarsScript.SetStars(stars);
        }
    }

    public void OnClick()
    {
        Debug.Log($"[КЛИК] Обработка кнопки L{levelIndex}_S{stageIndex}");

        if (crossFadeObject != null)
        {
            Debug.Log("Запуск анимации CrossFade...");
            // Тут будет твой код анимации
        }

        string targetScene = DetermineTargetScene();
        Debug.Log($"Переход на сцену: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    private string DetermineTargetScene()
    {
        int stars = ProgressManager.GetStars(levelIndex, stageIndex);

        if (stars == 0)
        {
            return $"L{levelIndex}_S{stageIndex}";
        }
        else
        {
            int nextStage = stageIndex + 1;
            int nextLevel = levelIndex;

            if (nextStage > 2) 
            {
                nextStage = 1;
                nextLevel++;
            }

            if (nextLevel > 18) return "MainMenu"; 

            return $"L{nextLevel}_S{nextStage}";
        }
    }
}