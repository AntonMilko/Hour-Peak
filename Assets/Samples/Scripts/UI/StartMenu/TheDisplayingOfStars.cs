using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт отображения звёзд в зависимости от результата уровня.
/// Все звёзды всегда видимы — состояние отображается цветом:
/// - Заработанные: цвет успеха (жёлтый).
/// - Незаработанные (Утро/Белый фон): чёрные.
/// - Незаработанные (Вечер/Чёрный фон): белые.
/// </summary>
public class TheDisplayingOfStars : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [Tooltip("Массив изображений звёзд (Image). Должно быть 3 элемента.")]
    public Image[] starImages;

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
        // Проверка на наличие звёзд
        if (starImages == null || starImages.Length == 0)
        {
            Debug.LogWarning($"[{GetType().Name}] Массив starImages пуст!", this);
        }
        
        // Начальное обновление цветов при старте
        UpdateStarColors();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Устанавливает результат уровня и количество звёзд.
    /// </summary>
    /// <param name="passed">Пройден ли уровень.</param>
    /// <param name="count">Количество собранных звёзд (0-3).</param>
    public void SetLevelResult(bool passed, int count)
    {
        isLevelPassed = passed;
        starsCollected = count;
        
        UpdateStarVisibility();
        UpdateStarColors();
    }

    /// <summary>
    /// Меняет режим фона (Утро/Вечер) и пересчитывает цвета звёзд.
    /// </summary>
    /// <param name="morning">true - Утро (белый фон), false - Вечер (чёрный фон).</param>
    public void SetBackgroundMode(bool morning)
    {
        isMorning = morning;
        UpdateStarColors();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Обновляет видимость звёзд.
    /// Все звёзды всегда видимы — состояние отображается цветом, а не скрытием.
    /// </summary>
    private void UpdateStarVisibility()
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Устанавливает правильный цвет звёзд в зависимости от фона и результата.
    /// Заработанные звёзды — цвет успеха, остальные — цвет фона.
    /// </summary>
    private void UpdateStarColors()
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            Color targetColor;

            if (isLevelPassed && i < starsCollected)
            {
                // Заработанная звезда — цвет успеха
                targetColor = colorPass;
            }
            else
            {
                // Незаработанная звезда — цвет зависит от фона
                targetColor = isMorning ? colorFailMorning : colorFailEvening;
            }

            starImages[i].color = targetColor;
        }
    }

    #endregion
}
