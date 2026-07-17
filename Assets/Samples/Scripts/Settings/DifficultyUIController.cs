using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HourPeak.Settings;

namespace HourPeak.Settings.UI
{
    /// <summary>
    /// Контроллер UI для отображения и переключения уровня сложности.
    /// Автоматически обновляет текст при переключении сложности.
    /// </summary>
    public class DifficultyUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Кнопка для переключения сложности")]
        [SerializeField] private Button difficultyButton;

        [Tooltip("Текст для отображения текущего уровня сложности")]
        [SerializeField] private TextMeshProUGUI difficultyText;

        [Header("Text Formatting")]
        [Tooltip("Показывать ли толпу в тексте")]
        [SerializeField] private bool showCrowd = true;

        [Tooltip("Показывать ли ограниченное время в тексте")]
        [SerializeField] private bool showTimeLimit = true;

        [Tooltip("Показывать ли допустимое время в тексте")]
        [SerializeField] private bool showAcceptableTime = true;

        [Tooltip("Жирный текст (Bold)")]
        [SerializeField] private bool boldText = true;

        [Tooltip("Курсив текст (Italic)")]
        [SerializeField] private bool italicText = false;

        [Tooltip("Выравнивание текста")]
        [SerializeField] private TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;

        [Tooltip("Размер шрифта")]
        [SerializeField] private float fontSize = 72f;

        private SettingDifficulty _difficulty;

        #region Properties

        /// <summary>
        /// Ссылка на компонент SettingDifficulty.
        /// </summary>
        public SettingDifficulty Difficulty
        {
            get
            {
                if (_difficulty == null)
                {
                    _difficulty = SettingDifficulty.Instance;
                }
                return _difficulty;
            }
        }

