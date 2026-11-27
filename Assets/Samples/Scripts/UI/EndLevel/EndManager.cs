using System;
using UnityEngine;

public class EndManager : MonoBehaviour
{    
    #region Fields

    [Header("UI References")]
    [Tooltip("Панель окончания игры.")]
    [SerializeField] private GameObject endMenuPanel;

    [Tooltip("Объект для учёта времени.")]
    [SerializeField] private TimerDisplay timerDisplay;

    [Header("Level Settings.")]
    [Tooltip("Number Level.")]
    [SerializeField] private int levelNumber;

    [Tooltip("Morning or Evening.")]
    [SerializeField] private bool isMorning;

    #endregion

    #region Unity Lifecycle

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            endMenuPanel.SetActive(true);
            timerDisplay.SetIsRunning(false);
            timerDisplay.SetIsFinished(true);

            // save game
            if (isMorning)
            {
                LevelMenuManager._morningStars[levelNumber] = SetStarForElapsedTime();
            }
            else
            {
                LevelMenuManager._eveningStars[levelNumber] = SetStarForElapsedTime();
            }

            SaveManager.SaveGame();
            Debug.Log("Сохраняется ли игра?");
        }
    }

    #endregion

    #region Public Methods

        private int SetStarForElapsedTime()
        {
            if (TimerDisplay.time < TimerDisplay.totalTime/8)
            {
                return 3;
            }
            else if (TimerDisplay.time < TimerDisplay.totalTime/4)
            {
                return 2;
            }
            else if (TimerDisplay.time < TimerDisplay.totalTime)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion
}