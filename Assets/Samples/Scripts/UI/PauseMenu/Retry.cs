using UnityEngine;
using UnityEngine.SceneManagement;
using HourPeak.Samples.Runtime;

namespace HourPeak.Samples.Runtime
{
    public class Retry : MonoBehaviour
    {
    [Header("References")]
    [Tooltip("Менеджер, который хранит настройки сложности")]
    [SerializeField] private Continue continuationManager;

    [Header("Настройки перезагрузки")]
    [Tooltip("Автосохранение настроек сложности при нажатии Retry")]
    [SerializeField] private bool autoSaveOnRetry = true;

    [Header("📊 Информация об уровне")]
    [Tooltip("Название сцены для перезагрузки (пусто = текущая сцена)")]
    [SerializeField] private string levelSceneName = "";
    [Tooltip("Номер текущего уровня")]
    [SerializeField] private string currentLevelNumber = "";
    [Tooltip("Часть текущего уровня (например: 1 или 2)")]
    [SerializeField] private string currentLevelPart = "";

    private void Start()
    {
        // Если название сцены не задано, используем текущую
        if (string.IsNullOrEmpty(levelSceneName))
        {
            levelSceneName = SceneManager.GetActiveScene().name;
        }
    }

    /// <summary>
    /// Логика кнопки Retry
    /// </summary>
    public void RetryLevel()
    {
        Debug.Log($"🔄 Retry Level: {levelSceneName} | " +
                  $"Уровень: {currentLevelNumber} | " +
                  $"Часть: {currentLevelPart}");

        // 1. Сохраняем настройки сложности, если автосохранение включено
        if (autoSaveOnRetry && continuationManager != null)
        {
            continuationManager.SaveCurrentSettings();
            Debug.Log("💾 Настройки сложности сохранены.");
        }

        // 2. Перезагружаем нужную сцену
        if (!string.IsNullOrEmpty(levelSceneName))
        {
            SceneManager.LoadScene(levelSceneName);
        }
        else
        {
            Debug.LogError("Не указано название сцены для перезагрузки!");
        }
    }
    }
}