        /// <summary>
        /// Текущий текст отображения.
        /// </summary>
        public string DisplayText
        {
            get => difficultyText?.text ?? string.Empty;
            set
            {
                if (difficultyText != null)
                {
                    difficultyText.text = value;
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Получаем экземпляр SettingDifficulty
            _difficulty = HourPeak.Settings.SettingDifficulty.Instance;

            // Настраиваем TextMeshProUGUI если назначен
            if (difficultyText != null)
            {
                ConfigureTextComponent();
            }

            Debug.Log($"✅ DifficultyUIController инициализирован");
        }

        private void Start()
        {
            // Настраиваем слушатели
            SetupListeners();

            // Обновляем текст при старте
            UpdateDifficultyText();

            Debug.Log($"📊 Текущая сложность: {Difficulty.GetCurrentDifficultyName()}");
        }

        private void OnDestroy()
        {
            // Удаляем слушатели
            RemoveListeners();
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Настраивает компонент TextMeshProUGUI.
        /// </summary>
        private void ConfigureTextComponent()
        {
            if (difficultyText == null) return;

            // Применяем стили через битовую операцию
            TMPro.FontStyles fontStyle = TMPro.FontStyles.Normal;
            
            if (boldText)
            {
                fontStyle |= TMPro.FontStyles.Bold;
            }
            
            if (italicText)
            {
                fontStyle |= TMPro.FontStyles.Italic;
            }

            difficultyText.fontStyle = fontStyle;
            difficultyText.fontSize = fontSize;
            difficultyText.alignment = textAlignment;
            
            // Для multi-line текста
            difficultyText.enableAutoSizing = false;

            Debug.Log("✅ TextMeshProUGUI настроен");
        }

        /// <summary>
        /// Настраивает слушатели событий.
        /// </summary>
        private void SetupListeners()
        {
            // Подключаем клик по кнопке
            if (difficultyButton != null)
            {
                difficultyButton.onClick.AddListener(OnDifficultyButtonClicked);
                Debug.Log("✅ Слушатель кнопки настроен");
            }

            // Подключаем событие изменения сложности
            Difficulty.OnDifficultyChanged += OnDifficultyChanged;
            Debug.Log("✅ Слушатель события настроен");
        }

        /// <summary>
        /// Удаляет все слушатели событий.
        /// </summary>
        private void RemoveListeners()
        {
            if (difficultyButton != null)
            {
                difficultyButton.onClick.RemoveListener(OnDifficultyButtonClicked);
            }

            Difficulty.OnDifficultyChanged -= OnDifficultyChanged;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Обработчик нажатия на кнопку переключения сложности.
        /// </summary>
        public void OnDifficultyButtonClicked()
        {
            Debug.Log("🔘 Кнопка сложности нажата!");

            // Переключаем сложность
            Difficulty.SetDifficultyChange();

            // Обновляем текст
            UpdateDifficultyText();
        }

        /// <summary>
        /// Обработчик события изменения сложности.
        /// Вызывается автоматически при переключении сложности.
        /// </summary>
        private void OnDifficultyChanged(DifficultyLevel level, DifficultySettings settings)
        {
            Debug.Log($"🎯 Сложность изменена: {settings.DisplayName}");
            
            // Обновляем текст
            UpdateDifficultyText();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Обновляет текст отображения текущей сложности.
        /// </summary>
        public void UpdateDifficultyText()
        {
            if (difficultyText == null)
            {
                Debug.LogWarning("⚠️ TextMeshProUGUI не назначен!");
                return;
            }

            if (Difficulty == null)
            {
                Debug.LogWarning("⚠️ SettingDifficulty не инициализирован!");
                return;
            }

            var settings = Difficulty.GetCurrentSettings();
            
            if (settings == null)
            {
                Debug.LogWarning("⚠️ Настройки сложности не найдены!");
                return;
            }

            // Формируем полный текст с характеристиками
            string text = FormatDifficultyText(settings);
            
            // Применяем цвет в зависимости от сложности
            string coloredText = ApplyColorToText(text, settings.Level);

            difficultyText.text = coloredText;

            Debug.Log($"📝 Текст обновлён: {settings.DisplayName}");
        }

        /// <summary>
        /// Форматирует текст сложности со всеми характеристиками.
        /// </summary>
        private string FormatDifficultyText(DifficultySettings settings)
        {
            string text = "";

            // Название сложности (жирное)
            text += $"<size=120%><b>Сложность: {settings.DisplayName}</b></size>\n";

            // Толпа
            if (showCrowd)
            {
                float crowdPercent = settings.CrowdMultiplier * 100;
                text += $"Толпа: {crowdPercent:F0}%\n";
            }

            // Ограниченное время
            if (showTimeLimit)
            {
                string timeLimitFormatted = DifficultySettings.FormatTime(settings.TimeLimit);
                text += $"Ограниченное время: {timeLimitFormatted}\n";
            }

            // Допустимое время
            if (showAcceptableTime)
            {
                string acceptableTimeFormatted = DifficultySettings.FormatTime(settings.AcceptableTime);
                text += $"Допустимое время: {acceptableTimeFormatted}";
            }

            return text;
        }

        /// <summary>
        /// Применяет цвет ко всем строкам текста в зависимости от уровня сложности.
        /// </summary>
        private string ApplyColorToText(string text, DifficultyLevel level)
        {
            // Цвета для каждого уровня
            string color = level switch
            {
                DifficultyLevel.Beginner => "#4CAF50",    // Зелёный
                DifficultyLevel.Professional => "#FF9800",  // Оранжевый
                DifficultyLevel.Extremal => "#F44336",      // Красный
                _ => "#FFFFFF"                              // Белый
            };

            // Применяем цвет ко всему тексту (все строки)
            return $"<color={color}>{text}</color>";
        }

        /// <summary>
        /// Устанавливает конкретный уровень сложности.
        /// </summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            Difficulty.SetDifficulty(level);
            UpdateDifficultyText();
        }

        /// <summary>
        /// Переключает сложность на следующий уровень.
        /// </summary>
        public void CycleDifficulty()
        {
            Difficulty.SetDifficultyChange();
        }

        /// <summary>
        /// Получает текущее название сложности.
        /// </summary>
        public string GetCurrentDifficultyName()
        {
            return Difficulty.GetCurrentDifficultyName();
        }

        /// <summary>
        /// Сбрасывает сложность на Новичок.
        /// </summary>
        public void ResetToBeginner()
        {
            Difficulty.SetDifficulty(DifficultyLevel.Beginner);
            UpdateDifficultyText();
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Автоматическая настройка ссылок через контекстное меню.
        /// </summary>
        [ContextMenu("Auto-Setup References")]
        private void AutoSetupReferences()
        {
            // Ищем TextMeshProUGUI в иерархии
            difficultyText = GetComponentInChildren<TextMeshProUGUI>();
            
            // Ищем Button в иерархии
            difficultyButton = GetComponentInChildren<Button>();

            if (difficultyText != null)
            {
                Debug.Log($"✅ TextMeshProUGUI найден: {difficultyText.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ TextMeshProUGUI не найден!");
            }

            if (difficultyButton != null)
            {
                Debug.Log($"✅ Button найден: {difficultyButton.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Button не найден!");
            }
        }

        /// <summary>
        /// Тестовое переключение сложности для проверки.
        /// </summary>
        [ContextMenu("Test Cycle Difficulty")]
        private void TestCycleDifficulty()
        {
            Debug.Log("🧪 Тестовое переключение сложности...");
            CycleDifficulty();
            UpdateDifficultyText();
        }

        /// <summary>
        /// Вывод текущих настроек в консоль.
        /// </summary>
        [ContextMenu("Debug Log Settings")]
        private void DebugLogSettings()
        {
            Difficulty.DebugLogAllDifficulties();
        }
#endif

        #endregion

        #region Debug

        private void OnValidate()
        {
            // Обновляем текст при изменении настроек в Inspector только во время игры
            if (!Application.isPlaying) return;
            
            if (difficultyText == null) return;
            
            if (Difficulty == null) return;
            
            try
            {
                UpdateDifficultyText();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Ошибка в OnValidate: {e.Message}");
            }
        }

        #endregion
    }
}