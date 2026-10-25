using UnityEngine;
using UnityEngine.UI;

namespace HourPeak.Samples.Runtime
{
    public class IndicatorSuccessForEndMenu : MonoBehaviour
    {
        #region Fields

        [Header("UI References")]
        [SerializeField] private Image progressFill;
        [SerializeField] private Slider resultSlider;
        
        [Header("Colors")]
        [Tooltip("Превосходно (шустро) - зелёный")]
        [SerializeField] private Color perfectColor = new Color(0f, 0.8f, 0f);
        
        [Tooltip("Супер (быстро) - синий")]
        [SerializeField] private Color superColor = new Color(0f, 0.5f, 1f);
        
        [Tooltip("Отлично (хорошо) - жёлтый")]
        [SerializeField] private Color excellentColor = new Color(1f, 0.8f, 0f);
        
        [Tooltip("Вовремя (нормально) - красный")]
        [SerializeField] private Color onTimeColor = new Color(1f, 0f, 0f);
        
        [Tooltip("Провал (опоздал) - чёрный")]
        [SerializeField] private Color failColor = Color.black;

        #endregion

        #region Unity Lifecycle

        void Update()
        {
            
            float timeRatio = TimerDisplay.time / TimerDisplay.totalTime;
            if (timeRatio < 0.25f)
                timeRatio = timeRatio * 2;
            else if (timeRatio < 0.5f)
                timeRatio = timeRatio + 0.25f;
            else 
                timeRatio = timeRatio / 2 + 0.5f;
            resultSlider.value = Mathf.Min(resultSlider.value + Time.deltaTime, 1 - timeRatio);
            progressFill.color = GetColorForTimeRatio(resultSlider.value);
        }

        #endregion

        #region UI Update

        private Color GetColorForTimeRatio(float timeRatio)
        {
            if (timeRatio < 0.25f)
            {
                return perfectColor;
            }
            else if (timeRatio < 0.5f)
            {
                return superColor;
            }
            else if (timeRatio < 0.75f)
            {
                return excellentColor;
            }
            else if (timeRatio < 1f)
            {
                return onTimeColor;
            }
            else
            {
                return failColor;
            }
        }

        #endregion
    }
}