using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HourPeak.Settings;

namespace HourPeak.Samples.Runtime
{
    public class NextLevel2 : MonoBehaviour
    {
        #region Private State

        [SerializeField] private string certificateSceneName;
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
            SceneManager.LoadScene(certificateSceneName);
        }

        #endregion
    }
}