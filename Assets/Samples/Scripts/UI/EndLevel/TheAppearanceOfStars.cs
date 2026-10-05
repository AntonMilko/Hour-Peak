using UnityEngine;
using UnityEngine.UI;
using HourPeak.Settings;
using HourPeak.Addition;
using System.Collections;
using HourPeak.Samples.Runtime.StarAppearTimings;

namespace HourPeak.Samples.Runtime.StarAppearTimings
{
    #region Algorithm

    [System.Serializable]
    public struct StarAppearTimings
    {
        [Tooltip("Все 3 звезды чёрные (Xс+)")]
        public float allBlackTime;

        [Tooltip("Звезда 1 появляется вовремя (Xс)")]
        public float star1AppearOnTime;
        
        [Tooltip("Звезда 1 появляется (Xс)")]
        public float star1AppearTime;
        
        [Tooltip("Звезда 2 появляется (Xс)")]
        public float star2AppearTime;
        
        [Tooltip("Все 3 звезды жёлтые (Xс-0)")]
        public float allYellowTime;
    }

    #endregion

    public class TheAppearanceOfStars : MonoBehaviour
    {
        #region Fields

        [Header("Star Objects")]
        [Tooltip("Звезда 1 - Слева")]
        [SerializeField] private GameObject starObject1;

        [Tooltip("Звезда 2 - По центру")]
        [SerializeField] private GameObject starObject2;

        [Tooltip("Звезда 3 - Справа")]
        [SerializeField] private GameObject starObject3;

        [Header("Star Visuals")]
        [Tooltip("Image компоненты для смены цвета (жёлтый/чёрный)")]
        [SerializeField] private Image starImage1;
        [SerializeField] private Image starImage2;
        [SerializeField] private Image starImage3;

        [Tooltip("Жёлтый цвет (активная звезда)")]
        [SerializeField] private Color yellowColor = Color.yellow;

        [Tooltip("Чёрный цвет (неактивная звезда)")]
        [SerializeField] private Color blackColor = Color.black;

        [Header("Difficulty Manager")]
        [Tooltip("Менеджер сложности")]
        [SerializeField] private DifficultyManager difficultyManager;

        [Header("Timing - Beginner (секунды)")]
        [SerializeField] private StarAppearTimings beginnerTimings = new StarAppearTimings
        {
            allBlackTime = 240f,
            star1AppearOnTime = 120f,
            star1AppearTime = 60f,
            star2AppearTime = 30f,
            allYellowTime = 0f
        };

        [Header("Timing - Professional (секунды)")]
        [SerializeField] private StarAppearTimings professionalTimings = new StarAppearTimings
        {
            allBlackTime = 120f,
            star1AppearOnTime = 60f,
            star1AppearTime = 30f,
            star2AppearTime = 15f,
            allYellowTime = 0f
        };

        [Header("Timing - Extremal (секунды)")]
        [SerializeField] private StarAppearTimings extremalTimings = new StarAppearTimings
        {
            allBlackTime = 60f,
            star1AppearOnTime = 30f,
            star1AppearTime = 15f,
            star2AppearTime = 7.5f,
            allYellowTime = 0f
        };

        [Header("Settings")]
        [Tooltip("Автоматически запускать отсчёт при старте")]
        [SerializeField] private bool autoStart = true;

        #endregion

        #region Private Methods

        private float currentTime = 0f;
        private bool _onTime = false;
        private float allBlackTime;
        private float star1AppearOnTime;
        private float star1AppearTime;
        private float star2AppearTime;
        private float allYellowTime;
        private StarAppearTimings starAppearTimings;

        #endregion

        #region Unity Lifecycle


        private void Awake()
        {
            CalculateTimingsByDifficulty(ref starAppearTimings);
            InitializeStars();
            SetFinalResultTime(0f);

            if (autoStart)
            {
                _onTime = true;
            }
        }

        private void Update()
        {
            if (!_onTime) return;

            currentTime += Time.deltaTime;
            UpdateStarsState();
        }

        #endregion

        #region Public Methods

