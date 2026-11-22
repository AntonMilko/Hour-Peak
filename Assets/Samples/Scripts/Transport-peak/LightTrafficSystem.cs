using UnityEngine;

namespace HourPeak.Transport
{
    public class LightTrafficSystem : MonoBehaviour
    {
        #region Fields        

        [Header("References of Pedestrian Light Traffic")]
        [Tooltip("GameObject красного пешехода (нельзя идти)")]
        [SerializeField] private GameObject redPedLight;
        [Tooltip("GameObject зелёного пешехода (можно идти)")]
        [SerializeField] private GameObject greenPedLight;

        [Header("Timing")]
        [Tooltip("Длительность зелёного пешехода (сек)")]
        [SerializeField] private float greenPedDuration = 5f;
        [Tooltip("Длительность красного пешехода (сек)")]
        [SerializeField] private float redPedDuration = 10f;

        [Header("References of Light Traffic")]
        [Tooltip("GameObject красного света (нельзя ехать)")]
        [SerializeField] private GameObject redLight;
        [Tooltip("GameObject жёлтого света (будь внимательным)")]
        [SerializeField] private GameObject yellowLight;
        [Tooltip("GameObject зелёного света (можно ехать)")]
        [SerializeField] private GameObject greenLight;

        [Header("Timing (секунды)")]
        [Tooltip("Длительность зелёного")]
        [SerializeField] private float greenDuration = 10f;
        [Tooltip("Длительность жёлтого")]
        [SerializeField] private float yellowDuration = 3f;
        [Tooltip("Длительность красного")]
        [SerializeField] private float redDuration = 5f;

        #endregion

        #region Private State

        private TrafficSignalType _currentSignal;
        private float _timer;
        private float _totalDuration;
        internal object lightTrafficSystem;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _totalDuration = greenPedDuration + redPedDuration;
            _totalDuration = redDuration + 2 * yellowDuration + greenDuration;
        }

        private void Update()
        {            
            _timer += Time.deltaTime;
            if(_timer % _totalDuration < greenDuration)
            {
                SetCarSignal(TrafficSignalType.Green);
                SetPedestrianSignal(TrafficSignalType.redPedLight);
            }
            else if (_timer % _totalDuration < greenDuration + yellowDuration || _timer % _totalDuration > greenDuration + yellowDuration + redDuration)
            {
                SetCarSignal(TrafficSignalType.Yellow);
                SetPedestrianSignal(TrafficSignalType.redPedLight);
            }
            else
            {
                SetCarSignal(TrafficSignalType.Red);
                SetPedestrianSignal(TrafficSignalType.greenPedLight);
            }
        }

        #endregion

        #region Public Methods

        public enum TrafficSignalType
        {
            Red,
            Yellow,
            Green,
            redPedLight,
            greenPedLight
        }

        public bool IsGreen() => _currentSignal == TrafficSignalType.Green;
        public bool IsRed() => _currentSignal == TrafficSignalType.Red;

        public void SetCarSignal(TrafficSignalType carSignal)
        {
            if (_currentSignal == carSignal) return;

            _currentSignal = carSignal;

            redLight.SetActive(carSignal == TrafficSignalType.Red);
            yellowLight.SetActive(carSignal == TrafficSignalType.Yellow);
            greenLight.SetActive(carSignal == TrafficSignalType.Green);
        }

        public void SetPedestrianSignal(TrafficSignalType pedestrianSignal)
        {
            if (_currentSignal == pedestrianSignal) return;

            greenPedLight.SetActive(pedestrianSignal == TrafficSignalType.greenPedLight);
            redPedLight.SetActive(pedestrianSignal == TrafficSignalType.redPedLight);
        }

        #endregion
    }
}