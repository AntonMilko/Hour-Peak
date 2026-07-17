# SavingSettingsGame - Документация

## Обзор

Чистая, современная система сохранения настроек игры. **Автоматически сохраняет все настройки при выходе из меню настроек**.

## Особенности

✅ **Автоматическое сохранение**:
- При выходе из настроек
- При изменении сложности
- При изменении чувствительности

✅ **Упрощённая архитектура (Singleton)**:
- Нет необходимости назначать ссылки в Inspector
- SettingDifficulty.Instance и SettingSensitivity.Instance автоматически инициализируются

✅ **Безопасность**:
- Подтверждение выхода
- Обработка ошибок
- Валидация данных

✅ **Современный код**:
- Singleton паттерн
- События для реактивного программирования
- Clean Code практики

## Использование

### 1. Базовое использование

```csharp
using HourPeak.Settings;

// Получение экземпляра
var saving = SavingSettingsGame.Instance;

// Сохранение всех настроек
saving.SaveAllSettings();

// Загрузка всех настроек
saving.LoadAllSettings();

// Сброс к значениям по умолчанию
saving.ResetAllSettingsToDefault();

// Выход из настроек (с сохранением)
saving.ConfirmExitSettings();
```

### 2. Подключение к UI

```csharp
using UnityEngine;
using UnityEngine.UI;
using HourPeak.Settings;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button saveButton;

    private void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveClicked);
        }
    }

    private void OnExitClicked()
    {
        // Показывает диалог подтверждения и сохраняет настройки
        SavingSettingsGame.Instance.ShowExitConfirmationDialog();
    }

    private void OnSaveClicked()
    {
        // Сохраняет настройки и закрывает меню
        SavingSettingsGame.Instance.SaveAllSettings();
    }
}
```

### 3. События

```csharp
void Start()
{
    var saving = SavingSettingsGame.Instance;
    
    // При успешном сохранении
    saving.OnSettingsSaved += OnSettingsSaved;
    
    // При выходе из настроек
    saving.OnExitSettings += OnExitSettings;
    
    // При отмене выхода
    saving.OnCancelExit += OnCancelExit;
}

void OnSettingsSaved()
{
    Debug.Log("✅ Настройки сохранены");
}

void OnExitSettings()
{
    Debug.Log("🔙 Выход из настроек");
    // Закрыть окно настроек
    CloseSettingsWindow();
}

void OnCancelExit()
{
    Debug.Log("⛔ Выход отменён");
}
```

## Настройка в Unity Editor

### 1. Добавить в сцену

```
GameObject → Create Empty → Rename to "SettingsManager"
Add Component → SavingSettingsGame
```

### 2. Настроить ссылки

В Inspector установите:
- `Exit Settings Button` → Кнопка выхода
- `Save Settings Button` → Кнопка сохранения
- `Confirmation Dialog` → Окно подтверждения
- `Confirm Exit Button` → Кнопка подтверждения
- `Cancel Exit Button` → Кнопка отмены

**Примечание**: Ссылки на SettingDifficulty и SettingSensitivity **не нужны**! Они автоматически инициализируются через Singleton.

### 3. Настроить кнопки

```csharp
// Кнопка выхода
exitButton.onClick.AddListener(() => 
{
    SavingSettingsGame.Instance.ShowExitConfirmationDialog();
});

// Кнопка подтверждения выхода
confirmExitButton.onClick.AddListener(() => 
{
    SavingSettingsGame.Instance.ConfirmExitSettings();
});

// Кнопка отмены
cancelExitButton.onClick.AddListener(() => 
{
    SavingSettingsGame.Instance.HideExitConfirmationDialog();
});
```

## Интеграция с SettingDifficulty

```csharp
using UnityEngine;
using HourPeak.Settings;

public class DifficultyController : MonoBehaviour
{
    private void Start()
    {
        var difficulty = SettingDifficulty.Instance;
        
        // При изменении сложности автоматически сохраняется через SavingSettingsGame
        difficulty.OnDifficultyChanged += (level, settings) =>
        {
            Debug.Log($"📊 Сложность изменена: {settings.DisplayName}");
            // Настройки сохраняются автоматически
        };
    }
}
```

