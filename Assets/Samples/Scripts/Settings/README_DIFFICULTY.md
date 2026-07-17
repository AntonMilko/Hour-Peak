# SettingDifficulty - Документация

## Обзор

Чистая, современная система настроек сложности для мобильных устройств. Поддержка трёх уровней сложности с автоматическим переключением и сохранением.

## Уровни сложности

### Новичок (Beginner)
| Параметр | Значение |
|----------|----------|
| **Толпа** | 20% |
| **Ограниченное время** | 120 секунд |
| **Допустимое время** | 240 секунд |
| **Автобусы** | 3 |
| **Машины** | 8 |
| **Множитель очков** | 1.0x |
| **Множитель опыта** | 1.0x |

### Профессионал (Professional)
| Параметр | Значение |
|----------|----------|
| **Толпа** | 50% |
| **Ограниченное время** | 60 секунд |
| **Допустимое время** | 120 секунд |
| **Автобусы** | 6 |
| **Машины** | 15 |
| **Множитель очков** | 1.5x |
| **Множитель опыта** | 1.5x |

### Экстремал (Extremal)
| Параметр | Значение |
|----------|----------|
| **Толпа** | 80% |
| **Ограниченное время** | 30 секунд |
| **Допустимое время** | 60 секунд |
| **Автобусы** | 10 |
| **Машины** | 25 |
| **Множитель очков** | 2.0x |
| **Множитель опыта** | 2.0x |

## Использование

### 1. Базовое использование

```csharp
using HourPeak.Settings;

// Получение экземпляра
var difficulty = SettingDifficulty.Instance;

// Текущая сложность
DifficultyLevel current = difficulty.CurrentDifficulty;

// Название текущей сложности
string name = difficulty.GetCurrentDifficultyName();
// Результат: "Новичок" или "Профессионал" или "Экстремал"
```

### 2. Переключение сложности

```csharp
// Автоматическое переключение (циклически)
// Новичок → Профессионал → Экстремал → Новичок
SettingDifficulty.Instance.CycleDifficulty();

// Установка конкретной сложности
SettingDifficulty.Instance.SetDifficulty(DifficultyLevel.Professional);

// Установка по индексу (0=Новичок, 1=Профессионал, 2=Экстремал)
SettingDifficulty.Instance.SetDifficultyByIndex(1);
```

### 3. Получение настроек

```csharp
var settings = SettingDifficulty.Instance.CurrentSettings;

// Параметры
float crowdMultiplier = settings.CrowdMultiplier;      // 0.2, 0.5 или 0.8
float timeLimit = settings.TimeLimit;                  // 120, 60 или 30
float acceptableTime = settings.AcceptableTime;        // 240, 120 или 60
int baseBusCount = settings.BaseBusCount;              // 3, 6 или 10
int baseCarCount = settings.BaseCarCount;              // 8, 15 или 25
float scoreMultiplier = settings.ScoreMultiplier;      // 1.0, 1.5 или 2.0
float xpMultiplier = settings.XPMultiplier;            // 1.0, 1.5 или 2.0
```

### 4. Применение множителей

```csharp
var settings = SettingDifficulty.Instance.CurrentSettings;

// Применяет множитель толпы
int actualCrowd = settings.ApplyCrowdMultiplier(100);  // 100 * 0.2 = 20

// Применяет множитель очков
int actualScore = settings.ApplyScoreMultiplier(1000); // 1000 * 1.5 = 1500

// Применяет множитель опыта
int actualXP = settings.ApplyXPMultiplier(100);        // 100 * 2.0 = 200
```

### 5. События

```csharp
void Start()
{
    var difficulty = SettingDifficulty.Instance;
    difficulty.OnDifficultyChanged += OnDifficultyChanged;
}

void OnDifficultyChanged(DifficultyLevel level, DifficultySettings settings)
{
    Debug.Log($"Сложность изменена на: {settings.DisplayName}");
    
    // Примените настройки к игре
    ApplyDifficultySettings(settings);
}

void ApplyDifficultySettings(DifficultySettings settings)
{
    // Установите толпу
    crowdSystem.SetMultiplier(settings.CrowdMultiplier);
    
    // Установите время
    timeManager.SetTimeLimit(settings.TimeLimit);
    
    // Установите транспорт
    trafficSystem.SetBusCount(settings.BaseBusCount);
    trafficSystem.SetCarCount(settings.BaseCarCount);
}
```