        public void CalculateTimingsByDifficulty(ref StarAppearTimings selectedTimings)
        {
            bool currentDifficulty = true;
            bool isDifficultySet = false;

            if (difficultyManager != null)
            {
                currentDifficulty = difficultyManager.CurrentDifficulty;
                isDifficultySet = difficultyManager.IsDifficultySet;
            }
            else if (PlayerPrefs.HasKey("DiffLevel"))
            {
                currentDifficulty = PlayerPrefs.GetInt("DiffLevel") == 0;
                isDifficultySet = PlayerPrefs.GetInt("DiffIsSet") == 1;
            }

            if (!isDifficultySet)
            {
                if (!currentDifficulty)
                {
                    selectedTimings = beginnerTimings;  
                }
                else if (!currentDifficulty)
                {
                    selectedTimings = professionalTimings; 
                }
                else
                {
                    selectedTimings = extremalTimings;
                }
            }
            else
            {
                switch (PlayerPrefs.GetInt("Difficulty"))
                {
                    case 0: selectedTimings = beginnerTimings; break;
                    case 1: selectedTimings = professionalTimings; break;
                    case 2: selectedTimings = extremalTimings; break;
                };
            }

            allBlackTime = selectedTimings.allBlackTime;
            star1AppearOnTime = selectedTimings.star1AppearOnTime;
            star1AppearTime = selectedTimings.star1AppearTime;    
            star2AppearTime = selectedTimings.star2AppearTime;
            allYellowTime = selectedTimings.allYellowTime;

            Debug.Log($"⏱️ Тайминги: Чёрные={allBlackTime}с+ | Звезда1={allBlackTime}-{star1AppearOnTime}s | Звезда1={star1AppearOnTime}-{star1AppearTime}s | Звезда2={star1AppearTime}-{star2AppearTime}s | {star2AppearTime}-Все жёлтые=0s");
        }

        private void InitializeStars()
        {
            SetStarColor(starImage1, yellowColor);
            SetStarColor(starImage2, yellowColor);
            SetStarColor(starImage3, yellowColor);

            if (starObject1 != null) starObject1.SetActive(true);
            if (starObject2 != null) starObject2.SetActive(true);
            if (starObject3 != null) starObject3.SetActive(true);

            _onTime = false;
            Debug.Log("🟡 Все звёзды инициализированы жёлтыми");
        }

        public void SetFinalResultTime(float time)
        {
            currentTime = time;
            UpdateStarsState();
        }

        public void UpdateStarsState()
        {
            // 🕐 AllBlackTime+: Все 3 звезды чёрные
            if (currentTime >= allBlackTime)
            {
                SetAllStarsBlack();
                _onTime = false;
                Debug.Log("⚫ Все звёзды стали чёрными - отсчёт завершён");
            }
            // 🕐 AllBlackTime - Star1AppearOnTime: Звезда 1 появляется во время
            else if (currentTime >= star1AppearOnTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, blackColor);
                SetStarColor(starImage3, blackColor);
                _onTime = true;
            }
            // 🕐 Star1AppearOnTime - Star1AppearTime: Звезда 1 появляется
            else if (currentTime >= star1AppearTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, blackColor);
                SetStarColor(starImage3, blackColor);
            }
            // 🕐 Star1AppearTime - Star2AppearTime: Звезда 2 появляется
            else if (currentTime >= star2AppearTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, yellowColor);
                SetStarColor(starImage3, blackColor);
            }
            // 🕐 Star2AppearTime - 0: Все 3 звезды жёлтые
            else
            {
                SetAllStarsYellow();
            }
        }

        public void SetAllStarsYellow()
        {
            SetStarColor(starImage1, yellowColor);
            SetStarColor(starImage2, yellowColor);
            SetStarColor(starImage3, yellowColor);
        }

        public void SetAllStarsBlack()
        {
            SetStarColor(starImage1, blackColor);
            SetStarColor(starImage2, blackColor);
            SetStarColor(starImage3, blackColor);
            _onTime = false;
        }

        public void SetStarColor(Image starImage, Color color)
        {
            if (starImage != null)
            {
                starImage.color = color;
            }
        }

        public IEnumerator onTime()
        {
            if (starImage1 != null)
            {
                starImage1.enabled = !starImage1.enabled;
            }
            yield return new WaitForSeconds(0.5f);
        }

        public float GetCurrentTime() => currentTime;
        public float GetTotalTime() => allBlackTime;

        #endregion

        #region Data Structures

        internal class DifficultyManager
        {
            internal bool CurrentDifficulty;
            internal bool IsDifficultySet;
        }

        #endregion
    }
}