## Интеграция с SettingSensitivity

```csharp
using UnityEngine;
using HourPeak.Settings;

public class SensitivityController : MonoBehaviour
{
    private void Start()
    {
        var sensitivity = SettingSensitivity.Instance;
        
        // При изменении чувствительности автоматически сохраняется через SavingSettingsGame
        sensitivity.OnSensitivityChanged += (type, value) =>
        {
            Debug.Log($"🎚️ Чувствительность изменена: {type} = {value}");
            // Настройки сохраняются автоматически
        };
    }
}
```

## Полный пример: Меню настроек

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HourPeak.Settings;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("Difficulty")]
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Buttons")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private SettingDifficulty _difficulty;
    private SettingSensitivity _sensitivity;
    private SavingSettingsGame _saving;

    private void Start()
    {
        // Получаем экземпляры
        _difficulty = SettingDifficulty.Instance;
        _sensitivity = SettingSensitivity.Instance;
        _saving = SavingSettingsGame.Instance;

        // Подписываемся на события
        _saving.OnExitSettings += CloseSettings;
        _saving.OnCancelExit += CancelExit;

        // Настраиваем кнопки
        exitButton.onClick.AddListener(OnExitClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        // Обновляем UI
        UpdateDifficultyText();
    }

    private void OnExitClicked()
    {
        _saving.ShowExitConfirmationDialog();
    }

    private void OnSaveClicked()
    {
        _saving.SaveAllSettings();
        CloseSettings();
    }

    private void OnConfirmClicked()
    {
        _saving.ConfirmExitSettings();
    }

    private void OnCancelClicked()
    {
        _saving.HideExitConfirmationDialog();
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(false);
    }

    private void CancelExit()
    {
        // Ничего не делаем, просто скрываем диалог
    }

    private void UpdateDifficultyText()
    {
        difficultyText.text = _difficulty.GetCurrentDifficultyName();
    }
}
```

## API Reference

### Методы

| Метод | Описание |
|-------|----------|
| `SaveAllSettings()` | Сохранить все настройки |
| `LoadAllSettings()` | Загрузить все настройки |
| `ResetAllSettingsToDefault()` | Сбросить к значениям по умолчанию |
| `ClearAllSettings()` | Очистить все настройки |
| `ShowExitConfirmationDialog()` | Показать диалог подтверждения выхода |
| `HideExitConfirmationDialog()` | Скрыть диалог подтверждения |
| `ConfirmExitSettings()` | Подтвердить выход с сохранением |
| `DebugLogAllSettings()` | Вывести настройки в консоль |

### События

| Событие | Вызывается |
|---------|------------|
| `OnSettingsSaved` | При успешном сохранении |
| `OnExitSettings` | При выходе из настроек |
| `OnCancelExit` | При отмене выхода |

## Порядок работы

```
1. Игрок заходит в настройки
   ↓
2. Изменяет сложность/чувствительность
   ↓
3. Нажимает "Выйти" или "Сохранить"
   ↓
4. Показывается диалог подтверждения
   ↓
5. Игрок подтверждает выход
   ↓
6. Автоматически сохраняются все настройки
   ↓
7. Выход из меню настроек
```

## Сохранение

Все настройки сохраняются в `PlayerPrefs`:

```csharp
// Сложность
PlayerPrefs.SetInt("DifficultyLevel", 0-2);

// Чувствительность
PlayerPrefs.SetFloat("Sensitivity_Camera", 1.0f);
PlayerPrefs.SetFloat("Sensitivity_Movement", 1.0f);
PlayerPrefs.SetFloat("Sensitivity_Zoom", 1.0f);

// Общие настройки
PlayerPrefs.SetInt("SettingsVersion", 1);
PlayerPrefs.Save();
```

## Статус

✅ Файл переписан
✅ Удалён BinaryFormatter (устаревший)
✅ Используется PlayerPrefs
✅ Автоматическое сохранение при выходе
✅ Интеграция с SettingDifficulty/SettingSensitivity
✅ Документация готова
