using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Создаёт и управляет 20 слотами сохранений на GameObject FoldersSaveMenu.
/// Полностью самодостаточный — не использует GameObject.Find().
/// Гарантирует отображение всех 20 слотов.
/// </summary>
public class SaveSlotSystem : MonoBehaviour
{
    [Header("Настройки слотов")]
    [SerializeField] private int totalSlots = 20;
    [SerializeField] private int slotsPerRow = 5; // 5 в ряд
    [SerializeField] private int rowsCount = 4;   // 4 ряда
    [SerializeField] private Vector3 slotSize = new Vector3(180f, 90f, 1f); // Фиксированный размер слота
    [SerializeField] private float slotSpacingX = 10f; // Горизонтальный отступ между слотами
    [SerializeField] private float slotSpacingY = 10f; // Вертикальный отступ между слотами
    [SerializeField] private Color slotColor = Color.black; // Чёрный цвет

    private List<GameObject> _slots = new List<GameObject>();
    private List<TextMeshProUGUI> _dateTimeTexts = new List<TextMeshProUGUI>();
    private Canvas _canvas;
    private Transform _slotsParent; // FoldersSavePanel - родитель для слотов
    private Dictionary<int, Vector2> _slotPositions = new Dictionary<int, Vector2>(); // Сохраняем позиции слотов
    private bool _isInitialized = false;

    // 🔥 КЭШИРОВАННЫЕ ССЫЛКИ — чтобы не искать каждый раз
    private SaveManager _saveManager;
    private GameSaveController _gameController;
    private LevelMenuManager _levelMenuManager;
    private GameObject _foldersSaveMenu;
    private Transform _rootTransform; // Кэш root

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // 🔥 Сохраняем root (MainMenuUI)
        _rootTransform = transform.root;
        
        _foldersSaveMenu = GameObject.Find("FoldersSaveMenu");
        if (_foldersSaveMenu == null) _foldersSaveMenu = _rootTransform.gameObject;
        
