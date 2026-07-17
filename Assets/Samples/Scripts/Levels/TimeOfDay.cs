using System;
using UnityEngine;

namespace HourPeak.Levels
{
    /// <summary>
    /// Settings for a specific time of day variant (morning/evening).
    /// Contains all configuration including BackgroundImage.
    /// </summary>
    [Serializable]
    public class TimeOfDaySettings
    {
        [Tooltip("Display name")]
        public string DisplayName;

        [Tooltip("Description")]
        public string Description;

        [Tooltip("Time range (e.g., '7:00 - 9:00')")]
        public string TimeRange;

        [Tooltip("Traffic density multiplier")]
        public float TrafficMultiplier;

        [Tooltip("Crowd density multiplier")]
        public float CrowdMultiplier;

        [Tooltip("Background image for this time of day")]
        public Sprite BackgroundImage;

        [Tooltip("Base bus count")]
        public int BaseBusCount;

        [Tooltip("Base car count")]
        public int BaseCarCount;

        [Tooltip("Stars reward")]
        public int StarsReward;

        [Tooltip("XP reward")]
        public int XPReward;

        public TimeOfDaySettings()
        {
            DisplayName = "Unknown";
            Description = "";
            TimeRange = "";
            TrafficMultiplier = 1.0f;
            CrowdMultiplier = 1.0f;
            BackgroundImage = null;
            BaseBusCount = 0;
            BaseCarCount = 0;
            StarsReward = 1;
            XPReward = 100;
        }

        public TimeOfDaySettings(string name, string timeRange, float trafficMult, float crowdMult, 
            int buses, int cars, int stars, int xp, Sprite bgImage)
        {
            DisplayName = name;
            Description = "";
            TimeRange = timeRange;
            TrafficMultiplier = trafficMult;
            CrowdMultiplier = crowdMult;
            BackgroundImage = bgImage;
            BaseBusCount = buses;
            BaseCarCount = cars;
            StarsReward = stars;
            XPReward = xp;
        }
    }

    /// <summary>
    /// Represents time of day for level variations (morning/evening rush hours).
    /// </summary>
    public enum TimeOfDay
    {
        /// <summary>
        /// Morning rush hour (7:00 - 9:00)
        /// </summary>
        Morning = 0,

        /// <summary>
        /// Evening rush hour (17:00 - 19:00)
        /// </summary>
        Evening = 1
    }

    /// <summary>
    /// Extension methods for TimeOfDay enum.
    /// </summary>
    public static class TimeOfDayExtensions
    {
        /// <summary>
        /// Gets settings for the time of day.
        /// </summary>
        public static TimeOfDaySettings GetSettings(this TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => new TimeOfDaySettings(
                    "Morning Rush", 
                    "7:00 - 9:00", 
                    1.0f, 1.0f, 
                    5, 15, 
                    1, 100, 
                    null),
                
                TimeOfDay.Evening => new TimeOfDaySettings(
                    "Evening Rush", 
                    "17:00 - 19:00", 
                    1.3f, 1.5f, 
                    7, 20, 
                    1, 100, 
                    null),
                
                _ => new TimeOfDaySettings()
            };
        }

        /// <summary>
        /// Gets display name for the time of day.
        /// </summary>
        public static string GetDisplayName(this TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => "Morning Rush",
                TimeOfDay.Evening => "Evening Rush",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets descriptive text for the time of day.
        /// </summary>
        public static string GetDescription(this TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => "7:00 - 9:00 | Commuters heading to work",
                TimeOfDay.Evening => "17:00 - 19:00 | Commuters heading home",
                _ => ""
            };
        }

        /// <summary>
        /// Gets traffic density multiplier for the time of day.
        /// </summary>
        public static float GetTrafficMultiplier(this TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => 1.0f,
                TimeOfDay.Evening => 1.3f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Gets crowd density multiplier for the time of day.
        /// </summary>
        public static float GetCrowdMultiplier(this TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => 1.0f,
                TimeOfDay.Evening => 1.5f,
                _ => 1.0f
            };
        }
    }
}
