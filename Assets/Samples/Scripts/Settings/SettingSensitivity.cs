using UnityEngine;
using UnityEngine.UI;
using System;

namespace HourPeak.Settings
{
    /// <summary>
    /// Типы чувствительности для разных действий.
    /// </summary>
    public enum SensitivityType
    {
        /// <summary>
        /// Чувствительность камеры (вращение).
        /// </summary>
        Camera = 0,

        /// <summary>
        /// Чувствительность движения (перемещение).
        /// </summary>
        Movement = 1,

        /// <summary>
        /// Чувствительность зума (приближение).
        /// </summary>
        Zoom = 2
    }

    /// <summary>
    /// Настройки чувствительности для мобильных устройств.
    /// Поддержка тач-управления и сохранение настроек.
    /// </summary>
    public class SettingSensitivity : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Минимальное значение чувствительности.
        /// </summary>
        private const float MIN_SENSITIVITY = 0.1f;

        /// <summary>
        /// Максимальное значение чувствительности.
        /// </summary>
        private const float MAX_SENSITIVITY = 10.0f;

        /// <summary>
        /// Значение по умолчанию.
        /// </summary>
        private const float DEFAULT_SENSITIVITY = 1.0f;

        /// <summary>
        /// Шаг изменения чувствительности.
        /// </summary>
        private const float SENSITIVITY_STEP = 0.1f;

        /// <summary>
        /// Ключ для сохранения в PlayerPrefs.
        /// </summary>
        private const string PREFS_KEY_CAMERA = "Sensitivity_Camera";
        private const string PREFS_KEY_MOVEMENT = "Sensitivity_Movement";
        private const string PREFS_KEY_ZOOM = "Sensitivity_Zoom";

        #endregion

        #region Configuration

        [Header("UI References")]
        [SerializeField] private Slider cameraSensitivitySlider;
        [SerializeField] private Slider movementSensitivitySlider;
        [SerializeField] private Slider zoomSensitivitySlider;
        
        #endregion

        #region Properties

        /// <summary>
        /// Чувствительность камеры (вращение).
        /// Диапазон: 0.1 - 10.0
        /// </summary>
        public float CameraSensitivity { get; private set; }

        /// <summary>
        /// Чувствительность движения (перемещение).
        /// Диапазон: 0.1 - 10.0
        /// </summary>
        public float MovementSensitivity { get; private set; }

        /// <summary>
        /// Чувствительность зума (приближение).
        /// Диапазон: 0.1 - 10.0
        /// </summary>
        public float ZoomSensitivity { get; private set; }

        /// <summary>
        /// Инверсия вертикальной оси камеры.
        /// </summary>
        public bool InvertYAxis { get; private set; }

        /// <summary>
        /// Вибрация при взаимодействии (мобильные устройства).
        /// </summary>
        public bool EnableHapticFeedback { get; private set; }

        #endregion

        #region Singleton

        private static SettingSensitivity _instance;

