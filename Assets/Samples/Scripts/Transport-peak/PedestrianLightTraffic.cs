using UnityEngine;

namespace HourPeak.Transport
{
    public class PedestrianLightTraffic : MonoBehaviour
    {
        #region Fields        

        [Header("References")]
        [Tooltip("GameObject красного пешехода (нельзя идти)")]
        public GameObject redPedLight;
        [Tooltip("GameObject зелёного пешехода (можно идти)")]
        public GameObject greenPedLight;

        [Header("Timing")]
        [Tooltip("Длительность зелёного пешехода (сек)")]
        public float greenPedDuration = 5f;
        [Tooltip("Длительность красного пешехода (сек)")]
        public float redPedDuration = 5f;

        #endregion

        #region Private State

        private LightTrafficController _currentSignal;
        private float _timer;
        private float _totalDuration;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _totalDuration = greenPedDuration + redPedDuration;
        }

        private void Update()
        {            
            _timer += Time.deltaTime;
            if(_timer % _totalDuration < redPedDuration)
            {
                SetPedestrianSignal(LightTrafficController.PedRed);
            }
            else if (_timer % _totalDuration < redPedDuration || _timer % _totalDuration > redPedDuration + greenPedDuration)
            {
                SetPedestrianSignal(LightTrafficController.PedRed);
            }
            else
            {
                SetPedestrianSignal(LightTrafficController.PedGreen);
            }
        }

        #endregion

        #region Public Methods

        public enum LightTrafficController
        {
            PedRed,
            PedGreen,
        }

        public void SetPedestrianSignal(LightTrafficController pedSignal)
        {
            if (_currentSignal == pedSignal) return;

            _currentSignal = pedSignal;

            redPedLight.SetActive(pedSignal == LightTrafficController.PedRed);
            greenPedLight.SetActive(pedSignal == LightTrafficController.PedGreen);
        }

        #endregion
    }
}