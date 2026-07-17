# SettingSensitivity - Документация

## Обзор

Чистая, современная система настроек чувствительности для мобильных устройств. Поддержка тач-управления, вибрации и сохранение настроек.

## Особенности

✅ **Три типа чувствительности**:
- Камера (вращение)
- Движение (перемещение)
- Зум (приближение)

✅ **Мобильные функции**:
- Вибрация при взаимодействии (Haptic Feedback)
- Инверсия вертикальной оси
- Поддержка Android и iOS

✅ **Современный код**:
- Pattern matching (`switch` expression)
- Свойства с get/set
- События для реактивного программирования
- Singleton паттерн

✅ **Сохранение настроек**:
- Автоматическое сохранение в PlayerPrefs
- Загрузка при старте

## Использование

### 1. Базовое использование

```csharp
using HourPeak.Settings;

// Получение экземпляра
var sensitivity = SettingSensitivity.Instance;

// Получение чувствительности
float cameraSens = sensitivity.GetSensitivity(SensitivityType.Camera);

// Установка чувствительности
sensitivity.SetSensitivity(SensitivityType.Movement, 2.5f);

// Увеличение/уменьшение
sensitivity.IncreaseSensitivity(SensitivityType.Zoom);
sensitivity.DecreaseSensitivity(SensitivityType.Zoom);

// Сброс к настройкам по умолчанию
sensitivity.ResetToDefaults();
```

### 2. Подключение к UI

```csharp
// В Unity Editor настройте UI элементы:
// - Slider для Camera Sensitivity
// - Slider для Movement Sensitivity
// - Slider для Zoom Sensitivity
// - Toggle для Invert Y Axis
// - Toggle для Haptic Feedback

// Скрипт автоматически инициализирует UI
```

### 3. События

```csharp
void Start()
{
    var sensitivity = SettingSensitivity.Instance;
    sensitivity.OnSensitivityChanged += OnSensitivityChanged;
}

void OnSensitivityChanged(SensitivityType type, float value)
{
    Debug.Log($"Чувствительность {type} изменена на {value}");
    
    // Обновите соответствующий компонент
    switch (type)
    {
        case SensitivityType.Camera:
            cameraController.SetSensitivity(value);
            break;
        case SensitivityType.Movement:
            playerController.SetMovementSensitivity(value);
            break;
        case SensitivityType.Zoom:
            cameraController.SetZoomSensitivity(value);
            break;
    }
}
```

### 4. Вибрация (Haptic Feedback)

```csharp
// Автоматическая вибрация при изменении чувствительности
sensitivity.IncreaseSensitivity(SensitivityType.Camera);

// Ручная вибрация
sensitivity.TriggerHapticFeedback();
```

## API Reference

### Константы

| Константа | Значение | Описание |
|-----------|----------|----------|
| `MIN_SENSITIVITY` | `0.1f` | Минимальное значение |
| `MAX_SENSITIVITY` | `10.0f` | Максимальное значение |
| `DEFAULT_SENSITIVITY` | `1.0f` | Значение по умолчанию |
| `SENSITIVITY_STEP` | `0.1f` | Шаг изменения |

### Свойства

| Свойство | Тип | Описание |
|----------|-----|----------|
| `CameraSensitivity` | `float` | Чувствительность камеры |
| `MovementSensitivity` | `float` | Чувствительность движения |
| `ZoomSensitivity` | `float` | Чувствительность зума |
| `InvertYAxis` | `bool` | Инверсия вертикальной оси |
| `EnableHapticFeedback` | `bool` | Вибрация при взаимодействии |

### Методы

| Метод | Описание |
|-------|----------|
| `GetSensitivity(SensitivityType)` | Получить чувствительность |
| `SetSensitivity(SensitivityType, float)` | Установить чувствительность |
| `IncreaseSensitivity(SensitivityType)` | Увеличить на шаг |
| `DecreaseSensitivity(SensitivityType)` | Уменьшить на шаг |
| `ResetToDefaults()` | Сбросить к значениям по умолчанию |
| `TriggerHapticFeedback()` | Запустить вибрацию |
| `DebugLogSettings()` | Вывести настройки в консоль |

### События

| Событие | Вызывается |
|---------|------------|
| `OnSensitivityChanged` | При изменении любой чувствительности |

## Настройка в Unity Editor

### 1. Добавить в сцену

```
GameObject → Create Empty → Rename to "SettingsManager"
Add Component → SettingSensitivity
```

### 2. Настроить UI

Создайте UI элементы:

```
Canvas → UI Elements:
├── Slider (Camera Sensitivity)
├── Slider (Movement Sensitivity)
├── Slider (Zoom Sensitivity)
├── Toggle (Invert Y Axis)
└── Toggle (Haptic Feedback)
```

### 3. Привязать UI элементы

В Inspector настройте:
- `Camera Sensitivity Slider` → Ссылка на Slider
- `Movement Sensitivity Slider` → Ссылка на Slider
- `Zoom Sensitivity Slider` → Ссылка на Slider
- `Invert Y Axis Toggle` → Ссылка на Toggle
- `Haptic Feedback Toggle` → Ссылка на Toggle

## Пример: Интеграция с камерой

```csharp
using UnityEngine;
using HourPeak.Settings;

public class CameraController : MonoBehaviour
{
    private float _rotationSpeed;

    private void Start()
    {
        var sensitivity = SettingSensitivity.Instance;
        sensitivity.OnSensitivityChanged += OnSensitivityChanged;
        
        UpdateRotationSpeed();
    }

    private void UpdateRotationSpeed()
    {
        _rotationSpeed = SettingSensitivity.Instance
            .GetSensitivity(SensitivityType.Camera);
    }

    private void OnSensitivityChanged(SensitivityType type, float value)
    {
        if (type == SensitivityType.Camera)
        {
            UpdateRotationSpeed();
        }
    }

    private void Update()
    {
        float rotation = Input.GetAxis("Mouse X") * _rotationSpeed;
        transform.Rotate(0, rotation, 0);
    }
}
```

## Пример: Интеграция с игроком

```csharp
using UnityEngine;
using HourPeak.Settings;

public class PlayerController : MonoBehaviour
{
    private float _movementSpeed;

    private void Start()
    {
        var sensitivity = SettingSensitivity.Instance;
        sensitivity.OnSensitivityChanged += OnSensitivityChanged;
        
        UpdateMovementSpeed();
    }

    private void UpdateMovementSpeed()
    {
        _movementSpeed = SettingSensitivity.Instance
            .GetSensitivity(SensitivityType.Movement);
    }

    private void OnSensitivityChanged(SensitivityType type, float value)
    {
        if (type == SensitivityType.Movement)
        {
            UpdateMovementSpeed();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0, vertical);
        transform.Translate(direction * _movementSpeed * Time.deltaTime);
    }
}
```

## Статус

✅ Файл создан
✅ Современные практики программирования
✅ Поддержка мобильных устройств
✅ Документация готова
