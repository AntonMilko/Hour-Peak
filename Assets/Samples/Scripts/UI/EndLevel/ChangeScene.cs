using UnityEngine;
using UnityEngine.SceneManagement;

namespace HourPeak.Samples.Runtime
{
    public class ChangeScene : MonoBehaviour
    {
        #region Private State

        [SerializeField] private string sceneName;

        #endregion

        #region Public Methods

        public void OnClick()
        {
            SceneManager.LoadScene(sceneName);
        }

        #endregion

    }
}