        /// <summary>
        /// Глобальный экземпляр.
        /// </summary>
        public static SettingSensitivity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SettingSensitivity>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SettingSensitivity");
                        _instance = go.AddComponent<SettingSensitivity>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            InitializeUIElements();
            LoadSettings();
            UpdateUI();
        }

        private void OnDestroy()
        {
            RemoveUIListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует UI элементы.
        /// </summary>
        private void InitializeUIElements()
        {
            if (cameraSensitivitySlider != null)
                cameraSensitivitySlider.onValueChanged.AddListener(OnCameraSensitivityChanged);

            if (movementSensitivitySlider != null)
                movementSensitivitySlider.onValueChanged.AddListener(OnMovementSensitivityChanged);

            if (zoomSensitivitySlider != null)
                zoomSensitivitySlider.onValueChanged.AddListener(OnZoomSensitivityChanged);
        }

        /// <summary>
        /// Удаляет слушатели UI для предотвращения утечек памяти.
        /// </summary>
        private void RemoveUIListeners()
        {
            if (cameraSensitivitySlider != null)
                cameraSensitivitySlider.onValueChanged.RemoveListener(OnCameraSensitivityChanged);

            if (movementSensitivitySlider != null)
                movementSensitivitySlider.onValueChanged.RemoveListener(OnMovementSensitivityChanged);

            if (zoomSensitivitySlider != null)
                zoomSensitivitySlider.onValueChanged.RemoveListener(OnZoomSensitivityChanged);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Получает чувствительность по типу.
        /// </summary>
        /// <param name="type">Тип чувствительности</param>
        /// <returns>Значение чувствительности</returns>
        public float GetSensitivity(SensitivityType type)
        {
            return type switch
            {
                SensitivityType.Camera => CameraSensitivity,
                SensitivityType.Movement => MovementSensitivity,
                SensitivityType.Zoom => ZoomSensitivity,
                _ => DEFAULT_SENSITIVITY
            };
        }

        /// <summary>
        /// Устанавливает чувствительность по типу.
        /// </summary>
        /// <param name="type">Тип чувствительности</param>
        /// <param name="value">Новое значение</param>
        public void SetSensitivity(SensitivityType type, float value)
        {
            float clampedValue = ClampSensitivity(value);

            switch (type)
            {
                case SensitivityType.Camera:
                    CameraSensitivity = clampedValue;
                    UpdateSlider(cameraSensitivitySlider, clampedValue);
                    break;

                case SensitivityType.Movement:
                    MovementSensitivity = clampedValue;
                    UpdateSlider(movementSensitivitySlider, clampedValue);
                    break;

                case SensitivityType.Zoom:
                    ZoomSensitivity = clampedValue;
                    UpdateSlider(zoomSensitivitySlider, clampedValue);
                    break;
            }

            SaveSettings();
            OnSensitivityChanged?.Invoke(type, clampedValue);
        }

        /// <summary>
        /// Увеличивает чувствительность на шаг.
        /// </summary>
        /// <param name="type">Тип чувствительности</param>
        public void IncreaseSensitivity(SensitivityType type)
        {
            float currentValue = GetSensitivity(type);
            SetSensitivity(type, currentValue + SENSITIVITY_STEP);
            TriggerHapticFeedback();
        }

        /// <summary>
        /// Уменьшает чувствительность на шаг.
        /// </summary>
        /// <param name="type">Тип чувствительности</param>
        public void DecreaseSensitivity(SensitivityType type)
        {
            float currentValue = GetSensitivity(type);
            SetSensitivity(type, currentValue - SENSITIVITY_STEP);
            TriggerHapticFeedback();
        }

        /// <summary>
        /// Сбрасывает все настройки к значениям по умолчанию.
        /// </summary>
        public void ResetToDefaults()
        {
            CameraSensitivity = DEFAULT_SENSITIVITY;
            MovementSensitivity = DEFAULT_SENSITIVITY;
            ZoomSensitivity = DEFAULT_SENSITIVITY;
            InvertYAxis = false;
            EnableHapticFeedback = true;

            UpdateUI();
            SaveSettings();

            OnSensitivityChanged?.Invoke(SensitivityType.Camera, CameraSensitivity);
            OnSensitivityChanged?.Invoke(SensitivityType.Movement, MovementSensitivity);
            OnSensitivityChanged?.Invoke(SensitivityType.Zoom, ZoomSensitivity);
        }

        /// <summary>
        /// Применяет вибрацию (для мобильных устройств).
        /// </summary>
        public void TriggerHapticFeedback()
        {
            if (!EnableHapticFeedback) return;

            if (Application.isMobilePlatform)
            {
                Handheld.Vibrate();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ограничивает значение чувствительности допустимым диапазоном.
        /// </summary>
        /// <param name="value">Значение для ограничения</param>
        /// <returns>Ограниченное значение</returns>
        private static float ClampSensitivity(float value)
        {
            return Mathf.Clamp(value, MIN_SENSITIVITY, MAX_SENSITIVITY);
        }

        /// <summary>
        /// Обновляет значение слайдера безопасно.
        /// </summary>
        /// <param name="slider">Слайдер для обновления</param>
        /// <param name="value">Новое значение</param>
        private void UpdateSlider(Slider slider, float value)
        {
            if (slider != null && Mathf.Abs(slider.value - value) > 0.001f)
            {
                slider.value = value;
            }
        }

        /// <summary>
        /// Обновляет все UI элементы.
        /// </summary>
        private void UpdateUI()
        {
            UpdateSlider(cameraSensitivitySlider, CameraSensitivity);
            UpdateSlider(movementSensitivitySlider, MovementSensitivity);
            UpdateSlider(zoomSensitivitySlider, ZoomSensitivity);
        }

        /// <summary>
        /// Сохраняет настройки в PlayerPrefs.
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(PREFS_KEY_CAMERA, CameraSensitivity);
            PlayerPrefs.SetFloat(PREFS_KEY_MOVEMENT, MovementSensitivity);
            PlayerPrefs.SetFloat(PREFS_KEY_ZOOM, ZoomSensitivity);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Загружает настройки из PlayerPrefs.
        /// </summary>
        public void LoadSettings()
        {
            CameraSensitivity = PlayerPrefs.GetFloat(PREFS_KEY_CAMERA, DEFAULT_SENSITIVITY);
            MovementSensitivity = PlayerPrefs.GetFloat(PREFS_KEY_MOVEMENT, DEFAULT_SENSITIVITY);
            ZoomSensitivity = PlayerPrefs.GetFloat(PREFS_KEY_ZOOM, DEFAULT_SENSITIVITY);
        }

        #endregion

        #region UI Event Handlers

        private void OnCameraSensitivityChanged(float value)
        {
            SetSensitivity(SensitivityType.Camera, value);
        }

        private void OnMovementSensitivityChanged(float value)
        {
            SetSensitivity(SensitivityType.Movement, value);
        }

        private void OnZoomSensitivityChanged(float value)
        {
            SetSensitivity(SensitivityType.Zoom, value);
        }

        private void OnInvertYChanged(bool value)
        {
            InvertYAxis = value;
            SaveSettings();
        }

        private void OnHapticFeedbackChanged(bool value)
        {
            EnableHapticFeedback = value;
            SaveSettings();
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие при изменении чувствительности.
        /// </summary>
        public event Action<SensitivityType, float> OnSensitivityChanged;

        #endregion

        #region Debug

        /// <summary>
        /// Выводит текущие настройки в консоль.
        /// </summary>
        public void DebugLogSettings()
        {
            Debug.Log("═══════════════════════════════");
            Debug.Log("📊 Настройки чувствительности");
            Debug.Log($"Камера: {CameraSensitivity:F2}x");
            Debug.Log($"Движение: {MovementSensitivity:F2}x");
            Debug.Log($"Зум: {ZoomSensitivity:F2}x");
            Debug.Log($"Инверсия Y: {(InvertYAxis ? "Вкл" : "Выкл")}");
            Debug.Log($"Вибрация: {(EnableHapticFeedback ? "Вкл" : "Выкл")}");
            Debug.Log("═══════════════════════════════");
        }

        #endregion
    }
}