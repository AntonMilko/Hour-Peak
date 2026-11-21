using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;

namespace UI.StartMenu
{
    public class DateTime : MonoBehaviour
    {
        #region Configuration

        [Header("UI Settings")]
        [SerializeField] private TextMeshProUGUI realTimeText;

        [Header("Format Settings")]
        [SerializeField] private string timeFormat = "dd.MM.yyyy HH:mm:ss";

        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "rush_hour_progress.json";

        #endregion

        #region Fields

        // Internal state
        private bool _isRealTimeFound = false;
        private GameProgress _currentProgress;

        #endregion

        #region DateTime Structure

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

        #endregion

        #region Unity Lifecycle

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

        #endregion

        #region Public Methods

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

        #endregion

        #region Private Methods

        private void UpdateCurrentTime()
        {
            if (!_isRealTimeFound) return;
            System.DateTime now = System.DateTime.Now;
            realTimeText.text = now.ToString(timeFormat);
        }

        private void ClearDisplay()
        {
            if (!_isRealTimeFound) return;
            realTimeText.text = "";
        }

        #endregion

        #region Structure of Saving

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

        public void UpdateLastLevelIndex(int index)
        {
            if (_currentProgress == null) _currentProgress = new GameProgress();
            _currentProgress.lastLevelIndex = index;
            SaveProgress();
        }

        public int GetContinueLevelIndex()
        {
            if (_currentProgress == null) LoadProgress();
            return _currentProgress != null ? _currentProgress.lastLevelIndex : 0;
        }

        #endregion

        #region Progress

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

        #endregion
    }
}