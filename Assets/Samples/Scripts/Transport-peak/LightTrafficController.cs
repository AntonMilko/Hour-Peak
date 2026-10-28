using UnityEngine;

namespace HourPeak.Transport
{
    /// <summary>
    /// Главный контроллер транспортных светофоров.
    /// Управляет переключением сигналов для нескольких направлений.
    /// Связан с пешеходными светофорами для синхронизации.
    /// </summary>
    public class LightTrafficController : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("GameObject красного света")]
        public GameObject redLight;
        [Tooltip("GameObject жёлтого света")]
        public GameObject yellowLight;
        [Tooltip("GameObject зелёного света")]
        public GameObject greenLight;

        [Header("Timing (секунды)")]
        [Tooltip("Длительность зелёного")]
        public float greenDuration = 10f;
        [Tooltip("Длительность жёлтого")]
        public float yellowDuration = 3f;
        [Tooltip("Длительность красного")]
        public float redDuration = 5f;
       
        [Header("References")]
        [Tooltip("GameObject красного пешехода (нельзя идти)")]
        public GameObject redPedLight;
        [Tooltip("GameObject зелёного пешехода (можно идти)")]
        public GameObject greenPedLight;

        #endregion

        #region Private State

        private TrafficSignalType _currentSignal;
        private float _timer;
        private float _totalDuration;

        #endregion

        #region Public Methods

        /// <summary>
        /// Тип транспортного сигнала.
        /// </summary>
        public enum TrafficSignalType
        {
            Red,
            Yellow,
            Green,
            pedRedSignal,
            pedGreenSignal
        }

        public bool IsGreen() => _currentSignal == TrafficSignalType.Green;

        public void SetCarSignal(TrafficSignalType carSignal)
        {
            if (_currentSignal == carSignal) return;

            redLight.SetActive(carSignal == TrafficSignalType.Red);
            yellowLight.SetActive(carSignal == TrafficSignalType.Yellow);
            greenLight.SetActive(carSignal == TrafficSignalType.Green);
        }

        public void SetPedestrianSignal(TrafficSignalType pedestrianSignal)
        {
            if (_currentSignal == pedestrianSignal) return;

            greenPedLight.SetActive(pedestrianSignal == TrafficSignalType.pedGreenSignal);
            redPedLight.SetActive(pedestrianSignal == TrafficSignalType.pedRedSignal);
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _totalDuration = redDuration + 2 * yellowDuration + greenDuration;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if(_timer % _totalDuration < greenDuration)
            {
                SetCarSignal(TrafficSignalType.Green);
                SetPedestrianSignal(TrafficSignalType.pedRedSignal);
            }
            else if (_timer % _totalDuration < greenDuration + yellowDuration || _timer % _totalDuration > greenDuration + yellowDuration + redDuration)
            {
                SetCarSignal(TrafficSignalType.Yellow);
                SetPedestrianSignal(TrafficSignalType.pedRedSignal);
            }
            else
            {
                SetCarSignal(TrafficSignalType.Red);
                SetPedestrianSignal(TrafficSignalType.pedGreenSignal);
            }
        }

        #endregion
    }
}