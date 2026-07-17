using UnityEngine;

public class CharacterSetup : MonoBehaviour
{
    public float height = 2f;
    public float radius = 0.3f;
    public float stepOffset = 1.5f;

    void Start()
    {
        CharacterController controller = GetComponent<CharacterController>();

        float maxStepOffset = height + radius * 2;

        if (stepOffset > maxStepOffset)
        {
            Debug.LogWarning($"Step Offset too large! Clamping from {stepOffset} to {maxStepOffset}");
            stepOffset = maxStepOffset;
        }

        controller.height = height;
        controller.radius = radius;
        controller.stepOffset = stepOffset;
    }
}