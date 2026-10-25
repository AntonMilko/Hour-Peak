using UnityEngine;
using UnityEngine.UI;

namespace HourPeak.Samples.Runtime.StarDisappearTimings
{
    public class TheDisappearanceOfStars : MonoBehaviour
    {
        #region Fields

        [Header("Star Visuals")]
        [Tooltip("Image компоненты для смены цвета (жёлтый/чёрный)")]
        [SerializeField] private Image starImage1;
        [SerializeField] private Image starImage2;
        [SerializeField] private Image starImage3;

        [Header("Colors")]
        [Tooltip("Жёлтый цвет (активная звезда)")]
        [SerializeField] private Color yellowColor = new Color(1f, 1f, 0f);

        [Tooltip("Чёрный цвет (неактивная звезда)")]
        [SerializeField] private Color blackColor = new Color(0f, 0f, 0f);

        [Header("Settings")]
        [Tooltip("Скорость мигания")]
        [SerializeField] private float blinkSpeed = 10f;

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            SetStarForElapsedTime();
        }

        #endregion

        #region Public Methods

        

        #endregion

        #region Private Methods

        private void SetStarForElapsedTime()
        {
            // 🕐 0 - Star3DisappearTime: Все 3 звезды жёлтые
            if (TimerDisplay.time < TimerDisplay.totalTime/8)
            {
                starImage1.color = yellowColor;
                starImage2.color = yellowColor;
                starImage3.color = yellowColor;
            }
            // 🕐 Star3DisappearTime - Star2DisappearTime: Звезда 3 чёрная, 1-2 жёлтые
            else if (TimerDisplay.time < TimerDisplay.totalTime/4)
            {
                starImage1.color = yellowColor;
                starImage2.color = yellowColor;
                starImage3.color = blackColor;
            }
            // 🕐 Star2DisappearTime - Star1BlinkTime: Звезда 1 жёлтая (одна осталась), 2-3 чёрные
            else if (TimerDisplay.time < TimerDisplay.totalTime/2)
            {
                starImage1.color = yellowColor;
                starImage2.color = blackColor;
                starImage3.color = blackColor;
            }
            // 🕐 Star1BlinkTime - AllBlackTime: Звезда 1 жёлтая (она мигает), 2-3 чёрные
            else if (TimerDisplay.time < TimerDisplay.totalTime)
            {
                if(TimerDisplay.time % (blinkSpeed * 2f) < blinkSpeed)
                    starImage1.color = yellowColor;
                else
                    starImage1.color = blackColor;
                starImage2.color = blackColor;
                starImage3.color = blackColor;
            }
            // 🕐 AllBlackTime+: Все 3 звезды чёрные
            else
            {
                starImage1.color = blackColor;
                starImage2.color = blackColor;
                starImage3.color = blackColor;
            }
        }

        #endregion
    }
}