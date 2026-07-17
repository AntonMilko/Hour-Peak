using UnityEngine;
using TMPro;
using System; // Required for DateTime
using System.IO; // Required for saving files
using System.Collections.Generic;

namespace UI.StartMenu
{
    /// <summary>
    /// Displays the static real-world date and time of the currently loaded save file.
    /// The text remains empty until a save file is loaded.
    /// Records level completion times to the save file.
    /// </summary>
    public class DateTime : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField]
        private TextMeshProUGUI realTimeText;

        [Header("Format Settings")]
        [SerializeField]
        private string timeFormat = "dd.MM.yyyy HH:mm";

        [Header("Save Settings")]
        [SerializeField]
        private string saveFileName = "rush_hour_progress.json";

        // Internal state
        private bool _isRealTimeFound = false;
        private GameProgress _currentProgress;

        // Data structure for saving
        [System.Serializable]
        public class LevelCompletion
        {
            public string levelName;
            public string completionTime; // Real-world date/time string
        }

        [System.Serializable]
        public class GameProgress
        {
            public List<LevelCompletion> completedLevels;
            public int lastLevelIndex;
            public string saveTimeStamp; // When this save was created/last loaded
        }

        private void Start()
        {
            // 1. Setup Real Time Display
            if (realTimeText == null)
            {
                realTimeText = GetComponent<TextMeshProUGUI>();
            }

            if (realTimeText != null)
            {
                _isRealTimeFound = true;
                // Requirement: At the very beginning, folders don't display the date and time.
                ClearDisplay();
            }
            else
            {
                _isRealTimeFound = false;
            }

            // Initialize progress structure but don't load yet
            _currentProgress = new GameProgress();
            _currentProgress.completedLevels = new List<LevelCompletion>();
        }

        /// <summary>
        /// Call this when the player clicks a file/folder to load.
        /// This loads the progress and displays the time.
        /// </summary>
        public void LoadGameFile()
        {
            LoadProgress();

            if (_isRealTimeFound && _currentProgress != null)
            {
                // Requirement: When you click on a folder, date and time are displayed.
                // We display the 'saveTimeStamp' from the file.
                if (!string.IsNullOrEmpty(_currentProgress.saveTimeStamp))
                {
                    realTimeText.text = _currentProgress.saveTimeStamp;
                }
                else
                {
                    // If it's a new save with no timestamp yet, show current time
                    UpdateCurrentTime();
                }
            }
        }

        /// <summary>
        /// Updates the text to show the real-world time right now.
        /// </summary>
        private void UpdateCurrentTime()
        {
            if (!_isRealTimeFound) return;
            System.DateTime now = System.DateTime.Now;
            realTimeText.text = now.ToString(timeFormat);
        }

        /// <summary>
        /// Clears the text display (used at start).
        /// </summary>
        private void ClearDisplay()
        {
            if (!_isRealTimeFound) return;
            realTimeText.text = "";
        }

        /// <summary>
        /// Call this method when a level is finished to record the time.
        /// </summary>
        public void RecordLevelCompletion(string levelName)
        {
            if (_currentProgress == null) _currentProgress = new GameProgress();

            // Create new completion record
            LevelCompletion newRecord = new LevelCompletion
            {
                levelName = levelName,
                completionTime = System.DateTime.Now.ToString(timeFormat)
            };

            // Add to list
            _currentProgress.completedLevels.Add(newRecord);

            // Update the main save timestamp to "now" so it reflects the latest activity
            _currentProgress.saveTimeStamp = System.DateTime.Now.ToString(timeFormat);

            // Update UI to show this new time
            if (_isRealTimeFound)
            {
                realTimeText.text = _currentProgress.saveTimeStamp;
            }

            // Save to disk
            SaveProgress();

            Debug.Log($"[DateTime] Level '{levelName}' completed at {newRecord.completionTime}");
        }

        /// <summary>
        /// Updates the last played level index so the player can continue from there.
        /// </summary>
        public void UpdateLastLevelIndex(int index)
        {
            if (_currentProgress == null) _currentProgress = new GameProgress();
            _currentProgress.lastLevelIndex = index;
            SaveProgress();
        }

        /// <summary>
        /// Returns the index of the level the player should continue from.
        /// </summary>
        public int GetContinueLevelIndex()
        {
            if (_currentProgress == null) LoadProgress();
            return _currentProgress != null ? _currentProgress.lastLevelIndex : 0;
        }

        private void SaveProgress()
        {
            string json = JsonUtility.ToJson(_currentProgress, true);
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            File.WriteAllText(path, json);
        }

        private void LoadProgress()
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _currentProgress = JsonUtility.FromJson<GameProgress>(json);

                    // Ensure lists are initialized
                    if (_currentProgress.completedLevels == null)
                    {
                        _currentProgress.completedLevels = new List<LevelCompletion>();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[DateTime] Failed to load save file: {e.Message}. Starting new progress.");
                    _currentProgress = new GameProgress();
                    _currentProgress.completedLevels = new List<LevelCompletion>();
                    _currentProgress.lastLevelIndex = 0;
                }
            }
            else
            {
                // New game setup
                _currentProgress = new GameProgress();
                _currentProgress.completedLevels = new List<LevelCompletion>();
                _currentProgress.lastLevelIndex = 0;
            }
        }
    }
}