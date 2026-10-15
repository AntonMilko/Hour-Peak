using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    public class NextLevel : MonoBehaviour
    {
        #region Private State

        [SerializeField] private string nextLevelSceneName;
        [SerializeField] private TimerDisplay timer;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            if (timer.isTimeUp())
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Methods

        public void OnClick()
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }

        #endregion
    }
}