        _isInitialized = true;
    }

    private void Start()
    {
        SpawnAllSlots();
    }

    // ========================================================================
    // ГЛАВНЫЙ МЕТОД: СОЗДАНИЕ ВСЕХ 20 СЛОТОВ
    // ========================================================================

    [ContextMenu("🔥 Spawn All Slots")]
    public void SpawnAllSlots()
    {
        // 🔥 Ищем FoldersSavePanel только один раз
        _slotsParent = transform.Find("FoldersSavePanel");
        if (_slotsParent == null && transform.root != null)
        {
            _slotsParent = transform.root.Find("FoldersSavePanel");
        }
        
        if (_slotsParent == null)
        {
            _slotsParent = transform;
        }
        
        _isInitialized = true;

        _slots.Clear();
        _dateTimeTexts.Clear();

        // НЕ удаляем GameObject — просто пересоздаём слоты заново
        // Старые GameObject останутся, но мы будем использовать новые из _slots
        Debug.Log($"[SaveSlotSystem] Пересоздание {totalSlots} СЛОТОВ...");
        int createdCount = 0;
        int failedCount = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            bool success = CreateSingleSlot(i);
            
            if (success)
            {
                createdCount++;
            }
            else
            {
                failedCount++;
            }
        }

        // 📊 ОТЧЁТ ПО СТАТУСУ
        Debug.Log($"[SaveSlotSystem] ========================================");
        Debug.Log($"[SaveSlotSystem] Total slots: {totalSlots}");
        Debug.Log($"[SaveSlotSystem] Created slots: {createdCount}");
        Debug.Log($"[SaveSlotSystem] Failed slots: {failedCount}");
        Debug.Log($"[SaveSlotSystem] Render check: {(failedCount == 0 ? "PASSED" : "FAILED")}");
        Debug.Log($"[SaveSlotSystem] ========================================");

        // Если есть ошибки — делаем скриншот
        if (failedCount > 0)
        {
            Debug.LogError($"[SaveSlotSystem] КРИТИЧЕСКАЯ ОШИБКА: {failedCount} слотов не созданы!");
#if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot("slot_error.png");
#endif
            throw new System.Exception($"Failed to create {failedCount} out of {totalSlots} slots!");        }

        // 💥 УСИЛЕННАЯ ЛОГИКА: финальная проверка
        Debug.Assert(_slots.Count == totalSlots, "Critical failure: slot count mismatch!");
        
        // Валидация: все слоты в списке
        Debug.Assert(_slots.Count == 20, "Wrong slot count!");
    }

    // ========================================================================
    // УТИЛИТЫ
    // ========================================================================

    // ========================================================================
    // СОЗДАНИЕ ОДНОГО СЛОТА С ПОЛНОЙ ВАЛИДАЦИЕЙ
    // ========================================================================

    private bool CreateSingleSlot(int index)
    {
        string folderName = $"Folder {index + 1}";
        
        // Создаём GameObject
        GameObject folderGO = new GameObject(folderName);
        
        // Проверяем, что объект создан
        if (folderGO == null)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Folder {index} не создан!");
            throw new System.Exception($"Folder {index} не создан");
        }

        // Добавляем в иерархию — прямой ребёнок FoldersSavePanel
        folderGO.transform.SetParent(_slotsParent, true);
        Debug.Log($"[CreateSingleSlot] Слот #{index} создан. Родитель: {_slotsParent.name}, childCount родителя: {_slotsParent.childCount}");
        
        // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РАЗМЕРА PARENT (Panel)
        RectTransform parentRT = _slotsParent as RectTransform;
        float parentWidth = parentRT != null ? parentRT.rect.width : 800f;
        float parentHeight = parentRT != null ? parentRT.rect.height : 600f;
        
        float slotWidth = (parentWidth - (slotsPerRow - 1) * slotSpacingX) / slotsPerRow;
        float slotHeight = (parentHeight - (rowsCount - 1) * slotSpacingY) / rowsCount;
        
        // Ограничиваем максимальный размер слота — чтобы не были чертовски большими
        float maxSlotWidth = 300f;
        float maxSlotHeight = 150f;
        float currentSlotSizeX = Mathf.Min(Mathf.Max(slotWidth, 50f), maxSlotWidth);
        float currentSlotSizeY = Mathf.Min(Mathf.Max(slotHeight, 50f), maxSlotHeight);
        
        // 🔴 РАСЧЁТ ПОЗИЦИИ В СЕТКЕ 5x4 (сверху вниз по возрастанию)
        int row = index / slotsPerRow;
        int col = index % slotsPerRow;
        
        // Инвертируем row: 0 = верхний ряд, 3 = нижний ряд
        int displayRow = rowsCount - 1 - row;
        
        float totalGridWidth = slotsPerRow * currentSlotSizeX + (slotsPerRow - 1) * slotSpacingX;
        float totalGridHeight = rowsCount * currentSlotSizeY + (rowsCount - 1) * slotSpacingY;
        
        // Центрируем сетку относительно родителя
        float centerX = -totalGridWidth / 2f + (col * (currentSlotSizeX + slotSpacingX) + currentSlotSizeX / 2f);
        float centerY = totalGridHeight / 2f - (displayRow * (currentSlotSizeY + slotSpacingY) + currentSlotSizeY / 2f);
        
        // Масштаб 1, размеры через RectTransform
        folderGO.transform.localScale = Vector3.one;
        folderGO.transform.localRotation = Quaternion.identity;

        // Получаем или создаём RectTransform для folderGO
        RectTransform folderRT = folderGO.GetComponent<RectTransform>();
        if (folderRT == null)
        {
            folderRT = folderGO.AddComponent<RectTransform>();
        }
        // Устанавливаем размер слота на основе размера Panel
        folderRT.sizeDelta = new Vector2(currentSlotSizeX, currentSlotSizeY);
        // Используем anchoredPosition для позиционирования относительно центра родителя
        folderRT.anchorMin = new Vector2(0.5f, 0.5f);
        folderRT.anchorMax = new Vector2(0.5f, 0.5f);
        folderRT.anchoredPosition = new Vector2(centerX, -centerY);
        
        // Запоминаем оригинальную позицию слота для возврата
        _slotPositions[index] = new Vector2(centerX, -centerY);

        // Добавляем Image компонент (UISprite форма)
        Image img = folderGO.AddComponent<Image>();
        if (img == null)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Folder {index} не может добавить Image компонент!");
            return false;
        }

        img.enabled = true;
        img.color = Color.black; // Чёрный цвет
        img.raycastTarget = true;
        img.sprite = null; // Используем цвет, не спрайт

        // Добавляем Button для обработки кликов
        Button btn = folderGO.AddComponent<Button>();
        if (btn == null)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Folder {index} не может добавить Button компонент!");
            return false;
        }

        btn.enabled = true;
        
        // Принудительно включаем Image (Button требует Image для работы!)
        Image imgComp = folderGO.GetComponent<Image>();
        if (imgComp != null)
        {
            imgComp.enabled = true;
            imgComp.raycastTarget = true;
        }
        
        // Привязываем обработчик клика к кнопке
        int slotIndex = index; // замыкаем в локальную переменную для корректной работы лямбды
        btn.onClick.AddListener(() => {
            Debug.Log($"[SaveSlotSystem] 🖱️ OnSlotClicked вызван для слота {slotIndex}, parent={folderGO.transform.parent?.name ?? "null"}");
            OnSlotClicked(slotIndex);
        });

        // Добавляем TextMeshProUGUI для отображения имени папки
        GameObject textGO = new GameObject("FileNameText");
        textGO.transform.SetParent(folderGO.transform, false);
        textGO.transform.localPosition = Vector3.zero;
        textGO.transform.localScale = Vector3.one;
        textGO.transform.localRotation = Quaternion.identity;

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.sizeDelta = new Vector2(slotSize.x - 20f, 50f); // Чуть меньше слота по ширине
        textRT.anchoredPosition = new Vector2(0f, 8f); // Чуть выше центра

        TMPro.TextMeshProUGUI textMesh = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Folder {index} не может добавить Text компонент!");
            return false;
        }

        textMesh.enabled = true;
        textMesh.text = $"Файл {index + 1}";
        textMesh.fontSize = 36;
        textMesh.color = Color.white;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;

        // Добавляем TextMeshProUGUI для отображения даты/времени
        GameObject dateTimeGO = new GameObject("DateTimeText");
        dateTimeGO.transform.SetParent(folderGO.transform, false);
        dateTimeGO.transform.localPosition = Vector3.zero;
        dateTimeGO.transform.localScale = Vector3.one;
        dateTimeGO.transform.localRotation = Quaternion.identity;

        RectTransform dateTimeRT = dateTimeGO.AddComponent<RectTransform>();
        dateTimeRT.anchorMin = new Vector2(0.5f, 0.5f);
        dateTimeRT.anchorMax = new Vector2(0.5f, 0.5f);
        dateTimeRT.sizeDelta = new Vector2(slotSize.x - 16f, 35f);
        dateTimeRT.anchoredPosition = new Vector2(0f, -12f);

        TMPro.TextMeshProUGUI dateTimeText = dateTimeGO.AddComponent<TMPro.TextMeshProUGUI>();
        if (dateTimeText == null)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Folder {index} не может добавить DateTimeText компонент!");
            return false;
        }

        dateTimeText.enabled = true;
        dateTimeText.text = "";
        dateTimeText.fontSize = 36;
        dateTimeText.color = Color.white;
        dateTimeText.alignment = TMPro.TextAlignmentOptions.Center;

        // Сохраняем ссылку на DateTimeText
        _dateTimeTexts.Add(dateTimeText);

        // Gizmo
        folderGO.AddComponent<SlotGizmoDrawer>();
        _slots.Add(folderGO);

        // Лог
        Debug.Log($"[SaveSlotSystem] ✅ Folder {index + 1}: '{folderName}', pos={folderRT.anchoredPosition}, size={folderRT.sizeDelta}");

        return true;
    }

    // ========================================================================
    // УПРАВЛЕНИЕ МЕНЮ
    // ========================================================================

    /// <summary>
    /// Открывает меню сохранений.
    /// </summary>
    public void OpenMenu()
    {
        if (gameObject.activeSelf)
        {
            Debug.Log("[SaveSlotSystem] Menu is already open.");
            return;
        }

        Debug.Log("[SaveSlotSystem] Opening Menu...");
        
        // Активируем родительский GameObject (FoldersSaveMenu)
        // Это автоматически сделает видимыми все 20 слотов
        gameObject.SetActive(true);

        // На всякий случай убеждаемся, что слоты созданы
        if (_slots.Count == 0)
        {
            SpawnAllSlots();
        }
    }

    /// <summary>
    /// Закрывает меню сохранений.
    /// </summary>
    public void CloseMenu()
    {
        Debug.Log("[SaveSlotSystem] Closing Menu...");
        gameObject.SetActive(false);
    }

    // ========================================================================
    // ОБРАБОТКА КЛИКОВ ПО СЛОТАМ
    // ========================================================================

    /// <summary>
    /// Вызывается при клике на слот сохранения.
    /// Загружает сохранение из слота и переходит на уровень.
    /// </summary>
    /// <param name="slotIndex">Индекс слота (0-19).</param>
    public void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return;
        
        GameObject slot = _slots[slotIndex];
        if (slot != null) slot.SetActive(false);
        
        if (_foldersSaveMenu != null) _foldersSaveMenu.SetActive(false);
        
        if (_rootTransform != null)
        {
            Transform levelTransform = _rootTransform.Find("LevelMenu");
            if (levelTransform != null)
            {
                levelTransform.gameObject.SetActive(true);
            }
        }
    }
    // ========================================================================
    // ON DRAW GIZMOS: ВИЗУАЛИЗАЦИЯ В РЕДАКТОРЕ
    // ========================================================================

    private void OnDrawGizmos()
    {
        if (!_isInitialized) return;

        Gizmos.color = Color.yellow;
        
        foreach (var slot in _slots)
        {
            if (slot != null && slot.transform != null)
            {
                // Рисуем проволочный куб вокруг каждого слота
                Vector3 pos = slot.transform.position;
                Gizmos.DrawWireCube(pos, slotSize);
                
                // Рисуем линию к центру
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, pos);
                
                // Рисуем номер файла
                Gizmos.color = Color.white;
                string fileName = slot.name; // "Файл 1", "Файл 2", etc.
                // Примечание: OnDrawGizmos не поддерживает текст, 
                // но можно использовать Editor-only рисование
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Подсвечиваем область сетки
        float totalWidth = slotsPerRow * slotSize.x + (slotsPerRow - 1) * slotSpacingX;
        float totalHeight = rowsCount * slotSize.y + (rowsCount - 1) * slotSpacingY;
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(totalWidth, totalHeight, 1f));
    }

    // ========================================================================
    // УТИЛИТЫ
    // ========================================================================

    /// <summary>
    /// Рассчитывает размер слота на основе реального размера Canvas.
    /// Перезаписывает поле slotSize для использования в CreateSingleSlot.
    /// </summary>
    private void CalculateSlotSizeFromCanvas()
    {
        if (_canvas == null) return;

        RectTransform mainCanvasRT = _canvas.GetComponent<RectTransform>();
        if (mainCanvasRT == null) return;

        float canvasWidth = mainCanvasRT.rect.width;
        float canvasHeight = mainCanvasRT.rect.height;

        float slotWidth = (canvasWidth - (slotsPerRow - 1) * slotSpacingX) / slotsPerRow;
        float slotHeight = (canvasHeight - (rowsCount - 1) * slotSpacingY) / rowsCount;

        // Квадратные слоты — берём минимальное значение, чтобы вписались по обеим осям
        float slotSizeValue = Mathf.Min(slotWidth, slotHeight);
        slotSize = new Vector3(slotSizeValue, slotSizeValue, 1f);

        Debug.Log($"[SaveSlotSystem] 📐 Canvas: {canvasWidth:F1}x{canvasHeight:F1} | Слот: {slotSizeValue:F1}x{slotSizeValue:F1}");
    }

    /// <summary>
    /// Возвращает список созданных слотов.
    /// </summary>
    public List<GameObject> GetSlots()
    {
        return new List<GameObject>(_slots);
    }

    /// <summary>
    /// Возвращает слот по индексу.
    /// </summary>
    public GameObject GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Count)
        {
            Debug.LogError($"[SaveSlotSystem] Invalid slot index: {index}");
            return null;
        }
        return _slots[index];
    }

    /// <summary>
    /// Возвращает количество созданных слотов.
    /// </summary>
    public int GetSlotCount()
    {
        return _slots.Count;
    }

    /// <summary>
    /// Обновляет DateTimeText для указанного слота.
    /// Вызывать при автосохранении или ручном сохранении.
    /// </summary>
    /// <param name="slotIndex">Индекс слота (0-19).</param>
    /// <param name="dateTime">Дата и время в формате dd.MM.yyyy HH:mm:ss.</param>
    public void UpdateDateTimeText(int slotIndex, string dateTime)
    {
        if (slotIndex < 0 || slotIndex >= _dateTimeTexts.Count)
        {
            Debug.LogError($"[SaveSlotSystem] Invalid slot index for DateTimeText: {slotIndex}");
            return;
        }

        if (_dateTimeTexts[slotIndex] == null)
        {
            Debug.LogError($"[SaveSlotSystem] DateTimeText для слота {slotIndex} — null!");
            return;
        }

        _dateTimeTexts[slotIndex].text = dateTime;
        Debug.Log($"[SaveSlotSystem] 🕐 DateTimeText слота {slotIndex} обновлён: {dateTime}");
    }

    /// <summary>
    /// Пересоздаёт все слоты (перезапускает систему).
    /// </summary>
    [ContextMenu("🔄 Respawn All Slots")]
    public void RespawnAllSlots()
    {
        Debug.Log("[SaveSlotSystem] Пересоздание всех слотов...");
        
        // Удаляем старые слоты с помощью DestroyImmediate (Editor context)
        foreach (var slot in _slots)
        {
            if (slot != null)
            {
                DestroyImmediate(slot.gameObject, true);
            }
        }
        _slots.Clear();
        _dateTimeTexts.Clear();

        // Пересоздаём
        SpawnAllSlots();
    }

    /// <summary>
    /// Очистка при уничтожении.
    /// </summary>
    private void OnDestroy()
    {
        StopAllCoroutines();
        _slots.Clear();
        _dateTimeTexts.Clear();
        _isInitialized = false;
    }

    // ============================================================================
    // КОМПОНЕНТ ДЛЯ ОТРИСОВКИ GIZMO В РЕДАКТОРЕ
    // ============================================================================

    /// <summary>
    /// Добавляется к каждому слоту для отрисовки Gizmo в редакторе.
    /// Не влияет на производительность в билде.
    /// </summary>
    public class SlotGizmoDrawer : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            // Рисуем проволочный куб вокруг слота
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            // Подсвечиваем выбранный слот
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
        }
    }

    // ========================================================================
    // ИСПРАВЛЕНИЕ МАСШТАБА И ИЕРАРХИИ
    // ========================================================================

    /// <summary>
    /// Сбрасывает масштаб FoldersSaveMenu, удаляет всех потомков, создаёт 20 файлов.
    /// Использовать в Editor контексте или перед запуском игры.
    /// </summary>
    [ContextMenu("🔧 Fix FoldersSaveMenu")]
    public void FixFoldersSaveMenu()
    {
        // Автоинициализация если не сработал Awake()
        if (!_isInitialized || _canvas == null || _slotsParent == null)
        {
            Debug.Log("[FixFoldersSaveMenu] Автоинициализация...");
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasGO = new GameObject("SaveSlotsCanvas");
                _canvas = canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.worldCamera = null;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 1000;
                canvasGO.AddComponent<GraphicRaycaster>();
                canvasGO.AddComponent<CanvasScaler>();
            }
            RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
            if (canvasRT != null)
            {
                canvasRT.anchorMin = Vector2.zero;
                canvasRT.anchorMax = Vector2.one;
                canvasRT.sizeDelta = Vector2.zero;
                canvasRT.anchoredPosition = Vector2.zero;
            }
            
            // Ищем FoldersSavePanel — ТОЧНО ТАК ЖЕ, КАК В Awake()
            Transform foldersSavePanel = transform.Find("FoldersSavePanel");
            if (foldersSavePanel == null)
            {
                foldersSavePanel = transform.root.Find("FoldersSavePanel");
            }
            if (foldersSavePanel == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] FoldersSavePanel НЕ НАЙДЕН!");
            }
            _slotsParent = foldersSavePanel != null ? foldersSavePanel : transform;
            Debug.Log("[FixFoldersSaveMenu] ✅ Найден FoldersSavePanel: " + (_slotsParent != null ? _slotsParent.name : "null"));
            
            _isInitialized = true;
        }
        
        Debug.Log("==========================================================");
        Debug.Log("[FixFoldersSaveMenu] 🔧 НАЧАЛО ИСПРАВЛЕНИЯ FoldersSaveMenu");
        Debug.Log("[FixFoldersSaveMenu] Текущий localScale родителя: " + _slotsParent.localScale);
        Debug.Log("[FixFoldersSaveMenu] _slotsParent.childCount перед очисткой: " + _slotsParent.childCount);

        // 1. СБРОС МАСШТАБА РОДИТЕЛЯ
        if (_slotsParent.localScale != Vector3.one)
        {
            _slotsParent.localScale = Vector3.one;
            Debug.Log("[FixFoldersSaveMenu] ✅ Масштаб родителя сброшен до Vector3.one");
        }
        else
        {
            Debug.Log("[FixFoldersSaveMenu] ⚠️ Масштаб родителя уже Vector3.one");
        }

        // 2. УДАЛЕНИЕ ТОЛЬКО СЛОТОВ (Folder 1-20), оставляем FoldersSavePanel и BackToMainMenu
        Debug.Log("[FixFoldersSaveMenu] 📋 Очистка только слотов Folder 1-20...");
        for (int i = _slotsParent.childCount - 1; i >= 0; i--)
        {
            Transform child = _slotsParent.GetChild(i);
            if (child.name.StartsWith("Folder ", System.StringComparison.OrdinalIgnoreCase))
            {
                DestroyImmediate(child.gameObject);
            }
        }
        Debug.Log("[FixFoldersSaveMenu] ✅ Слоты удалены, FoldersSavePanel и BackToMainMenu сохранены");

        // 3. СОЗДАНИЕ 20 ФАЙЛОВ
        _slots.Clear();
        _dateTimeTexts.Clear();
        Debug.Log("[FixFoldersSaveMenu] 📦 Создание 20 файлов...");

        for (int i = 0; i < totalSlots; i++)
        {
            string folderName = "Folder " + (i + 1);

            GameObject folderGO = new GameObject(folderName);
            folderGO.transform.SetParent(_slotsParent, true);

            // 🔴 ФИКСИРОВАННАЯ ПОЗИЦИЯ В СЕТКЕ 5x4
            int row = i / slotsPerRow;
            int col = i % slotsPerRow;
            
            // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РАЗМЕРА PARENT
            RectTransform parentRT = _slotsParent as RectTransform;
            float parentWidth = parentRT != null ? parentRT.rect.width : 800f;
            float parentHeight = parentRT != null ? parentRT.rect.height : 600f;
            
            float slotWidth = (parentWidth - (slotsPerRow - 1) * slotSpacingX) / slotsPerRow;
            float slotHeight = (parentHeight - (rowsCount - 1) * slotSpacingY) / rowsCount;
            float currentSlotSizeX = Mathf.Min(Mathf.Max(slotWidth, 50f), 300f);
            float currentSlotSizeY = Mathf.Min(Mathf.Max(slotHeight, 50f), 150f);
            
            // Инвертируем row: 0 = верхний ряд, 3 = нижний ряд
            int displayRow = rowsCount - 1 - row;
            
            float totalGridWidth = slotsPerRow * currentSlotSizeX + (slotsPerRow - 1) * slotSpacingX;
            float totalGridHeight = rowsCount * currentSlotSizeY + (rowsCount - 1) * slotSpacingY;
            
            float centerX = -totalGridWidth / 2f + (col * (currentSlotSizeX + slotSpacingX) + currentSlotSizeX / 2f);
            float centerY = totalGridHeight / 2f - (displayRow * (currentSlotSizeY + slotSpacingY) + currentSlotSizeY / 2f);
            
            folderGO.transform.localScale = Vector3.one;
            folderGO.transform.localRotation = Quaternion.identity;

            // RectTransform для folderGO
            RectTransform folderRT = folderGO.GetComponent<RectTransform>();
            if (folderRT == null) folderRT = folderGO.AddComponent<RectTransform>();
            folderRT.sizeDelta = new Vector2(currentSlotSizeX, currentSlotSizeY);
            folderRT.anchorMin = new Vector2(0.5f, 0.5f);
            folderRT.anchorMax = new Vector2(0.5f, 0.5f);
            folderRT.anchoredPosition = new Vector2(centerX, -centerY);

            // Image (чёрный)
            Image img = folderGO.AddComponent<Image>();
            if (img == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Не добавился Image для: " + folderName);
                continue;
            }
            img.enabled = true;
            img.color = Color.black;
            img.raycastTarget = true;

            // Button
            Button btn = folderGO.AddComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Не добавился Button для: " + folderName);
                continue;
            }
            btn.enabled = true;
            
            // Принудительно включаем Image
            Image imgComp2 = folderGO.GetComponent<Image>();
            if (imgComp2 != null)
            {
                imgComp2.enabled = true;
                imgComp2.raycastTarget = true;
            }
            
            int fixSlotIndex = i;
            btn.onClick.AddListener(() => {
                Debug.Log($"[FixFoldersSaveMenu] 🖱️ Click slot {fixSlotIndex}");
                OnSlotClicked(fixSlotIndex);
            });

            // Текст
            GameObject textGO = new GameObject("FileNameText");
            textGO.transform.SetParent(folderGO.transform, false);
            textGO.transform.localPosition = Vector3.zero;
            textGO.transform.localScale = Vector3.one;
            textGO.transform.localRotation = Quaternion.identity;

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(slotSize.x - 20f, 50f);
            textRT.anchoredPosition = new Vector2(0f, 10f);

            TMPro.TextMeshProUGUI textMesh = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            if (textMesh == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Не добавился Text для: " + folderName);
                continue;
            }
            textMesh.enabled = true;
            textMesh.text = "Файл " + (i + 1);
            textMesh.fontSize = 36;
            textMesh.color = Color.white;
            textMesh.alignment = TMPro.TextAlignmentOptions.Center;

            // Добавляем DateTimeText
            GameObject dateTimeGO = new GameObject("DateTimeText");
            dateTimeGO.transform.SetParent(folderGO.transform, false);
            dateTimeGO.transform.localPosition = Vector3.zero;
            dateTimeGO.transform.localScale = Vector3.one;
            dateTimeGO.transform.localRotation = Quaternion.identity;

            RectTransform dateTimeRT = dateTimeGO.AddComponent<RectTransform>();
            dateTimeRT.anchorMin = new Vector2(0.5f, 0.5f);
            dateTimeRT.anchorMax = new Vector2(0.5f, 0.5f);
            dateTimeRT.sizeDelta = new Vector2(slotSize.x - 16f, 35f);
            dateTimeRT.anchoredPosition = new Vector2(0f, -12f);

            TMPro.TextMeshProUGUI dateTimeText = dateTimeGO.AddComponent<TMPro.TextMeshProUGUI>();
            if (dateTimeText == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Не добавился DateTimeText для: " + folderName);
                continue;
            }
            dateTimeText.enabled = true;
            dateTimeText.text = "";
            dateTimeText.fontSize = 36;
            dateTimeText.color = Color.white;
            dateTimeText.alignment = TMPro.TextAlignmentOptions.Center;

            // Сохраняем ссылку на DateTimeText
            _dateTimeTexts.Add(dateTimeText);

            // Gizmo
            folderGO.AddComponent<SlotGizmoDrawer>();

            _slots.Add(folderGO);

            // Логирование для отладки
            Debug.Log("[FixFoldersSaveMenu] ✅ " + folderName +
                " | pos=" + folderGO.transform.localPosition +
                " | scale=" + folderGO.transform.localScale +
                " | parent=" + folderGO.transform.parent.name);
        }

        // 4. ФИНАЛЬНАЯ ПРОВЕРКА
        Debug.Log("[FixFoldersSaveMenu] ============================================");
        Debug.Log("[FixFoldersSaveMenu] Финальная проверка:");
        Debug.Log("[FixFoldersSaveMenu]   - _slotsParent.childCount: " + _slotsParent.childCount);
        Debug.Log("[FixFoldersSaveMenu]   - _slots.Count: " + _slots.Count);
        Debug.Log("[FixFoldersSaveMenu]   - parent localScale: " + _slotsParent.localScale);

        Debug.Assert(_slots.Count == 20, "Slot list mismatch!");
        Debug.Assert(_slotsParent.localScale == Vector3.one, "Parent scale broken!");

        for (int i = 0; i < _slots.Count; i++)
        {
            var s = _slots[i];
            if (s == null)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Слот " + i + " — null!");
            }
            else if (s.transform.localScale != Vector3.one)
            {
                Debug.LogError("[FixFoldersSaveMenu] [ERROR] Слот " + i + " scale = " + s.transform.localScale + " (ожидалось Vector3.one)");
            }
        }

        Debug.Log("[FixFoldersSaveMenu] ✅ ИСПРАВЛЕНИЕ ЗАВЕРШЕНО УСПЕШНО");
        Debug.Log("==========================================================");
    }

    // ========================================================================
    // ДОБАВИТЬ ЭТОТ МЕТОД В SaveSlotSystem
    // ========================================================================

    [ContextMenu("🔥 Очистить и пересоздать слоты")]
    public void CleanupAndRespawnSlots()
    {
        // Автоинициализация если не сработал Awake()
        if (!_isInitialized || _canvas == null || _slotsParent == null)
        {
            Debug.Log("[SaveSlotSystem] Автоинициализация в CleanupAndRespawnSlots...");
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasGO = new GameObject("SaveSlotsCanvas");
                _canvas = canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.worldCamera = null;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 1000;
                canvasGO.AddComponent<GraphicRaycaster>();
                canvasGO.AddComponent<CanvasScaler>();
            }
            RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
            if (canvasRT != null)
            {
                canvasRT.anchorMin = Vector2.zero;
                canvasRT.anchorMax = Vector2.one;
                canvasRT.sizeDelta = Vector2.zero;
                canvasRT.anchoredPosition = Vector2.zero;
            }
            
            // Ищем FoldersSavePanel — ТОЧНО ТАК ЖЕ, КАК В Awake()
            Transform foldersSavePanel = transform.Find("FoldersSavePanel");
            if (foldersSavePanel == null)
            {
                foldersSavePanel = transform.root.Find("FoldersSavePanel");
            }
            if (foldersSavePanel == null)
            {
                Debug.LogError("[SaveSlotSystem] FoldersSavePanel НЕ НАЙДЕН в CleanupAndRespawnSlots!");
            }
            _slotsParent = foldersSavePanel != null ? foldersSavePanel : transform;
            Debug.Log("[SaveSlotSystem] ✅ Найден FoldersSavePanel: " + (_slotsParent != null ? _slotsParent.name : "null"));
            
            _isInitialized = true;
        }
        
        Debug.Log("[SaveSlotSystem] 🔥 НАЧАТО ПЕРЕСОЗДАНИЕ СЛОТОВ...");
        
        // НЕ удаляем GameObject через DestroyImmediate — просто пересоздаём слоты заново
        // Старые GameObject останутся, но мы будем использовать новые из _slots
        Debug.Log($"[SaveSlotSystem] 📋 _slotsParent.childCount: {_slotsParent.childCount}");
        
        // 2. СОЗДАНИЕ 20 НОВЫХ СЛОТОВ
        Debug.Log("[SaveSlotSystem] 📦 СОЗДАНИЕ 20 НОВЫХ ФАЙЛОВ...");
        
        _slots.Clear();
        _dateTimeTexts.Clear();
        
        for (int i = 0; i < totalSlots; i++)
        {
            string folderName = $"Folder {i + 1}";
            
            // Создаём GameObject
            GameObject folderGO = new GameObject(folderName);
            folderGO.transform.SetParent(_slotsParent, true);
            
            // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РАЗМЕРА PARENT
            RectTransform parentRT = _slotsParent as RectTransform;
            float parentWidth = parentRT != null ? parentRT.rect.width : 800f;
            float parentHeight = parentRT != null ? parentRT.rect.height : 600f;
            
            float slotWidth = (parentWidth - (slotsPerRow - 1) * slotSpacingX) / slotsPerRow;
            float slotHeight = (parentHeight - (rowsCount - 1) * slotSpacingY) / rowsCount;
            float currentSlotSizeX = Mathf.Min(Mathf.Max(slotWidth, 50f), 300f);
            float currentSlotSizeY = Mathf.Min(Mathf.Max(slotHeight, 50f), 150f);
            
            // 🔴 ФИКСИРОВАННАЯ ПОЗИЦИЯ В СЕТКЕ 5x4 (сверху вниз по возрастанию)
            int row = i / slotsPerRow;
            int col = i % slotsPerRow;
            
            // Инвертируем row: 0 = верхний ряд, 3 = нижний ряд
            int displayRow = rowsCount - 1 - row;
            
            float totalGridWidth = slotsPerRow * currentSlotSizeX + (slotsPerRow - 1) * slotSpacingX;
            float totalGridHeight = rowsCount * currentSlotSizeY + (rowsCount - 1) * slotSpacingY;
            
            float centerX = -totalGridWidth / 2f + (col * (currentSlotSizeX + slotSpacingX) + currentSlotSizeX / 2f);
            float centerY = totalGridHeight / 2f - (displayRow * (currentSlotSizeY + slotSpacingY) + currentSlotSizeY / 2f);
            
            // 🔴 Масштаб 1, размеры через RectTransform
            folderGO.transform.localScale = Vector3.one;
            folderGO.transform.localRotation = Quaternion.identity;
            
            // RectTransform для folderGO
            RectTransform folderRT = folderGO.GetComponent<RectTransform>();
            if (folderRT == null) folderRT = folderGO.AddComponent<RectTransform>();
            folderRT.sizeDelta = new Vector2(currentSlotSizeX, currentSlotSizeY);
            folderRT.anchorMin = new Vector2(0.5f, 0.5f);
            folderRT.anchorMax = new Vector2(0.5f, 0.5f);
            folderRT.anchoredPosition = new Vector2(centerX, -centerY);
            
            // Добавляем Image для видимости (UISprite форма)
            Image img = folderGO.AddComponent<Image>();
            if (img == null)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] '{folderName}' не может добавить Image!");
                continue;
            }
            
            img.enabled = true;
            img.color = Color.black; // Чёрный цвет
            img.raycastTarget = true;
            
            // Добавляем Button
            Button btn = folderGO.AddComponent<Button>();
            if (btn == null)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] '{folderName}' не может добавить Button!");
                continue;
            }
            
            btn.enabled = true;
            
            // Принудительно включаем Image
            Image imgComp3 = folderGO.GetComponent<Image>();
            if (imgComp3 != null)
            {
                imgComp3.enabled = true;
                imgComp3.raycastTarget = true;
            }
            
            int cleanupSlotIndex = i;
            btn.onClick.AddListener(() => {
                Debug.Log($"[SaveSlotSystem] 🖱️ Click slot {cleanupSlotIndex}");
                OnSlotClicked(cleanupSlotIndex);
            });
            
            // Добавляем текст
            GameObject textGO = new GameObject("FileNameText");
            textGO.transform.SetParent(folderGO.transform, false);
            textGO.transform.localPosition = Vector3.zero;
            textGO.transform.localScale = Vector3.one;
            
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(slotSize.x - 20f, 50f);
            textRT.anchoredPosition = new Vector2(0f, 10f);
            
            TMPro.TextMeshProUGUI textMesh = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            if (textMesh == null)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] '{folderName}' не может добавить Text!");
                continue;
            }
            
            textMesh.enabled = true;
            textMesh.text = $"Файл {i + 1}";
            textMesh.fontSize = 36;
            textMesh.color = Color.white;
            textMesh.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Добавляем DateTimeText
            GameObject dateTimeGO = new GameObject("DateTimeText");
            dateTimeGO.transform.SetParent(folderGO.transform, false);
            dateTimeGO.transform.localPosition = Vector3.zero;
            dateTimeGO.transform.localScale = Vector3.one;
            dateTimeGO.transform.localRotation = Quaternion.identity;
            
            RectTransform dateTimeRT = dateTimeGO.AddComponent<RectTransform>();
            dateTimeRT.anchorMin = new Vector2(0.5f, 0.5f);
            dateTimeRT.anchorMax = new Vector2(0.5f, 0.5f);
            dateTimeRT.sizeDelta = new Vector2(slotSize.x - 16f, 35f);
            dateTimeRT.anchoredPosition = new Vector2(0f, -12f);
            
            TMPro.TextMeshProUGUI dateTimeText = dateTimeGO.AddComponent<TMPro.TextMeshProUGUI>();
            if (dateTimeText == null)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] '{folderName}' не может добавить DateTimeText!");
                continue;
            }
            
            dateTimeText.enabled = true;
            dateTimeText.text = "";
            dateTimeText.fontSize = 36;
            dateTimeText.color = Color.white;
            dateTimeText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Сохраняем ссылку на DateTimeText
            _dateTimeTexts.Add(dateTimeText);
            
            // Добавляем Gizmo
            folderGO.AddComponent<SlotGizmoDrawer>();
            
            // Сохраняем в список
            _slots.Add(folderGO);
            
            Debug.Log($"[SaveSlotSystem] ✅ Создан Folder: '{folderName}'");
        }
        
        // 4. ФИНАЛЬНАЯ ПРОВЕРКА
        Debug.Log("[SaveSlotSystem] 📊 ФИНАЛЬНЫЙ ОТЧЁТ:");
        Debug.Log($"[SaveSlotSystem] Total slots: {totalSlots}");
        Debug.Log($"[SaveSlotSystem] Created slots: {_slots.Count}");
        Debug.Log($"[SaveSlotSystem] _slotsParent.childCount: {_slotsParent.childCount}");
        
        if (_slots.Count != totalSlots)
        {
            Debug.LogError($"[SaveSlotSystem] [ERROR] Создано {_slots.Count} из {totalSlots} слотов!");
#if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot("slot_error.png");
#endif
        }
        else
        {
            Debug.Log("[SaveSlotSystem] ✅ ВСЕ 20 СЛОТОВ УСПЕШНО СОЗДАНЫ!");
        }
        
        // Валидация: все слоты в списке
        Debug.Assert(_slots.Count == 20, "Wrong slot count!");
        
        // 5. ВИЗУАЛИЗАЦИЯ GIZMOS
        Debug.Log("[SaveSlotSystem] 🎨 Визуализация Gizmos активирована");
    }

    // ========================================================================
    // ПЕРЕХОД К УРОВНЮ ПО ИНДЕКСУ (из меню уровней)
    // ========================================================================

    /// <summary>
    /// Переходит на уровень по индексу через LevelMenuManager.
    /// </summary>
    public void StartLevelFromSave(int levelIndex)
    {
        Debug.Log($"[SaveSlotSystem] 🚀 Переход на уровень {levelIndex + 1} через LevelMenuManager...");
        
        // Ищем LevelMenuManager
        var levelMenuManager = FindFirstObjectByType<LevelMenuManager>();
        
        if (levelMenuManager != null)
        {
            // Вызываем OnLevelClicked
            levelMenuManager.OnLevelClicked(levelIndex);
            Debug.Log($"[SaveSlotSystem] ✅ Уровень {levelIndex + 1} загружается через OnLevelClicked...");
        }
        else
        {
            Debug.LogError("[SaveSlotSystem] LevelMenuManager не найден на сцене!");
        }
    }

    /// <summary>
    /// Загружает уровень по индексу.
    /// </summary>
}
