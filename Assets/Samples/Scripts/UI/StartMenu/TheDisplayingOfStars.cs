using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт отображения звёзд в зависимости от результата уровня.
/// Логика цветов:
/// - Успех: Жёлтые.
/// - Провал (Утро/Белый фон): Чёрные.
/// - Провал (Вечер/Чёрный фон): Белые.
/// </summary>
public class TheDisplayingOfStars : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [Tooltip("Массив изображений звёзд (Image). Должно быть 3 элемента.")]
    [SerializeField] private Image[] starImages;

    [Header("Settings")]
    [Tooltip("Текущее состояние фона: true = Утро (Белый), false = Вечер (Чёрный)")]
    [SerializeField] private bool isMorning = true;

    [Header("Colors")]
    [Tooltip("Цвет звёзд при успешном прохождении")]
    [SerializeField] private Color colorPass = Color.yellow;
    
    [Tooltip("Цвет звёзд при проигрыше (используется, если isMorning = true)")]
    [SerializeField] private Color colorFailMorning = Color.black;
    
    [Tooltip("Цвет звёзд при проигрыше (используется, если isMorning = false)")]
    [SerializeField] private Color colorFailEvening = Color.white;

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
    /// Обновляет видимость звёзд (показывает только первые 'starsCollected' штук).
    /// </summary>
    private void UpdateStarVisibility()
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            // Показываем звезду, если её индекс меньше количества собранных звёзд
            starImages[i].gameObject.SetActive(i < starsCollected);
        }
    }

    /// <summary>
    /// Устанавливает правильный цвет звёзд в зависимости от фона и результата.
    /// </summary>
    private void UpdateStarColors()
    {
        if (starImages == null) return;

        Color targetColor = Color.clear;

        // Логика выбора цвета
        if (isLevelPassed)
        {
            // Если уровень пройден — всегда жёлтый
            targetColor = colorPass;
        }
        else
        {
            // Если уровень провален — цвет зависит от фона
            targetColor = isMorning ? colorFailMorning : colorFailEvening;
        }

        // Применяем цвет ко всем звёздам (даже неактивным, чтобы при появлении они были нужного цвета)
        foreach (Image star in starImages)
        {
            if (star != null)
            {
                star.color = targetColor;
            }
        }
    }

    #endregion
}