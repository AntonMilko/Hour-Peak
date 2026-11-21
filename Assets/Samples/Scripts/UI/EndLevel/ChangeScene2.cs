using UnityEngine;
using UnityEngine.SceneManagement;

namespace HourPeak.Samples.Runtime
{
    public class ChangeScene2 : MonoBehaviour
    {
        #region Private State

        [SerializeField] private string sceneName;
        [SerializeField] private TimerDisplay timer;

        #endregion

        #region Unity Lifecycle
        
        void Start()
        {
            gameObject.SetActive(timer.isTimeUp());
        }

        #endregion

        #region Public Methods

        public void OnClick()
        {
            SceneManager.LoadScene(sceneName);
        }

        #endregion
    }
}
