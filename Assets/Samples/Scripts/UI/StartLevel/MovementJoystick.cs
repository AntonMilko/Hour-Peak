using UnityEngine;
using HourPeak.Addition;

/// <summary>
/// Управление движением персонажа через джойстик на экране.
/// Читает ввод с UI джойстика и передаёт его PlayerController.
/// Поддерживает переключение на клавиатуру для отладки в Editor.
/// </summary>
public class MovementJoystick : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Минимальная величина ввода для считания активным.
    /// </summary>
    private const float INPUT_THRESHOLD = 0.01f;

    /// <summary>
    /// Максимальная величина вектора ввода.
    /// </summary>
    private const float MAX_INPUT_MAGNITUDE = 1f;

    #endregion

    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;
    
    [Tooltip("Ссылка на Joystick (автоматически найдётся по тегу Player)")]
    [SerializeField] private Joystick virtualJoystick;

    #endregion

    #region Private State

    private ClickTracker movementStick;
    private bool playerControllerResolved;

    private bool movementStickResolved;
    
    private Vector2 inputDirection; 

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        inputDirection = virtualJoystick.Direction;
    }

    private void FixedUpdate()
    {
        if (inputDirection != Vector2.zero)
        {
            playerController.Move(inputDirection);
        }
    }

    #endregion

    #region Reference Resolution

    /// <summary>
    /// Находит джойстик движения среди всех ClickTracker.
    /// </summary>
    private void ResolveMovementStick()
    {
        if (movementStick != null)
        {
            movementStickResolved = true;
            return;
        }
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Получает ввод с джойстика.
    /// </summary>
    private Vector2 GetJoystickInput()
    {
        // Способ 1: Через ClickTracker
        if (movementStick != null)
        {
            Vector2 stickInput = movementStick.GetInputAxis();
            if (stickInput.sqrMagnitude > INPUT_THRESHOLD)
                return stickInput;
        }

        return Vector2.zero;
    }

    #endregion
}