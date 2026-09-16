using UnityEngine;
using UnityEngine.UI;

public class MapToggle : MonoBehaviour
{
    [Header("UI Панели")]
    [Tooltip("Панель основного игрового интерфейса")]
    [SerializeField] private GameObject gameUI;
    
    [Tooltip("Панель интерфейса карты")]
    [SerializeField] private GameObject mapUI;

    [Header("Камеры")]
    [Tooltip("Главная камера игрока (найдётся автоматически по тегу MainCamera)")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("Камера, которая снимает карту")]
    [SerializeField] private Camera mapCamera;

    [Header("Кнопки")]
    [Tooltip("Ссылка на кнопку открытия карты (MapButton)")]
    [SerializeField] private Button mapButton;

    [Header("Настройка переключения")]
    [Tooltip("Флаг текущего состояния (true - карта открыта, false - закрыта)")]
    [SerializeField] private bool isMapOpen = false;

    private void Start()
    {
        // Если главная камера не перетащена вручную, ищем её на сцене
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Гарантируем правильное состояние при старте игры: игра видна, карта скрыта
        SetMapState(false);

        // Подписываем MapButton на метод переключения
        if (mapButton != null)
        {
            mapButton.onClick.AddListener(ToggleMap);
        }
        else
        {
            Debug.LogError("[MapToggle] Не привязана MapButton в инспекторе!");
        }
    }

    /// <summary>
    /// Метод инвертирует текущее состояние карты (открывает/закрывает)
    /// </summary>
    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        SetMapState(isMapOpen);
    }

    /// <summary>
    /// Включает и выключает нужные компоненты в зависимости от флага
    /// </summary>
    private void SetMapState(bool openMap)
    {
        // Переключаем UI-панели
        if (gameUI != null) gameUI.SetActive(!openMap);
        if (mapUI != null) mapUI.SetActive(openMap);

        // Переключаем камеры
        if (mainCamera != null) mainCamera.enabled = !openMap;
        if (mapCamera != null) mapCamera.enabled = openMap;
        
        Debug.Log(openMap ? "Карта открыта. MapCamera активна." : "Карта закрыта. Main Camera активна.");
    }

    private void OnDestroy()
    {
        if (mapButton != null)
        {
            mapButton.onClick.RemoveListener(ToggleMap);
        }
    }
}