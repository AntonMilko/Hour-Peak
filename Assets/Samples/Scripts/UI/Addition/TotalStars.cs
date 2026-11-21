using UnityEngine;
using TMPro;
using System.Linq;

public class TotalStars : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starText;

    private void Start()
    {
        int earnedStars = LevelMenuManager._morningStars.Sum() + LevelMenuManager._eveningStars.Sum();
        SetTotalStars(earnedStars);
    }

    /// <summary>
    /// Обновляет текст сертификата, отображая количество полученных звёзд.
    /// Значение звёзд ограничивается диапазоном от 72 до 216.
    /// </summary>
    /// <param name="earnedStars">Общее количество звёзд, полученных игроком.</param>
    public void SetTotalStars(int earnedStars)
    {
        starText.text = $"Сертификат достаётся вам, в связи с тем, что вы успешно прошли Hour-peak, " +
            $"доказывая, что оптимизировать маршрут в условиях утренних и вечерних часов-пик возможно. " +
            $"Вы вообще логистический гений. Так держать\nОценивается успех прохождения {earnedStars} звёзд из {216}";
    }
}