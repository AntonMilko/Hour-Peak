using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    #region Fields

    [Header("Zoom Settings")]
    [Tooltip("Максимальный масштаб карты")]
    [SerializeField] private float maxScale = 1200f;
    
    [Tooltip("Минимальный масштаб карты")]
    [SerializeField] private float minScale = 200f;

    [Tooltip("Насколько перемещать карту вверх за одно нажатие (в единицах)")]
    [SerializeField] private float moveAmount = 500f;
    
    [Tooltip("Скорость плавного перемещения карты")]
    [SerializeField] private float moveSpeed = 10f;
    
    [Tooltip("Скорость плавного перехода зума")]
    [SerializeField] private float zoomSpeed = 500f;

    [Header("UI References")]
    [Tooltip("Управление камерой при уменьшении масштаба карты")]
    [SerializeField] private Camera mapCamera;

    #endregion

    #region Private State

    private Vector3 targetPosition = new Vector3(0f, 900f, 0f);
    private float currentTargetScale;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        currentTargetScale = maxScale;
    }

    private void Update()
    {
        mapCamera.orthographicSize = Mathf.Lerp(mapCamera.orthographicSize, currentTargetScale, Time.deltaTime * 10);
        mapCamera.transform.position = Vector3.Lerp(mapCamera.transform.position, targetPosition, Time.deltaTime * 10);
    }

    #endregion

    #region Public Methods

    public void ZoomIn()
    {  
        if (mapCamera.orthographicSize > minScale)
        {
            currentTargetScale = Mathf.Max(currentTargetScale - zoomSpeed, minScale);
        }
    }

    public void ZoomOut()
    {  
        if (mapCamera.orthographicSize < maxScale)
        {
            currentTargetScale = Mathf.Min(currentTargetScale + zoomSpeed, maxScale);
        }
    }

    public void Up()
    {
        if (currentTargetScale < maxScale)
        {
            targetPosition.z += moveAmount;
        }
    }

    public void Down()
    {
        if (currentTargetScale < maxScale)
        {
            targetPosition.z -= moveAmount;
        }
    }

    public void Right()
    {
        if (currentTargetScale < maxScale)
        {
            targetPosition.x += moveAmount;
        }   
    }

    public void Left()
    {
        if (currentTargetScale < maxScale)
        {
            targetPosition.x -= moveAmount;
        }
    }

    public void Reset()
    {   
        currentTargetScale = maxScale;
        targetPosition = new Vector3(0f, 900f, 0f);
    }

    #endregion
}
