using UnityEngine;
using TMPro;

public class TotalStars : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starText;

    private const int MinStars = 72;
    private const int MaxStars = 216;

    private void Start()
    {
        int earnedStars = CalculateTotalEarnedStars();
        SetTotalStars(earnedStars);
    }

    /// <summary>
    /// Вычисляет общее количество заработанных звёзд через LevelMenuManager.
    /// </summary>
private int CalculateTotalEarnedStars()
    {
        int total = 0;
        // 36 уровней × 2 части (Morning + Evening)
        for (int levelIndex = 0; levelIndex < 36; levelIndex++)
        {
            for (int partIndex = 0; partIndex < 2; partIndex++)
            {
                total += LevelMenuManager.GetStarsStatic(levelIndex, partIndex);
            }
        }
        return total;
    }

    /// <summary>
    /// Обновляет текст сертификата, отображая количество полученных звёзд.
    /// Значение звёзд ограничивается диапазоном от 72 до 216.
    /// </summary>
    /// <param name="earnedStars">Общее количество звёзд, полученных игроком.</param>
    public void SetTotalStars(int earnedStars)
    {
        int displayStars = Mathf.Clamp(earnedStars, MinStars, MaxStars);

        starText.text = $"Сертификат достаётся вам, в связи с тем, что вы успешно прошли Hour-peak, " +
            $"доказывая, что оптимизировать маршрут в условиях утренних и вечерних часов-пик возможно. " +
            $"Вы вообще логистический гений. Так держать\nОценивается успех прохождения {displayStars} звёзд из {MaxStars}";
    }
}