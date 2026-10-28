using UnityEngine;

namespace HourPeak.Transport
{
    /// <summary>
    /// РљРѕРЅС‚СЂРѕР»Р»РµСЂ РїРµС€РµС…РѕРґРЅС‹С… СЃРІРµС‚РѕС„РѕСЂРѕРІ.
    /// РЎРёРЅС…СЂРѕРЅРёР·РёСЂРѕРІР°РЅ СЃ С‚СЂР°РЅСЃРїРѕСЂС‚РЅС‹РјРё: РєРѕРіРґР° РјР°С€РёРЅС‹ РµРґСѓС‚ вЂ” РїРµС€РµС…РѕРґР°Рј РЅРµР»СЊР·СЏ,
    /// Рё РЅР°РѕР±РѕСЂРѕС‚. РЈРїСЂР°РІР»СЏРµС‚СЃСЏ РёР· LightTrafficController.
    /// </summary>
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

        /// <summary>
        /// Тип транспортного сигнала.
        /// </summary>
        public enum LightTrafficController
        {
            PedRed,
            PedGreen,
        }

        /// <summary>
        /// РЈСЃС‚Р°РЅР°РІР»РёРІР°РµС‚ СѓРєР°Р·Р°РЅРЅС‹Р№ СЃРёРіРЅР°Р» РґР»СЏ РІСЃРµС… РїРµС€РµС…РѕРґРЅС‹С… РїРµСЂРµС…РѕРґРѕРІ.
        /// </summary>
        public void SetPedestrianSignal(LightTrafficController pedSignal)
        {
            if (_currentSignal == pedSignal) return;

            redPedLight.SetActive(pedSignal == LightTrafficController.PedRed);
            greenPedLight.SetActive(pedSignal == LightTrafficController.PedGreen);
        }

        #endregion
    }
}