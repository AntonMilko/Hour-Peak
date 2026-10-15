using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Переключение между камерой персонажа и картой.
/// При нажатии кнопки карты скрывается камера персонажа и отображается карта.
/// </summary>
public class CameraSwitch : MonoBehaviour
{
    #region Fields

    [Header("Cameras")]
    [Tooltip("Камера персонажа (основная)")]
    [SerializeField] private Camera playerCamera;
    
    [Tooltip("Камера карты")]
    [SerializeField] private Camera mapCamera;

    [Header("UI")]
    [Tooltip("Игровой интерфейс")]
    [SerializeField] private GameObject GameUI;

    [Tooltip("Панель карты")]
    [SerializeField] private GameObject mapPanel;

    [Header("TimerDisplay")]
    [Tooltip("Таймер")]
    [SerializeField] private TimerDisplay timer;

    #endregion

    #region Public Methods

    /// <summary>
    /// Принудительно устанавливает состояние карты.
    /// </summary>
    public void SetMapOpen(bool open)
    {
        // 1. Отключаем камеру персонажа
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(!open);

        // 2. Включаем камеру карты
        if (mapCamera != null)
            mapCamera.gameObject.SetActive(open);

        // 3. Отключаем GameUI
        if (GameUI.gameObject != null)
            GameUI.gameObject.SetActive(!open);

        // 4. Показываем панель карты
        if (mapPanel != null)
            mapPanel.SetActive(open);

        // 5. Останавливаем время (игра на паузе)
        timer.SetIsRunning(!open);
    }

    #endregion

}