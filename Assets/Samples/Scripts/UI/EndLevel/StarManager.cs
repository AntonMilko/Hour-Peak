using UnityEngine;

public class StarManager : MonoBehaviour
{
    [Header("Settings")]

    // Current stars collected in this specific play session
    public int currentStars = 0;
    public int maxStars = 3;

    // Call this method whenever the player achieves a goal (e.g., collects a coin, finishes fast)
    public void AddStar()
    {
        if (currentStars < maxStars)
        {
            currentStars++;
            Debug.Log($"Star collected! Total: {currentStars}");
        }
    }
}