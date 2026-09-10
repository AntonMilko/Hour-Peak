using UnityEngine;
using HourPeak.Addition;

public class MobileInputSystem : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public PlayerController player;
    public Joystick movementJoystick;
    public Joystick lookJoystick;
    public Transform cameraTransform;

    [Header("Настройки чувствительности обзора")]
    public float sensitivityX = 50f;
    public float sensitivityY = 50f;

    private float _xRotation = 0f;

    private void Start()
    {
        if (Camera.main != null && cameraTransform == null) 
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (player == null) return;

        if (movementJoystick != null)
        {
            player.JoystickMoveInput = movementJoystick.Direction;
        }

        if (lookJoystick != null && cameraTransform != null)
        {
            float mouseX = lookJoystick.Horizontal * sensitivityX * Time.deltaTime;
            float mouseY = lookJoystick.Vertical * sensitivityY * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            player.transform.Rotate(Vector3.up * mouseX);
        }
    }
}