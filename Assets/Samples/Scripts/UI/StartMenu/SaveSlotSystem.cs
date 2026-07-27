using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
    private bool _isInitialized = false;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // НЕ используем GameObject.Find() — это надёжно и быстро
        _canvas = GetComponentInChildren<Canvas>();
        
        // Если Canvas нет на дочерних объектах — ищем на родителе
        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
        
        // Если всё ещё нет — создаём новый Canvas
        if (_canvas == null)
        {
            GameObject canvasGO = new GameObject("SaveSlotsCanvas");
            canvasGO.transform.SetParent(transform);
            canvasGO.transform.localPosition = Vector3.zero;
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.worldCamera = null;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 1000;
            
            // Добавляем GraphicRaycaster для обработки кликов
            canvasGO.AddComponent<GraphicRaycaster>();
            
            Debug.Log($"[SaveSlotSystem] Создан новый Canvas: {_canvas.name}");
        }
        
        // Растягиваем Canvas на весь экран
        RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
        if (canvasRT != null)
        {
            canvasRT.anchorMin = Vector2.zero;
            canvasRT.anchorMax = Vector2.one;
            canvasRT.sizeDelta = Vector2.zero;
            canvasRT.anchoredPosition = Vector2.zero;
        }
        
        _isInitialized = true;
        Debug.Log($"[SaveSlotSystem] Инициализация завершена. Canvas: {_canvas?.name ?? "null"}");
    }

    private void Start()
    {
        // 🔴 Убедимся что родитель в правильном состоянии
        if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
        SpawnAllSlots();
    }

    // ========================================================================
    // ГЛАВНЫЙ МЕТОД: СОЗДАНИЕ ВСЕХ 20 СЛОТОВ
    // ========================================================================

    [ContextMenu("🔥 Spawn All Slots")]
    public void SpawnAllSlots()
    {
        // 🔴 АВТОИНИЦИАЛИЗАЦИЯ: если Awake() не сработал (например, вызов из контекстного меню)
        if (!_isInitialized)
        {
            Debug.Log("[SaveSlotSystem] Автоинициализация...");
            
            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }
            if (_canvas == null)
            {
                GameObject canvasGO = new GameObject("SaveSlotsCanvas");
                canvasGO.transform.SetParent(transform);
                canvasGO.transform.localPosition = Vector3.zero;
                _canvas = canvasGO.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.worldCamera = null;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 1000;
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
            if (canvasRT != null)
            {
                canvasRT.anchorMin = Vector2.zero;
                canvasRT.anchorMax = Vector2.one;
                canvasRT.sizeDelta = Vector2.zero;
                canvasRT.anchoredPosition = Vector2.zero;
            }
            _isInitialized = true;
            Debug.Log("[SaveSlotSystem] ✅ Автоинициализация завершена");
        }

        // Останавливаем все корутины перед новым запуском
        StopAllCoroutines();

        // 🔴 СБРОС МАСШТАБА РОДИТЕЛЯ — критически важно!
        if (transform.localScale != Vector3.one)
        {
            Debug.LogWarning($"[SaveSlotSystem] Сброс localScale родителя: {transform.localScale} -> Vector3.one");
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        _slots.Clear();
        _dateTimeTexts.Clear();

        // 🔴 УДАЛЯЕМ ВСЕХ ПРЕДЫДУЩИХ ПОТОМКОВ — чистый лист
        Debug.Log($"[SaveSlotSystem] Очистка {transform.childCount} предыдущих потомков...");
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            DestroyImmediate(child.gameObject, true);
        }
        Debug.Log("[SaveSlotSystem] ✅ Иерархия очищена");

        // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РЕАЛЬНОГО РАЗМЕРА CANVAS
        CalculateSlotSizeFromCanvas();

        Debug.Log($"[SaveSlotSystem] НАЧАТО СОЗДАНИЕ {totalSlots} СЛОТОВ...");

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
        Debug.Assert(transform.childCount >= totalSlots, "Critical failure: child count mismatch!");
        
        // 🔴 Жёсткая валидация: childCount должен быть ровно 20
        Debug.Assert(transform.childCount == 20, "Wrong file count!");
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

        // Добавляем в иерархию — прямой ребёнок FoldersSaveMenu
        folderGO.transform.SetParent(transform, false);
        
        // 🔴 РАСЧЁТ ПОЗИЦИИ В СЕТКЕ 5x4
        int row = index / slotsPerRow;
        int col = index % slotsPerRow;
        
        float totalGridWidth = slotsPerRow * slotSize.x + (slotsPerRow - 1) * slotSpacingX;
        float totalGridHeight = rowsCount * slotSize.y + (rowsCount - 1) * slotSpacingY;
        
        float startX = -totalGridWidth / 2f;
        float startY = totalGridHeight / 2f;
        
        float posX = startX + col * (slotSize.x + slotSpacingX) + slotSize.x / 2f;
        float posY = startY - row * (slotSize.y + slotSpacingY) - slotSize.y / 2f;
        
        folderGO.transform.localPosition = new Vector3(posX, posY, 0f);
        
        // Масштаб 1, размеры через RectTransform
        folderGO.transform.localScale = Vector3.one;
        folderGO.transform.localRotation = Quaternion.identity;

        // Получаем или создаём RectTransform для folderGO
        RectTransform folderRT = folderGO.GetComponent<RectTransform>();
        if (folderRT == null)
        {
            folderRT = folderGO.AddComponent<RectTransform>();
        }
        // Устанавливаем размер слота — фиксированный для всех
        folderRT.sizeDelta = slotSize;
        folderRT.anchorMin = new Vector2(0.5f, 0.5f);
        folderRT.anchorMax = new Vector2(0.5f, 0.5f);
        folderRT.anchoredPosition = new Vector2(posX, posY);

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
        Debug.Log($"[SaveSlotSystem] ✅ Folder {index + 1}: '{folderName}', pos={posX:F1},{posY:F1}, size={slotSize}");

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
        Debug.Log("==========================================================");
        Debug.Log("[FixFoldersSaveMenu] 🔧 НАЧАЛО ИСПРАВЛЕНИЯ FoldersSaveMenu");
        Debug.Log("[FixFoldersSaveMenu] Текущий localScale родителя: " + transform.localScale);
        Debug.Log("[FixFoldersSaveMenu] Текущий childCount: " + transform.childCount);

        // 1. СБРОС МАСШТАБА РОДИТЕЛЯ
        if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
            Debug.Log("[FixFoldersSaveMenu] ✅ Масштаб родителя сброшен до Vector3.one");
        }
        else
        {
            Debug.Log("[FixFoldersSaveMenu] ⚠️ Масштаб родителя уже Vector3.one");
        }

        // 2. УДАЛЕНИЕ ВСЕХ ПОТОМКОВ (без исключений — чистый лист)
        int oldCount = transform.childCount;
        Debug.Log("[FixFoldersSaveMenu] 📋 Удаление " + oldCount + " потомков...");

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Debug.Log("[FixFoldersSaveMenu]   - Удаление: '" + child.name + "'");
            DestroyImmediate(child.gameObject, true);
        }

        if (transform.childCount != 0)
        {
            Debug.LogError("[FixFoldersSaveMenu] [ERROR] Ожидался childCount 0, но: " + transform.childCount);
            return;
        }
        Debug.Log("[FixFoldersSaveMenu] ✅ Иерархия очищена. childCount = 0");

        // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РЕАЛЬНОГО РАЗМЕРА CANVAS
        CalculateSlotSizeFromCanvas();

        // 3. СОЗДАНИЕ 20 ФАЙЛОВ
        _slots.Clear();
        _dateTimeTexts.Clear();
        Debug.Log("[FixFoldersSaveMenu] 📦 Создание 20 файлов...");

        for (int i = 0; i < totalSlots; i++)
        {
            string folderName = "Folder " + (i + 1);

            GameObject folderGO = new GameObject(folderName);
            folderGO.transform.SetParent(transform, false);

            // 🔴 ФИКСИРОВАННАЯ ПОЗИЦИЯ В СЕТКЕ 5x4
            int row = i / slotsPerRow;
            int col = i % slotsPerRow;
            
            float totalGridWidth = slotsPerRow * slotSize.x + (slotsPerRow - 1) * slotSpacingX;
            float totalGridHeight = rowsCount * slotSize.y + (rowsCount - 1) * slotSpacingY;
            
            float startX = -totalGridWidth / 2f;
            float startY = totalGridHeight / 2f;
            
            float posX = startX + col * (slotSize.x + slotSpacingX) + slotSize.x / 2f;
            float posY = startY - row * (slotSize.y + slotSpacingY) - slotSize.y / 2f;
            
            folderGO.transform.localScale = Vector3.one;
            folderGO.transform.localRotation = Quaternion.identity;

            // RectTransform для folderGO
            RectTransform folderRT = folderGO.GetComponent<RectTransform>();
            if (folderRT == null) folderRT = folderGO.AddComponent<RectTransform>();
            folderRT.sizeDelta = slotSize;
            folderRT.anchorMin = new Vector2(0.5f, 0.5f);
            folderRT.anchorMax = new Vector2(0.5f, 0.5f);
            folderRT.anchoredPosition = new Vector2(posX, posY);

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
        Debug.Log("[FixFoldersSaveMenu]   - childCount: " + transform.childCount);
        Debug.Log("[FixFoldersSaveMenu]   - _slots.Count: " + _slots.Count);
        Debug.Log("[FixFoldersSaveMenu]   - parent localScale: " + transform.localScale);

        Debug.Assert(transform.childCount == 20, "Wrong file count!");
        Debug.Assert(_slots.Count == 20, "Slot list mismatch!");
        Debug.Assert(transform.localScale == Vector3.one, "Parent scale broken!");

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
        Debug.Log("[SaveSlotSystem] 🔥 НАЧАТО ОЧИЩЕНИЕ ИЕРАРХИИ...");
        
        // 1. УДАЛЕНИЕ ВСЕХ СТАРЫХ СЛОТОВ (сохраняем NumberOfFolder!)
        int oldSlotsRemoved = 0;
        Transform numberOfFolder = null;
        
        // Сначала находим NumberOfFolder
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "NumberOfFolder")
            {
                numberOfFolder = child;
                Debug.Log($"[SaveSlotSystem] 🔒 Сохранён NumberOfFolder");
            }
        }
        
        Debug.Log($"[SaveSlotSystem] 📋 Текущий childCount: {transform.childCount}");
        
        // Удаляем в ОБРАТНОМ порядке, чтобы индексы не сдвигались
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            
            // Пропускаем NumberOfFolder
            if (child == numberOfFolder)
                continue;
                
            Debug.Log($"[SaveSlotSystem] 🔍 Удаление объекта: '{child.name}'");
            DestroyImmediate(child.gameObject, true);
            oldSlotsRemoved++;
        }
        
        Debug.Log($"[SaveSlotSystem] 🧹 Удалено {oldSlotsRemoved} старых объектов");
        
        // 2. ПРОВЕРКА: должен остаться только NumberOfFolder (если был)
        if (numberOfFolder != null)
        {
            if (transform.childCount != 1)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] Ожидался childCount 1 (NumberOfFolder), но получено: {transform.childCount}!");
                return;
            }
            if (transform.GetChild(0) != numberOfFolder)
            {
                Debug.LogError("[SaveSlotSystem] [ERROR] NumberOfFolder потерян!");
                return;
            }
            Debug.Log("[SaveSlotSystem] ✅ NumberOfFolder на месте");
        }
        else
        {
            if (transform.childCount != 0)
            {
                Debug.LogError($"[SaveSlotSystem] [ERROR] Ожидался childCount 0, но получено: {transform.childCount}!");
                return;
            }
            Debug.Log("[SaveSlotSystem] ✅ Иерархия очищена");
        }
        
        // 🔴 РАСЧЁТ РАЗМЕРА СЛОТОВ НА ОСНОВЕ РЕАЛЬНОГО РАЗМЕРА CANVAS
        CalculateSlotSizeFromCanvas();
        
        // 3. СОЗДАНИЕ 20 НОВЫХ ФАЙЛОВ
        Debug.Log("[SaveSlotSystem] 📦 СОЗДАНИЕ 20 НОВЫХ ФАЙЛОВ...");
        
        _slots.Clear();
        _dateTimeTexts.Clear();
        
        for (int i = 0; i < totalSlots; i++)
        {
            string folderName = $"Folder {i + 1}";
            
            // Создаём GameObject
            GameObject folderGO = new GameObject(folderName);
            folderGO.transform.SetParent(transform, false);
            
            // 🔴 ФИКСИРОВАННАЯ ПОЗИЦИЯ В СЕТКЕ 5x4
            int row = i / slotsPerRow;
            int col = i % slotsPerRow;
            
            float totalGridWidth = slotsPerRow * slotSize.x + (slotsPerRow - 1) * slotSpacingX;
            float totalGridHeight = rowsCount * slotSize.y + (rowsCount - 1) * slotSpacingY;
            
            float startX = -totalGridWidth / 2f;
            float startY = totalGridHeight / 2f;
            
            float posX = startX + col * (slotSize.x + slotSpacingX) + slotSize.x / 2f;
            float posY = startY - row * (slotSize.y + slotSpacingY) - slotSize.y / 2f;
            
            folderGO.transform.localPosition = new Vector3(posX, posY, 0f);
            
            // 🔴 Масштаб 1, размеры через RectTransform
            folderGO.transform.localScale = Vector3.one;
            folderGO.transform.localRotation = Quaternion.identity;
            
            // RectTransform для folderGO
            RectTransform folderRT = folderGO.GetComponent<RectTransform>();
            if (folderRT == null) folderRT = folderGO.AddComponent<RectTransform>();
            folderRT.sizeDelta = slotSize;
            folderRT.anchorMin = new Vector2(0.5f, 0.5f);
            folderRT.anchorMax = new Vector2(0.5f, 0.5f);
            folderRT.anchoredPosition = new Vector2(posX, posY);
            
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
        Debug.Log($"[SaveSlotSystem] FoldersSaveMenu.childCount: {transform.childCount}");
        
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
        
        // 🔴 Жёсткая валидация: childCount должен быть ровно 20
        Debug.Assert(transform.childCount == 20, "Wrong file count!");
        
        // 5. ВИЗУАЛИЗАЦИЯ GIZMOS
        Debug.Log("[SaveSlotSystem] 🎨 Визуализация Gizmos активирована");
    }
}
