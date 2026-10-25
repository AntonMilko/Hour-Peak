using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(BoxCollider))]
public class BusSplineMovement : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private SplineContainer pathContainer;
    [SerializeField] private float cruiseSpeed = 30f;
    [SerializeField] private float slowDownSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 7f;

    [Header("Светофоры")]
    [SerializeField] private float lightDetectionDistance = 15f;
    [SerializeField] private float lightStopDistance = 5f;
    [SerializeField] private List<Transform> lightTraffics = new List<Transform>();

    [Header("Остановки")]
    [SerializeField] private List<Transform> busStopPoints = new List<Transform>();
    [SerializeField] private float stopDuration = 10f;
    [SerializeField] private float stopApproachDistance = 3f;

    [Header("Поток")]
    [SerializeField] private bool enableTrafficFlow = true;
    [SerializeField] private float safeFollowingDistance = 30f;

    private List<BezierKnot> waypoints;
    private float currentSpeed;
    private bool isStopped;
    private Transform currentStop;
    private BusSplineMovement busInFront;
    private GameObject activeSignalObject;
    private int currentKnot;
    public bool IsWaitingAtBusStop => isStopped && currentStop != null;

    private void Awake()
    {
        waypoints = pathContainer.Splines[0].Knots.ToList();
        currentSpeed = cruiseSpeed;        
    }

    private void Update()
    {
        if (waypoints.Count < 2 || isStopped)
            return;

        CheckLights();
        if (enableTrafficFlow)
            CheckBusAhead();
        CheckBusStop();
        Move();
    }

    private void CheckLights()
    {
        if (lightTraffics.Count == 0)
            return;

        GameObject closestSignal = null;
        float closestDist = float.MaxValue;

        // Проверяем только светофоры из назначенного списка
        foreach (Transform light in lightTraffics)
        {
            float dist = Vector3.Distance(transform.position, light.position);
            if (dist <= lightDetectionDistance)
            {
                var signal = light.GetComponent<LightTraficSignal>();
                if (signal.IsRed() && dist < closestDist)
                {
                    closestSignal = light.gameObject;
                    closestDist = dist;
                }
            }
        }

        if (closestSignal != null)
        {
            activeSignalObject = closestSignal;
            if (closestDist <= lightStopDistance)
            {
                isStopped = true;
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
            }
        }
        else if (isStopped && activeSignalObject != null)
        {
            var signal = activeSignalObject.GetComponent<LightTraficSignal>();
            if (signal != null && !signal.IsRed())
            {
                StartCoroutine(ReleaseFromSignal());
            }
        }
    }

    private IEnumerator ReleaseFromSignal()
    {
        isStopped = false;
        activeSignalObject = null;
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentSpeed = Mathf.Lerp(0f, cruiseSpeed, t);
            yield return null;
        }
        currentSpeed = cruiseSpeed;
    }

    private void CheckBusAhead()
    {
        busInFront = null;
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, safeFollowingDistance))
        {
            var other = hit.collider.GetComponent<BusSplineMovement>();
            if (other != null && other != this)
                busInFront = other;
        }

        if (busInFront != null)
        {
            float dist = Vector3.Distance(transform.position, busInFront.transform.position);
            if (dist < safeFollowingDistance * 0.5f)
                isStopped = true;
            else if (dist < safeFollowingDistance)
                currentSpeed = Mathf.Lerp(currentSpeed, slowDownSpeed, deceleration * Time.deltaTime);
        }
    }

    private void CheckBusStop()
    {
        if (currentStop != null)
            return;

        foreach (Transform stop in busStopPoints)
        {
            float dist = Vector3.Distance(transform.position, stop.position);
            if (dist <= stopApproachDistance)
            {
                currentStop = stop;
                StartCoroutine(StopAtBusStop());
                break;
            }
        }
    }

    private IEnumerator StopAtBusStop()
    {
        isStopped = true;
        yield return new WaitForSeconds(stopDuration);
        isStopped = false;
        currentStop = null;
        StartCoroutine(AccelerateFromStop());
    }

    private IEnumerator AccelerateFromStop()
    {
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentSpeed = Mathf.Lerp(0f, cruiseSpeed, t);
            yield return null;
        }
        currentSpeed = cruiseSpeed;
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentKnot].Position, currentSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, waypoints[currentKnot].Rotation, currentSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, waypoints[currentKnot].Position) < 0.1f)
        {
            currentKnot = Mathf.Min(currentKnot + 1, waypoints.Count - 1);
        }
    }
}