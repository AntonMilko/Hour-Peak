using UnityEngine;
using UnityEngine.SceneManagement;

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
            gameObject.SetActive(!timer.isTimeUp());
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