## Подключение к UI

### Кнопка переключения сложности

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HourPeak.Settings;

public class DifficultyButton : MonoBehaviour
{
    [SerializeField] private Button difficultyButton;
    [SerializeField] private TextMeshProUGUI difficultyText;

    private void Start()
    {
        var difficulty = SettingDifficulty.Instance;
        difficulty.OnDifficultyChanged += UpdateDifficultyText;
        
        UpdateDifficultyText(difficulty.CurrentDifficulty, difficulty.CurrentSettings);

        if (difficultyButton != null)
        {
            difficultyButton.onClick.AddListener(OnDifficultyClicked);
        }
    }

    private void OnDifficultyClicked()
    {
        SettingDifficulty.Instance.CycleDifficulty();
    }

    private void UpdateDifficultyText(DifficultyLevel level, DifficultySettings settings)
    {
        if (difficultyText != null)
        {
            difficultyText.text = settings.DisplayName;
        }
    }
}
```

### UI выбора сложности

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HourPeak.Settings;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private Button beginnerButton;
    [SerializeField] private Button professionalButton;
    [SerializeField] private Button extremalButton;

    private void Start()
    {
        if (beginnerButton != null)
            beginnerButton.onClick.AddListener(() => SetDifficulty(DifficultyLevel.Beginner));
        
        if (professionalButton != null)
            professionalButton.onClick.AddListener(() => SetDifficulty(DifficultyLevel.Professional));
        
        if (extremalButton != null)
            extremalButton.onClick.AddListener(() => SetDifficulty(DifficultyLevel.Extremal));
    }

    private void SetDifficulty(DifficultyLevel level)
    {
        SettingDifficulty.Instance.SetDifficulty(level);
    }
}
```

## Интеграция с игровыми системами

### Интеграция с системой толпы

```csharp
using UnityEngine;
using HourPeak.Settings;

public class CrowdSystem : MonoBehaviour
{
    private void Start()
    {
        var difficulty = SettingDifficulty.Instance;
        difficulty.OnDifficultyChanged += ApplyDifficulty;
        
        ApplyDifficulty(difficulty.CurrentDifficulty, difficulty.CurrentSettings);
    }

    private void ApplyDifficulty(DifficultyLevel level, DifficultySettings settings)
    {
        // Применяем множитель толпы
        float crowdMultiplier = settings.CrowdMultiplier;
        Debug.Log($"Толпа: {crowdMultiplier * 100:F0}%");
        
        // Ваш код для установки толпы
        // crowdManager.SetMultiplier(crowdMultiplier);
    }
}
```

### Интеграция с таймером

```csharp
using UnityEngine;
using HourPeak.Settings;

public class TimeManager : MonoBehaviour
{
    private void Start()
    {
        var difficulty = SettingDifficulty.Instance;
        difficulty.OnDifficultyChanged += ApplyDifficulty;
        
        ApplyDifficulty(difficulty.CurrentDifficulty, difficulty.CurrentSettings);
    }

    private void ApplyDifficulty(DifficultyLevel level, DifficultySettings settings)
    {
        // Устанавливаем время
        float timeLimit = settings.TimeLimit;
        float acceptableTime = settings.AcceptableTime;
        
        Debug.Log($"Время: {DifficultySettings.FormatTime(timeLimit)}");
        
        // Ваш код для установки времени
        // timer.SetTimeLimit(timeLimit);
        // timer.SetAcceptableTime(acceptableTime);
    }
}
```

### Интеграция с очками и опытом

```csharp
using UnityEngine;
using HourPeak.Settings;

public class ScoreManager : MonoBehaviour
{
    private void Start()
    {
        var difficulty = SettingDifficulty.Instance;
        difficulty.OnDifficultyChanged += ApplyDifficulty;
    }

    private void ApplyDifficulty(DifficultyLevel level, DifficultySettings settings)
    {
        // Применяем множители
        float scoreMult = settings.ScoreMultiplier;
        float xpMult = settings.XPMultiplier;
        
        Debug.Log($"Очки: {scoreMult}x | Опыт: {xpMult}x");
    }

    public int CalculateScore(int baseScore)
    {
        var settings = SettingDifficulty.Instance.CurrentSettings;
        return settings.ApplyScoreMultiplier(baseScore);
    }

    public int CalculateXP(int baseXP)
    {
        var settings = SettingDifficulty.Instance.CurrentSettings;
        return settings.ApplyXPMultiplier(baseXP);
    }
}
```

