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
        [Tooltip("Все 3 звезды жёлтые (0-Xс)")]
        public float allYellowTime;
        
        [Tooltip("Звезда 2 появляется (Xс)")]
        public float star2AppearTime;
        
        [Tooltip("Звезда 1 появляется (Xс)")]
        public float star1AppearTime;
        
        [Tooltip("Звезда 1 появляется вовремя (Xс)")]
        public float star1AppearOnTime;
        
        [Tooltip("Все 3 звезды чёрные (Xс+)")]
        public float allBlackTime;
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
            allYellowTime = 0f,
            star2AppearTime = 30f,
            star1AppearTime = 60f,
            star1AppearOnTime = 120f,
            allBlackTime = 240f
        };

        [Header("Timing - Professional (секунды)")]
        [SerializeField] private StarAppearTimings professionalTimings = new StarAppearTimings
        {
            allYellowTime = 0f,
            star2AppearTime = 15f,
            star1AppearTime = 30f,
            star1AppearOnTime = 60f,
            allBlackTime = 120f
        };

        [Header("Timing - Extremal (секунды)")]
        [SerializeField] private StarAppearTimings extremalTimings = new StarAppearTimings
        {
            allYellowTime = 0f,
            star2AppearTime = 7.5f,
            star1AppearTime = 15f,
            star1AppearOnTime = 30f,
            allBlackTime = 60f
        };

        [Header("Settings")]
        [Tooltip("Автоматически запускать отсчёт при старте")]
        [SerializeField] private bool autoStart = true;

        #endregion

        #region Private Methods

        private float currentTime = 0f;
        private bool _isRunning = false;
        private bool _onTime = false;
        private float allYellowTime;
        private float star2AppearTime;
        private float star1AppearTime;
        private float star1AppearOnTime;
        private float allBlackTime;
        private StarAppearTimings starAppearTimings;

        public bool OnTime { get => _onTime; set => _onTime = value; }

        #endregion

        #region Unity Lifecycle


        private void Awake()
        {
            CalculateTimingsByDifficulty(starAppearTimings);
            InitializeStars();

            if (autoStart)
            {
                _isRunning = true;
            }
        }

        private void Update()
        {
            if (!_isRunning) return;

            currentTime += Time.deltaTime;
            UpdateStarsState();
        }

        #endregion

        #region Public Methods

        public void CalculateTimingsByDifficulty(StarAppearTimings selectedTimings)
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
                selectedTimings = currentDifficulty ? beginnerTimings : professionalTimings;
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

            allYellowTime = selectedTimings.allYellowTime;    
            star2AppearTime = selectedTimings.star2AppearTime;
            star1AppearTime = selectedTimings.star1AppearTime;
            star1AppearOnTime = selectedTimings.star1AppearOnTime;
            allBlackTime = selectedTimings.allBlackTime;

            Debug.Log($"⏱️ Тайминги: Все жёлтые=0-{star2AppearTime}s | Звезда3={star2AppearTime}-{star1AppearTime}s | Звезда2={star1AppearTime}-{star1AppearOnTime}s | Мигание={star1AppearOnTime}-{allBlackTime}s | Чёрные={allBlackTime}с+");
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

        public void UpdateStarsState()
        {
            // 🕐 0 - Star2AppearTime: Все 3 звезды жёлтые
            if (currentTime < star2AppearTime)
            {
                SetAllStarsYellow();
            }
            // 🕐 Star2AppearTime - Star1AppearTime: Звезда 3 чёрная, 1-2 жёлтые
            else if (currentTime < star1AppearTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, yellowColor);
                SetStarColor(starImage3, blackColor);
            }
            // 🕐 Star1AppearTime - Star1AppearOnTime: Звезда 1 жёлтая (одна осталась), 2-3 чёрные
            else if (currentTime < star1AppearOnTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, blackColor);
                SetStarColor(starImage3, blackColor);
            }
            // 🕐 AllBlackTime+: Все 3 звезды чёрные
            else
            {
                SetAllStarsBlack();
                _isRunning = false;
                Debug.Log("⚫ Все звёзды стали чёрными - отсчёт завершён");
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
            while (currentTime < allBlackTime && _isRunning)
            {
                if (starImage1 != null)
                {
                    starImage1.enabled = !starImage1.enabled;
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            if (starImage1 != null)
            {
                starImage1.enabled = true;
            }
        }

        public void RestartTimer()
        {
            currentTime = 0f;
            _isRunning = true;
            _onTime = false;
            StopAllCoroutines();
            InitializeStars();
            Debug.Log("🔄 Таймер звёзд перезапущен");
        }

        public void StopTimer()
        {
            _isRunning = false;
            Debug.Log("⏸️ Таймер звёзд остановлен");
        }

        public void ResumeTimer()
        {
            _isRunning = true;
            Debug.Log("▶️ Таймер звёзд продолжен");
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
