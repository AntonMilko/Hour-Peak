using UnityEngine;
using UnityEngine.UI;

namespace HourPeak.Samples.Runtime.StarAppearTimings
{
    public class TheAppearanceOfStars : MonoBehaviour
    {
        #region Fields

        [Header("Star Visuals")]
        [Tooltip("Image компоненты для смены цвета (жёлтый/чёрный)")]
        [SerializeField] private Image starImage1;
        [SerializeField] private Image starImage2;
        [SerializeField] private Image starImage3;

        [Tooltip("Жёлтый цвет (активная звезда)")]
        [SerializeField] private Color yellowColor = Color.yellow;

        [Tooltip("Чёрный цвет (неактивная звезда)")]
        [SerializeField] private Color blackColor = Color.black;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            SetStarForElapsedTime();
        }

        #endregion

        #region Public Methods

        private void SetStarForElapsedTime()
        {
            if (TimerDisplay.time < TimerDisplay.totalTime/8)
            {
                starImage1.color = yellowColor;
                starImage2.color = yellowColor;
                starImage3.color = yellowColor;
            }
            else if (TimerDisplay.time < TimerDisplay.totalTime/4)
            {
                starImage1.color = yellowColor;
                starImage2.color = yellowColor;
                starImage3.color = blackColor;
            }
            else if (TimerDisplay.time < TimerDisplay.totalTime)
            {
                starImage1.color = yellowColor;
                starImage2.color = blackColor;
                starImage3.color = blackColor;
            }
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