## API Reference

### SettingDifficulty (Менеджер)

| Метод | Описание |
|-------|----------|
| `CycleDifficulty()` | Переключить на следующую сложность |
| `SetDifficulty(level)` | Установить конкретную сложность |
| `SetDifficultyByIndex(index)` | Установить по индексу (0-2) |
| `GetCurrentDifficultyName()` | Получить название сложности |
| `GetCurrentDifficultyIndex()` | Получить индекс сложности (0-2) |
| `IsBeginner()` | Проверить, Новичок ли? |
| `IsExtremal()` | Проверить, Экстремал ли? |
| `DebugLogAllDifficulties()` | Вывести все сложности в консоль |

### Свойства SettingDifficulty

| Свойство | Тип | Описание |
|----------|-----|----------|
| `CurrentDifficulty` | `DifficultyLevel` | Текущий уровень |
| `CurrentSettings` | `DifficultySettings` | Текущие настройки |
| `AllDifficulties` | `DifficultySettings[]` | Все доступные сложности |

### События

| Событие | Вызывается |
|---------|------------|
| `OnDifficultyChanged` | При изменении сложности |

### DifficultySettings (Настройки)

| Метод | Описание |
|-------|----------|
| `ApplyCrowdMultiplier(count)` | Применить множитель толпы |
| `ApplyScoreMultiplier(score)` | Применить множитель очков |
| `ApplyXPMultiplier(xp)` | Применить множитель опыта |
| `FormatTime(seconds)` | Форматировать время (ММ:СС) |

### Свойства DifficultySettings

| Свойство | Тип | Описание |
|----------|-----|----------|
| `Level` | `DifficultyLevel` | Уровень сложности |
| `DisplayName` | `string` | Название для отображения |
| `CrowdMultiplier` | `float` | Множитель толпы (0.2-0.8) |
| `TimeLimit` | `float` | Ограниченное время |
| `AcceptableTime` | `float` | Допустимое время |
| `BaseBusCount` | `int` | Базовое количество автобусов |
| `BaseCarCount` | `int` | Базовое количество машин |
| `ScoreMultiplier` | `float` | Множитель очков |
| `XPMultiplier` | `float` | Множитель опыта |

## Примеры

### Пример 1: Вывод текущей сложности

```csharp
void Start()
{
    var difficulty = SettingDifficulty.Instance;
    difficulty.DebugLogAllDifficulties();
}

// Вывод:
// ═══════════════════════════════
// 📊 Уровни сложности
// ═══════════════════════════════
// [Новичок] Толпа: 20% | Время: 2:00 | Автобусы: 3 | Машины: 8
// [Профессионал] Толпа: 50% | Время: 1:00 | Автобусы: 6 | Машины: 15
// [Экстремал] Толпа: 80% | Время: 0:30 | Автобусы: 10 | Машины: 25
// ═══════════════════════════════
// Текущая сложность: Новичок
// ═══════════════════════════════
```

### Пример 2: Быстрое переключение

```csharp
// Нажатие на кнопку
public void OnDifficultyButtonClicked()
{
    SettingDifficulty.Instance.CycleDifficulty();
}

// Результат:
// Нажатие 1: Новичок → Профессионал
// Нажатие 2: Профессионал → Экстремал
// Нажатие 3: Экстремал → Новичок
```

## Сохранение

Система автоматически сохраняет выбор сложности в `PlayerPrefs`:

```csharp
// Ключ сохранения
private const string PREFS_KEY = "DifficultyLevel";

// Значения:
// 0 = Новичок
// 1 = Профессионал
// 2 = Экстремал
```

При запуске игры загружается сохранённая сложность.

## Статус

✅ Файл создан
✅ Современные практики программирования
✅ Три уровня сложности
✅ Автоматическое переключение
✅ Сохранение настроек
✅ Документация готова
