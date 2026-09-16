using UnityEngine;
using UnityEngine.UI;
using HourPeak.Addition;
using System.Reflection;

/// <summary>
/// Управление бегом персонажа через UI кнопку на экране.
/// Читает ввод с UI кнопки и вызывает RequestRun() в PlayerController.
/// Поддерживает переключение на Shift для отладки в Editor.
/// </summary>
public class Running : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Минимальная величина ввода для считания активным.
    /// </summary>
    private const float INPUT_THRESHOLD = 0.01f;

    #endregion

    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;

    #endregion

    #region Unity Lifecycle

    public void OnRunButtonDown()
    {
        playerController.isRunning = true;
        Debug.Log("Кнопка бега ЗАЖАТА. Скорость увеличена!");
    }

    public void OnRunButtonUp()
    {
        playerController.isRunning = false;
        Debug.Log("Кнопка бега ОТПУЩЕНА. Скорость нормализована.");
    }

    #endregion

}