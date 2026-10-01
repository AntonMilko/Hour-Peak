using UnityEngine;
using UnityEngine.UI;
using HourPeak.Settings;
using HourPeak.Addition;
using System.Collections;
using HourPeak.Samples.Runtime.StarDisappearTimings;

namespace HourPeak.Samples.Runtime.StarDisappearTimings
{
    #region Algorithm

    [System.Serializable]
    public struct StarDisappearTimings
    {
        [Tooltip("Все 3 звезды жёлтые (0-Xс)")]
        public float allYellowTime;
        
        [Tooltip("Звезда 3 исчезает (Xс)")]
        public float star3DisappearTime;
        
        [Tooltip("Звезда 2 исчезает (Xс)")]
        public float star2DisappearTime;
        
        [Tooltip("Звезда 1 начинает мигать (Xс)")]
        public float star1BlinkTime;
        
        [Tooltip("Все 3 звезды чёрные (Xс+)")]
        public float allBlackTime;
    }

    #endregion

    public class TheDisappearanceOfStars : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("Объект главной звезды")]
        [SerializeField] private GameObject starObject;

        [Tooltip("CanvasGroup для плавного исчезновения")]
        [SerializeField] private CanvasGroup starCanvasGroup;

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
        [SerializeField] private StarDisappearTimings beginnerTimings = new StarDisappearTimings
        {
            allYellowTime = 0f,
            star3DisappearTime = 30f,
            star2DisappearTime = 60f,
            star1BlinkTime = 120f,
            allBlackTime = 240f
        };

        [Header("Timing - Professional (секунды)")]
        [SerializeField] private StarDisappearTimings professionalTimings = new StarDisappearTimings
        {
            allYellowTime = 0f,
            star3DisappearTime = 15f,
            star2DisappearTime = 30f,
            star1BlinkTime = 60f,
            allBlackTime = 120f
        };

        [Header("Timing - Extremal (секунды)")]
        [SerializeField] private StarDisappearTimings extremalTimings = new StarDisappearTimings
        {
            allYellowTime = 0f,
            star3DisappearTime = 7.5f,
            star2DisappearTime = 15f,
            star1BlinkTime = 30f,
            allBlackTime = 60f
        };

        [Header("Settings")]
        [Tooltip("Автоматически запускать отсчёт при старте")]
        [SerializeField] private bool autoStart = true;

        #endregion

        #region Private Methods

        private float allYellowTime;
        private float currentTime = 0f;
        private bool _isRunning = false;
        private bool _isBlinking = false;
        private float star3DisappearTime;
        private float star2DisappearTime;
        private float star1BlinkTime;
        private float allBlackTime;
        private StarDisappearTimings starDisappearTimings;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CalculateTimingsByDifficulty(starDisappearTimings);
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

        public void CalculateTimingsByDifficulty(StarDisappearTimings selectedTimings)
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
            star3DisappearTime = selectedTimings.star3DisappearTime;
            star2DisappearTime = selectedTimings.star2DisappearTime;
            star1BlinkTime = selectedTimings.star1BlinkTime;
            allBlackTime = selectedTimings.allBlackTime;

            Debug.Log($"⏱️ Тайминги: Все жёлтые=0-{star3DisappearTime}s | Звезда3={star3DisappearTime}-{star2DisappearTime}s | Звезда2={star2DisappearTime}-{star1BlinkTime}s | Мигание={star1BlinkTime}-{allBlackTime}s | Чёрные={allBlackTime}с+");
        }

        private void InitializeStars()
        {
            SetStarColor(starImage1, yellowColor);
            SetStarColor(starImage2, yellowColor);
            SetStarColor(starImage3, yellowColor);

            if (starObject1 != null) starObject1.SetActive(true);
            if (starObject2 != null) starObject2.SetActive(true);
            if (starObject3 != null) starObject3.SetActive(true);

            _isBlinking = false;
            Debug.Log("🟡 Все звёзды инициализированы жёлтыми");
        }

        public void UpdateStarsState()
        {
            // 🕐 0 - Star3DisappearTime: Все 3 звезды жёлтые
            if (currentTime < star3DisappearTime)
            {
                SetAllStarsYellow();
            }
            // 🕐 Star3DisappearTime - Star2DisappearTime: Звезда 3 чёрная, 1-2 жёлтые
            else if (currentTime < star2DisappearTime)
            {
                SetStarColor(starImage1, yellowColor);
                SetStarColor(starImage2, yellowColor);
                SetStarColor(starImage3, blackColor);
            }
            // 🕐 Star2DisappearTime - Star1BlinkTime: Звезда 1 жёлтая (одна осталась), 2-3 чёрные
            else if (currentTime < star1BlinkTime)
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
            _isBlinking = false;
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
            _isBlinking = false;
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