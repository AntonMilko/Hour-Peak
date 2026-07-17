using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a city traffic signal during rush hour and exposes the active signal state
/// for CarSplineMovement and BusSplineMovement to react to.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LightTraficSignal : MonoBehaviour
{
    public enum SignalState
    {
        Green,
        Yellow,
        Red
    }

    [Header("Cycle Settings")]
    public SignalState startState = SignalState.Green;
    public float greenDuration = 10f;
    public float yellowDuration = 3f;
    public float redDuration = 10f;
    public bool autoCycle = true;
    public bool startAutomatically = true;

    [Header("Visuals")]
    public Renderer signalRenderer;
    public Color greenColor = Color.green;
    public Color yellowColor = Color.yellow;
    public Color redColor = Color.red;
    public string colorProperty = "_BaseColor";

    private SignalState currentState;
    private Coroutine cycleCoroutine;
    private int colorPropertyId;

    public SignalState CurrentState => currentState;

    // Для SignalIntersectionController
    public readonly List<LightTraficSignal> coordinatedSignals = new List<LightTraficSignal>();
    public int WaitingVehicleCount { get; internal set; }

    public object OnStateChanged { get; internal set; }

    public bool IsGreen() => currentState == SignalState.Green;
    public bool IsYellow() => currentState == SignalState.Yellow;
    public bool IsRed() => currentState == SignalState.Red;

    private void Awake()
    {
        colorPropertyId = Shader.PropertyToID(colorProperty);
        currentState = startState;
        UpdateVisuals();
    }

    private void Start()
    {
        if (autoCycle && startAutomatically)
        {
            StartCycle();
        }
    }

    public void StartCycle()
    {
        if (cycleCoroutine != null)
            StopCoroutine(cycleCoroutine);

        cycleCoroutine = StartCoroutine(CycleRoutine());
    }

    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
    }

    public void SetState(SignalState targetState)
    {
        currentState = targetState;
        UpdateVisuals();
    }

    /// <summary>
    /// Принудительно устанавливает состояние (для SignalIntersectionController)
    /// </summary>
    public void ForceState(SignalState state)
    {
        if (cycleCoroutine != null)
            StopCoroutine(cycleCoroutine);
        SetState(state);
    }

    /// <summary>
    /// Ставит цикл на паузу (для SignalIntersectionController)
    /// </summary>
    public void PauseCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
    }

    /// <summary>
    /// Возобновляет цикл (для SignalIntersectionController)
    /// </summary>
    public void ResumeCycle()
    {
        if (cycleCoroutine == null && autoCycle)
        {
            cycleCoroutine = StartCoroutine(CycleRoutine());
        }
    }

    /// <summary>
    /// Устанавливает длительность фазы (для SignalIntersectionController)
    /// </summary>
    public void SetPhaseDuration(SignalState state, float duration)
    {
        switch (state)
        {
            case SignalState.Green:
                greenDuration = duration;
                break;
            case SignalState.Yellow:
                yellowDuration = duration;
                break;
            case SignalState.Red:
                redDuration = duration;
                break;
        }
    }

    public void SetStateTemporary(SignalState targetState, float duration)
    {
        SetState(targetState);
        if (cycleCoroutine != null)
            StopCoroutine(cycleCoroutine);

        cycleCoroutine = StartCoroutine(TemporaryStateRoutine(duration));
    }

    private IEnumerator TemporaryStateRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        cycleCoroutine = StartCoroutine(CycleRoutine());
    }

    private IEnumerator CycleRoutine()
    {
        SetState(startState);

        while (true)
        {
            switch (currentState)
            {
                case SignalState.Green:
                    yield return new WaitForSeconds(greenDuration);
                    SetState(SignalState.Yellow);
                    break;
                case SignalState.Yellow:
                    yield return new WaitForSeconds(yellowDuration);
                    SetState(SignalState.Red);
                    break;
                case SignalState.Red:
                    yield return new WaitForSeconds(redDuration);
                    SetState(SignalState.Green);
                    break;
            }
        }
    }

    private void UpdateVisuals()
    {
        if (signalRenderer == null)
            return;

        if (signalRenderer.material == null)
            return;

        Color stateColor = redColor;
        switch (currentState)
        {
            case SignalState.Green:
                stateColor = greenColor;
                break;
            case SignalState.Yellow:
                stateColor = yellowColor;
                break;
            case SignalState.Red:
                stateColor = redColor;
                break;
        }

        if (signalRenderer.material.HasProperty(colorPropertyId))
        {
            signalRenderer.material.SetColor(colorPropertyId, stateColor);
        }
        else
        {
            signalRenderer.material.color = stateColor;
        }
    }

    internal void RegisterWaitingVehicle(GameObject gameObject)
    {
        throw new NotImplementedException();
    }
}