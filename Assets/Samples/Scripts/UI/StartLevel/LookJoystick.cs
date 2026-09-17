using UnityEngine;
using HourPeak.Addition;

/// <summary>
/// Управление вращением камеры через джойстик на экране.
/// Читает ввод с UI джойстика и поворачивает камеру персонажа.
/// Поддерживает переключение на мышь для отладки в Editor.
/// </summary>
public class LookJoystick : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Минимальная величина ввода для считания активным.
    /// </summary>
    private const float INPUT_THRESHOLD = 0.01f;

    #endregion

    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на камеру (автоматически найдётся через PlayerController)")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    [Header("Rotation Settings")]
    [Tooltip("Скорость вращения камеры по горизонтали (градусов в секунду)")]
    [SerializeField] private float horizontalSensitivity = 120f;
    
    [Tooltip("Скорость вращения камеры по вертикали (градусов в секунду)")]
    [SerializeField] private float verticalSensitivity = 120f;
    
    [Tooltip("Минимальный угол наклона камеры (вверх)")]
    [SerializeField] private float minLookAngle = -45f;
    
    [Tooltip("Максимальный угол наклона камеры (вниз)")]
    [SerializeField] private float maxLookAngle = 45f;

    [Tooltip("Ссылка на Joystick")]
    [SerializeField] private Joystick virtualJoystick;

    #endregion

    #region Private State
    
    private float currentVerticalAngle;
    private Vector2 lookInput;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeLookAngle();
    }

    private void Update()
    {
        lookInput = virtualJoystick.Direction;
        ApplyRotation();
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Инициализирует текущий угол наклона камеры.
    /// </summary>
    private void InitializeLookAngle()
    {
        if (playerCamera != null)
        {
            Vector3 euler = playerCamera.transform.localEulerAngles;
            currentVerticalAngle = Mathf.Deg2Rad * euler.x;
        }
    }

    /// <summary>
    /// Применяет вращение к камере.
    /// </summary>
    private void ApplyRotation()
    {
        if (lookInput.sqrMagnitude < INPUT_THRESHOLD)
            return;

        // Вычисляем изменение угла
        float horizontalDelta = lookInput.x * horizontalSensitivity * Time.deltaTime;
        float verticalDelta = lookInput.y * verticalSensitivity * Time.deltaTime;

        // Поворачиваем персонажа по горизонтали
        if (playerController != null)
        {
            playerController.transform.Rotate(0, horizontalDelta, 0);
        }

        // Поворачиваем камеру по вертикали (с ограничениями)
        currentVerticalAngle -= verticalDelta * Mathf.Deg2Rad;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minLookAngle * Mathf.Deg2Rad, maxLookAngle * Mathf.Deg2Rad);

        // Применяем вращение к камере
        Vector3 euler = playerCamera.transform.localEulerAngles;
        euler.x = currentVerticalAngle * Mathf.Rad2Deg;
        playerCamera.transform.localEulerAngles = euler;
    }

    #endregion
}