# Система Сохранений Hour-peak

## Обзор

Система сохранений с поддержкой:
- **5 слотов** для сохранения
- **DateTime метки** для каждого сохранения
- **Автосохранение** на контроточных точках
- **UI интерфейс** для выбора слотов
- **JSON формат** для читаемости

## Файлы

```
Assets/Samples/Scripts/Addition/
├── SaveData.cs              # Структура данных сохранения
├── SaveManager.cs           # Менеджер сохранения/загрузки
├── SaveSlotUI.cs            # UI компонент слота
├── SaveManagerUI.cs         # UI менеджер списка сохранений
├── GameSaveController.cs    # Интеграция с игрой
└── SaveSystem_README.md     # Эта документация
```

## Быстрый старт

### 1. Настройка сцены

```
1. Создайте пустой объект → назовите "GameManager"
2. Добавьте компоненты:
   - SaveManager
   - GameSaveController
   - SaveManagerUI

3. Создайте UI:
   - Canvas → Panel (SaveMenuPanel)
   - Panel → Button (SlotPrefab)
   - Panel → Button (Create, Load, Delete, Cancel)
   - Panel → Text (StatusText)
```

### 2. Создание SaveData.asset

```
1. Project window → ПК → Create → Folder → "Resources"
2. Resources → ПК → Create → ScriptableObject → SaveData
3. Назовите: "SaveData.asset"
```

### 3. Использование в коде

```csharp
// Сохранение игры
SaveManager.Instance.SaveGame(0, saveData);

// Загрузка игры
var saveData = SaveManager.Instance.LoadGame(0);

// Создание из текущего состояния
var saveData = SaveManager.Instance.CreateCurrentSaveData();
SaveManager.Instance.SaveGame(0, saveData);

// Открытие UI меню
SaveManagerUI.Instance.OpenMenu();
```

## SaveData - Структура данных

```csharp
public class SaveData
{
    // Metadata
    public string SaveId;           // Уникальный ID
    public string SaveName;         // Имя для отображения
    public DateTime CreatedAt;      // Дата создания
    public DateTime LastModified;   // Последнее изменение
    public string FormattedDate;    // Формат: "dd.MM.yyyy HH:mm"

    // Progress
    public int CurrentLevel;        // Текущий уровень (0 = первый)
    public int DifficultyLevel;     // Сложность (0-2)
    public Vector3 PlayerPosition;  // Позиция игрока
    public Vector4 PlayerRotation;  // Вращение игрока
    public float LevelTime;         // Время в уровне
    public float TotalPlaytime;     // Общее время

    // State
    public bool IsLevelCompleted;   // Уровень пройден
    public int LevelStars;          // Звёзды в уровне
    public int TotalStars;          // Всего звёзд
    public int CrowdDensity;        // Настройки толпы
    public float CurrentSpeed;      // Текущая скорость
}
```

## SaveManager - Менеджер

### Основные методы

```csharp
// Сохранение
bool SaveGame(int slotIndex, SaveData data);
bool SaveCurrentGame(int slotIndex);

// Загрузка
SaveData LoadGame(int slotIndex);
bool LoadAndApplyGame(int slotIndex);

// Управление
bool DeleteSave(int slotIndex);
List<SaveData> RefreshSaveList();
List<int> GetAvailableSlots();
bool HasSave(int slotIndex);

// Статистика
long GetSaveFileSize(int slotIndex);
long GetTotalStorageUsed();
```

### События

```csharp
// Когда создан новый save
SaveManager.Instance.OnSaveCreated += (slot, data) => { ... };

// Когда загружен save
SaveManager.Instance.OnSaveLoaded += (slot, data) => { ... };

// Когда удалён save
SaveManager.Instance.OnSaveDeleted += slot => { ... };

// Когда обновлён список
SaveManager.Instance.OnSaveListRefreshed += () => { ... };
```

## GameSaveController - Интеграция

### Добавление в игру

```csharp
// На контрольной точке
public void OnCheckpointReached()
{
    GameSaveController.Instance.OnCheckpointReached();
}

// При завершении уровня
public void OnLevelCompleted(int stars)
{
    GameSaveController.Instance.OnLevelCompleted(stars);
}

// При старте уровня
public void StartLevel(int levelIndex)
{
    GameSaveController.Instance.StartLevel(levelIndex);
}

// При неудаче
public void OnLevelFailed()
{
    GameSaveController.Instance.OnLevelFailed();
}
```

## UI - SaveManagerUI

### Методы

```csharp
// Открыть/закрыть меню
SaveManagerUI.Instance.OpenMenu();
SaveManagerUI.Instance.CloseMenu();

// Обновить слоты
SaveManagerUI.Instance.RefreshSlots();

// Получить выбранный слот
int selectedSlot = SaveManagerUI.Instance.GetSelectedSlotIndex();
```

## Пример: Создание сохранения

```csharp
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    private void Start()
    {
        // Создаём данные сохранения
        var saveData = new SaveData
        {
            SaveName = "Мой прогресс",
            CurrentLevel = 2,
            DifficultyLevel = 1,
            PlayerPosition = transform.position,
            TotalStars = 15
        };

        // Сохраняем в слот 0
        SaveManager.Instance.SaveGame(0, saveData);
    }
}
```

## Пример: Загрузка сохранения

```csharp
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    private void Start()
    {
        // Проверяем наличие сохранения
        if (SaveManager.Instance.HasSave(0))
        {
            // Загружаем и применяем
            bool success = SaveManager.Instance.LoadAndApplyGame(0);
            
            if (success)
            {
                Debug.Log("Сохранение загружено!");
            }
        }
        else
        {
            Debug.Log("Нет сохранений. Начинаем новую игру.");
        }
    }
}
```

## Формат файла

Сохранения хранятся в:
```
{Application.persistentDataPath}/HourPeakSaves/save_00.json
{Application.persistentDataPath}/HourPeakSaves/save_01.json
...
```

Пример содержимого JSON:
```json
{
  "SaveId": "abc-123-def",
  "SaveName": "Level 3 - 15.03.2025 14.30",
  "CreatedAt": "2025-03-15T14:30:00",
  "LastModified": "2025-03-15T14:35:00",
  "CurrentLevel": 2,
  "DifficultyLevel": 1,
  "PlayerPosition": { "x": 10.5, "y": 0.0, "z": 25.3 },
  "PlayerRotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
  "LevelTime": 120.5,
  "TotalPlaytime": 3600.0,
  "IsLevelCompleted": true,
  "LevelStars": 3,
  "TotalStars": 15,
  "CrowdDensity": 50,
  "CurrentSpeed": 2.5
}
```

## Настройки

### В SaveManager (Inspector)

```
Max Save Slots: 5
Save Directory Name: HourPeakSaves
File Extension: .json
Encoding Name: UTF8
```

### В GameSaveController (Inspector)

```
Auto Save On Level Complete: true
Auto Save On Checkpoint: true
Auto Save Slot: 0
Show Save Logs: true
```

## Рекомендации

1. **Автосохранение**: Включите на контрольных точках
2. **Множественные слоты**: Дайте игроку выбор
3. **Обратная связь**: Показывайте статус сохранения
4. **Ошибки**: Обработайте сбои записи
5. **Тестирование**: Проверьте на разных устройствах

## Совместимость

- Unity 2021.3+
- .NET Standard 2.1
- Все платформы (Android, iOS, PC)

## Поддержка

При возникновении проблем проверьте:
1. Папка `Resources` существует
2. `SaveData.asset` создан
3. NavMesh запечён (для NPC)
4. Разрешения на запись файлов
