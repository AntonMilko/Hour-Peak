using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HourPeak.Transport
{
    /// <summary>
    /// РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ РѕРґРЅРѕРіРѕ РїРµС€РµС…РѕРґРЅРѕРіРѕ СЃРІРµС‚РѕС„РѕСЂР°.
    /// </summary>
    [Serializable]
    public class PedestrianLightConfig
    {
        [Header("References")]
        [Tooltip("GameObject красного пешехода (нельзя идти)")]
        public GameObject redPedLight;
        [Tooltip("GameObject зелёного пешехода (можно идти)")]
        public GameObject greenPedLight;

        [Header("Visual Settings")]
        [Tooltip("Базовый материал (выключенный)")]
        public Material offMaterial;
        [Tooltip("Материал активного зелёного пешехода (идти можно)")]
        public Material greenPedMaterial;
        [Tooltip("Материал активного красного пешехода (идти нельзя)")]
        public Material redPedMaterial;

        [Header("Timing")]
        [Tooltip("Длительность зелёного пешехода (сек)")]
        public float greenPedDuration = 5f;
        [Tooltip("Длительность мигающего зелёного пешехода (сек)")]
        public float greenPedFlashDuration = 2f;
        [Tooltip("Длительность красного пешехода (сек)")]
        public float redPedDuration = 5f;

        [Header("Direction")]
        [Tooltip("Направление, для которого действует этот сигнал")]
        public TrafficDirection pedDirection = TrafficDirection.All;

        public void SetSignal(TrafficSignalType signal, float remainingTime = 0f)
        {
            bool isGreen = signal == TrafficSignalType.Green;
            bool isRed = signal == TrafficSignalType.Red || signal == TrafficSignalType.Yellow;

            Vector3 lookDir = GetDirectionVector();

            if (redPedLight != null)
            {
                redPedLight.SetActive(isRed);
                redPedLight.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            if (greenPedLight != null)
            {
                greenPedLight.SetActive(isGreen);
                greenPedLight.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            ApplyMaterials(signal);
        }

        private Vector3 GetDirectionVector()
        {
            if ((pedDirection & TrafficDirection.North) == TrafficDirection.North) return Vector3.forward;
            if ((pedDirection & TrafficDirection.South) == TrafficDirection.South) return -Vector3.forward;
            if ((pedDirection & TrafficDirection.East) == TrafficDirection.East) return Vector3.right;
            if ((pedDirection & TrafficDirection.West) == TrafficDirection.West) return -Vector3.right;
            return Vector3.forward;
        }

        private void ApplyMaterials(TrafficSignalType signal)
        {
            var renderer = GetRenderer();
            if (renderer == null || renderer.sharedMaterial == null) return;

            Material activeMat = signal switch
            {
                TrafficSignalType.Green => greenPedMaterial,
                TrafficSignalType.Red => redPedMaterial,
                TrafficSignalType.Yellow => redPedMaterial,
                _ => offMaterial
            };

            if (activeMat != null)
                renderer.sharedMaterial = activeMat;
        }

        private Renderer GetRenderer()
        {
            foreach (var go in new[] { redPedLight, greenPedLight })
            {
                if (go != null)
                {
                    var r = go.GetComponent<Renderer>();
                    if (r != null) return r;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// РљРѕРЅС‚СЂРѕР»Р»РµСЂ РїРµС€РµС…РѕРґРЅС‹С… СЃРІРµС‚РѕС„РѕСЂРѕРІ.
    /// РЎРёРЅС…СЂРѕРЅРёР·РёСЂРѕРІР°РЅ СЃ С‚СЂР°РЅСЃРїРѕСЂС‚РЅС‹РјРё: РєРѕРіРґР° РјР°С€РёРЅС‹ РµРґСѓС‚ вЂ” РїРµС€РµС…РѕРґР°Рј РЅРµР»СЊР·СЏ,
    /// Рё РЅР°РѕР±РѕСЂРѕС‚. РЈРїСЂР°РІР»СЏРµС‚СЃСЏ РёР· LightTrafficController.
    /// </summary>
    public class PedestrianLightTraffic : MonoBehaviour
    {
        #region Constants

        private const float DefaultGreenDuration = 5f;
        private const float DefaultYellowDuration = 2f;

        #endregion

        #region Fields

        [Header("Signal Configurations")]
        [Tooltip("РЎРїРёСЃРѕРє РїРµС€РµС…РѕРґРЅС‹С… СЃРёРіРЅР°Р»РѕРІ (РїРѕ РѕРґРЅРѕРјСѓ РЅР° РїРµСЂРµС…РѕРґ)")]
        [SerializeField] private List<PedestrianLightConfig> signalConfigs = new();

        [Header("Settings")]
        [Tooltip("РђРІС‚РѕРјР°С‚РёС‡РµСЃРєРёР№ Р·Р°РїСѓСЃРє РїСЂРё СЃС‚Р°СЂС‚Рµ")]
        [SerializeField] private bool autoStart = true;

        [Header("Debug")]
        [Tooltip("Р›РѕРіРёСЂРѕРІР°С‚СЊ РїРµСЂРµРєР»СЋС‡РµРЅРёСЏ РІ РєРѕРЅСЃРѕР»СЊ")]
        [SerializeField] private bool logSwitches = true;

        #endregion

        #region Private State

        private TrafficSignalType _currentSignal;
        private float _timer;
        private bool _isRunning;
        private Coroutine _coroutine;

        #endregion

        #region Properties

        /// <summary>
        /// РўРµРєСѓС‰РёР№ СЃРёРіРЅР°Р» РїРµС€РµС…РѕРґРѕРІ.
        /// </summary>
        public TrafficSignalType CurrentSignal => _currentSignal;

        /// <summary>
        /// РћСЃС‚Р°Р»РѕСЃСЊ РІСЂРµРјРµРЅРё РґРѕ РїРµСЂРµРєР»СЋС‡РµРЅРёСЏ.
        /// </summary>
        public float TimeRemaining => _isRunning ? _timer : 0f;

        /// <summary>
        /// РљРѕР»РёС‡РµСЃС‚РІРѕ РЅР°СЃС‚СЂРѕРµРЅРЅС‹С… РїРµС€РµС…РѕРґРЅС‹С… СЃРёРіРЅР°Р»РѕРІ.
        /// </summary>
        public int SignalCount => signalConfigs.Count;

        /// <summary>
        /// Р—Р°РїСѓС‰РµРЅ Р»Рё РєРѕРЅС‚СЂРѕР»Р»РµСЂ.
        /// </summary>
        public bool IsRunning => _isRunning;

        #endregion

        #region Events

        /// <summary>
        /// Р’С‹Р·С‹РІР°РµС‚СЃСЏ РїСЂРё СЃРјРµРЅРµ СЃРёРіРЅР°Р»Р°.
        /// </summary>
        public event Action<TrafficSignalType> OnSignalChanged;

        /// <summary>
        /// Р’С‹Р·С‹РІР°РµС‚СЃСЏ РїСЂРё РєР°Р¶РґРѕРј РѕР±РЅРѕРІР»РµРЅРёРё С‚Р°Р№РјРµСЂР°.
        /// </summary>
        public event Action<float> OnTimerUpdate;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (autoStart)
            {
                StartCycle();
            }
        }

        private void OnDestroy()
        {
            StopCycle();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Р—Р°РїСѓСЃРєР°РµС‚ С†РёРєР» РїРµС€РµС…РѕРґРЅС‹С… СЃРІРµС‚РѕС„РѕСЂРѕРІ.
        /// РќР°С‡РёРЅР°РµС‚ СЃ Green (РїРµС€РµС…РѕРґР°Рј РјРѕР¶РЅРѕ РёРґС‚Рё).
        /// </summary>
        public void StartCycle()
        {
            if (_isRunning) return;

            _isRunning = true;
            SetSignal(TrafficSignalType.Green);
            _coroutine = StartCoroutine(SignalCoroutine());
        }

        /// <summary>
        /// РћСЃС‚Р°РЅР°РІР»РёРІР°РµС‚ С†РёРєР».
        /// </summary>
        public void StopCycle()
        {
            _isRunning = false;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        /// <summary>
        /// РЈСЃС‚Р°РЅР°РІР»РёРІР°РµС‚ СѓРєР°Р·Р°РЅРЅС‹Р№ СЃРёРіРЅР°Р» РґР»СЏ РІСЃРµС… РїРµС€РµС…РѕРґРЅС‹С… РїРµСЂРµС…РѕРґРѕРІ.
        /// </summary>
        public void SetSignal(TrafficSignalType signal)
        {
            if (_currentSignal == signal) return;

            _currentSignal = signal;

            foreach (var config in signalConfigs)
            {
                config.SetSignal(signal, _timer);
            }

            OnSignalChanged?.Invoke(signal);

            if (logSwitches)
            {
                Debug.Log($"рџљ¶ Pedestrian signal: {LightTrafficController.GetSignalName(signal)}");
            }
        }

        /// <summary>
        /// РџСЂРёРЅСѓРґРёС‚РµР»СЊРЅРѕ РїРµСЂРµРєР»СЋС‡Р°РµС‚ РЅР° СЃР»РµРґСѓСЋС‰РёР№ СЃРёРіРЅР°Р».
        /// РџРѕСЂСЏРґРѕРє: Green в†’ Yellow в†’ Red в†’ Green
        /// </summary>
        public void ForceNextSignal()
        {
            _timer = 0f;
            TrafficSignalType next = _currentSignal switch
            {
                TrafficSignalType.Green => TrafficSignalType.Yellow,
                TrafficSignalType.Yellow => TrafficSignalType.Red,
                TrafficSignalType.Red => TrafficSignalType.Green,
                _ => TrafficSignalType.Green
            };
            SetSignal(next);
        }

        /// <summary>
        /// Возвращает длительность для сигнала в конкретной конфигурации.
        /// </summary>
        public float GetSignalDuration(PedestrianLightConfig config, TrafficSignalType signal)
        {
            return signal switch
            {
                TrafficSignalType.Green => config.greenPedDuration,
                TrafficSignalType.Red => config.redPedDuration,
                TrafficSignalType.Yellow => config.greenPedFlashDuration,
                _ => config.greenPedDuration
            };
        }

        #endregion

        #region Coroutines

        private IEnumerator SignalCoroutine()
        {
            while (_isRunning)
            {
                TrafficSignalType signal = _currentSignal;

                float duration = signalConfigs.Count > 0
                    ? GetSignalDuration(signalConfigs[0], signal)
                    : DefaultGreenDuration;
                _timer = duration;

                while (_timer > 0f && _isRunning)
                {
                    _timer -= Time.deltaTime;
                    OnTimerUpdate?.Invoke(Mathf.Max(0, _timer));

                    // Обновляем таймер на всех пешеходных светофорах
                    foreach (var config in signalConfigs)
                    {
                        config.SetSignal(signal, _timer);
                    }

                    yield return null;
                }

                ForceNextSignal();
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 500, 280, 90));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"рџљ¶ Pedestrian Light");
            GUILayout.Label($"РЎРёРіРЅР°Р»: {LightTrafficController.GetSignalName(_currentSignal)}");
            GUILayout.Label($"РћСЃС‚Р°Р»РѕСЃСЊ: {_timer:F1}s");
            GUILayout.Label($"Р—Р°РїСѓС‰РµРЅ: {_isRunning}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Start Cycle")]
        private void DebugStart() => StartCycle();

        [ContextMenu("Stop Cycle")]
        private void DebugStop() => StopCycle();

        [ContextMenu("Force Next Signal")]
        private void DebugNext() => ForceNextSignal();
#endif

        #endregion
    }
}
