using UnityEngine;
using UnityEngine.UI;
using HourPeak.Addition;

public class BusExitButton : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [Tooltip("Ссылка на PlayerController (автоматически найдётся по тегу Player)")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject exitButton;

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        exitButton.SetActive(playerController.ShouldShowBusExitButton());
    }

    #endregion

    #region Public Methods

    public void OnClick()
    {
        playerController.BusExit();
    }

    #endregion
}