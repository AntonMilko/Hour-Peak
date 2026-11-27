using UnityEngine;
using UnityEngine.UI;

public class TheDisplayingOfStars : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [Tooltip("Ссылки на звезды")]
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;

    [Header("Settings")]
    [Tooltip("Текущее состояние фона: true = Утро (Белый), false = Вечер (Чёрный)")]
    public bool isMorning = true;

    [Header("Colors")]
    [Tooltip("Цвет звёзд при успешном прохождении")]
    public Color colorPass = Color.yellow;
    
    [Tooltip("Цвет звёзд при проигрыше (используется, если isMorning = true)")]
    public Color colorFailMorning = Color.black;
    
    [Tooltip("Цвет звёзд при проигрыше (используется, если isMorning = false)")]
    public Color colorFailEvening = Color.white;

    #endregion

    #region Private State

    private int starsCollected = 0;
    private bool isLevelPassed = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        SetStars(0);
    }

    #endregion

    #region Public Methods

    public void SetLevelResult(bool passed, int count)
    {
        starsCollected = count;
        isLevelPassed = passed;
        SetStars(count);
    }

    public void SetBackgroundMode(bool morning)
    {
        isMorning = morning;
        ShowSpecificCount(starsCollected);
    }

    #endregion

    #region Private Methods

    public void SetStars(int index)
    {
        int count = 0;

        switch (index)
        {
            case 1: count = 3; break; // Превосходно
            case 2: count = 2; break; // Супер
            case 3: count = 1; break; // Отлично
            case 4: count = 0; break; // Не пройден
            default: count = 0; break;
        }

        ShowSpecificCount(count);
    }

    private void ShowSpecificCount(int count)
    {
        bool show1 = count >= 1;
        bool show2 = count >= 2;
        bool show3 = count >= 3;

        ApplyStarState(star1, show1);
        ApplyStarState(star2, show2);
        ApplyStarState(star3, show3);
    }

    private void ApplyStarState(Image starImage, bool isVisible)
    {
        if (isVisible)
        {
            starImage.color = colorPass;
        }
        else
        {
            if (isMorning)
                starImage.color = colorFailMorning;
            else
                starImage.color = colorFailEvening;
        }
    }

    public void HideStars()
    {
        ShowSpecificCount(0);
    }

    #endregion
}