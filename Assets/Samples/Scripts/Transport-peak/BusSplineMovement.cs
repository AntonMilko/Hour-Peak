using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BusSplineMovement : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private Transform pathContainer;
    [SerializeField] private float cruiseSpeed = 30f;
    [SerializeField] private float slowDownSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 7f;
    [SerializeField] private bool loopPath = true;

    [Header("Светофоры")]
    [SerializeField] private float lightDetectionDistance = 15f;
    [SerializeField] private float lightStopDistance = 5f;
    [SerializeField] private List<Transform> lightTraffics = new List<Transform>();

    [Header("Остановки")]
    [SerializeField] private List<Transform> busStopPoints = new List<Transform>();
    [SerializeField] private float stopDuration = 10f;
    [SerializeField] private float stopApproachDistance = 3f;

    [Header("Поток")]
    [SerializeField] private bool enableTrafficFlow = false;
    [SerializeField] private float safeFollowingDistance = 30f;
    [SerializeField] private LayerMask busLayer;

    private List<Transform> waypoints;
    private float currentDistance;
    private float totalLength;
    private float currentSpeed;
    private bool isStopped;
    private bool reachedEnd;
    private Transform currentStop;
    private BusSplineMovement busInFront;
    private GameObject activeSignalObject;

    private void Awake()
    {
        waypoints = new List<Transform>();
        currentDistance = 0f;
        totalLength = 0f;
        currentSpeed = 0f;
        isStopped = false;
        reachedEnd = false;
        currentStop = null;
        busInFront = null;
        activeSignalObject = null;
    }

    private void Start()
    {
        if (!InitPath())
        {
            enabled = false;
            return;
        }
        currentSpeed = cruiseSpeed;
    }

    private void Update()
    {
        if (reachedEnd || waypoints.Count < 2)
            return;

        if (isStopped)
        {
            currentSpeed = 0f;
            return;
        }

        CheckLights();
        if (enableTrafficFlow)
            CheckBusAhead();
        CheckBusStop();
        UpdateSpeed();
        Move();
    }

    private bool InitPath()
    {
        if (pathContainer == null)
            return false;

        waypoints.Clear();
        for (int i = 0; i < pathContainer.childCount; i++)
        {
            Transform child = pathContainer.GetChild(i);
            if (child != null)
                waypoints.Add(child);
        }

        if (waypoints.Count < 2)
            return false;

        totalLength = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
            totalLength += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);

        if (loopPath && waypoints.Count > 1)
            totalLength += Vector3.Distance(waypoints[waypoints.Count - 1].position, waypoints[0].position);

        return true;
    }

    private Vector3 GetPositionOnPath(float dist)
    {
        if (waypoints.Count < 2)
            return transform.position;

        float acc = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float len = Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            if (dist >= acc && dist < acc + len)
                return Vector3.Lerp(waypoints[i].position, waypoints[i + 1].position, (dist - acc) / len);
            acc += len;
        }
        return waypoints[waypoints.Count - 1].position;
    }

    private void CheckLights()
    {
        if (lightTraffics == null || lightTraffics.Count == 0)
            return;

        GameObject closestSignal = null;
        float closestDist = float.MaxValue;

        // Проверяем только светофоры из назначенного списка
        foreach (Transform light in lightTraffics)
        {
            if (light == null) continue;

            float dist = Vector3.Distance(transform.position, light.position);
            if (dist <= lightDetectionDistance)
            {
                var signal = light.GetComponent<LightTraficSignal>();
                if (signal != null && signal.IsRed() && dist < closestDist)
                {
                    closestSignal = light.gameObject;
                    closestDist = dist;
                }
            }
        }

        if (closestSignal != null && closestDist <= lightDetectionDistance)
        {
            activeSignalObject = closestSignal;
            if (closestDist <= lightStopDistance)
            {
                isStopped = true;
            }
            else if (closestDist <= lightStopDistance + 5f)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, slowDownSpeed, deceleration * Time.deltaTime);
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
        if (Physics.Raycast(ray, out hit, safeFollowingDistance, busLayer))
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
            if (stop == null)
                continue;

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

    private void UpdateSpeed()
    {
        float target = cruiseSpeed;

        if (activeSignalObject != null)
        {
            var signal = activeSignalObject.GetComponent<LightTraficSignal>();
            if (signal != null && signal.IsRed())
            {
                float dist = Vector3.Distance(transform.position, activeSignalObject.transform.position);
                if (dist <= lightStopDistance)
                {
                    target = 0f;
                    isStopped = true;
                }
                else if (dist <= lightStopDistance + 5f)
                {
                    target = slowDownSpeed;
                }
            }
        }

        if (busInFront != null)
        {
            float dist = Vector3.Distance(transform.position, busInFront.transform.position);
            if (dist < safeFollowingDistance * 0.5f)
            {
                target = 0f;
                isStopped = true;
            }
            else if (dist < safeFollowingDistance)
            {
                target = slowDownSpeed;
            }
        }

        if (!isStopped)
        {
            if (currentSpeed > target)
                currentSpeed = Mathf.Lerp(currentSpeed, target, deceleration * Time.deltaTime);
            else
                currentSpeed = Mathf.Lerp(currentSpeed, target, acceleration * Time.deltaTime);
        }
    }

    private void Move()
    {
        float distanceToMove = currentSpeed * Time.deltaTime;
        currentDistance += distanceToMove;

if (currentDistance >= totalLength)
        {
            if (loopPath)
                currentDistance = currentDistance % totalLength;
            else
            {
                currentDistance = totalLength;
                reachedEnd = true;
                return;
            }
        }

        Vector3 pos = GetPositionOnPath(currentDistance);
        Vector3 next = GetPositionOnPath(currentDistance + 0.5f);
        transform.position = pos;

        Vector3 direction = next - pos;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    public float GetCurrentSpeed() => currentSpeed;
    public bool IsStopped() => isStopped;
    public bool IsAtBusStop() => currentStop != null;
    public bool IsWaitingAtBusStop => isStopped && currentStop != null;
    public void ForceStop() { isStopped = true; currentSpeed = 0f; }
    public void ResumeMovement() { isStopped = false; currentSpeed = slowDownSpeed; }
    public void SetCruiseSpeed(float speed) { cruiseSpeed = Mathf.Max(speed, 1f); }

    private void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 1)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < waypoints.Count - 1; i++)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector3 detectionCenter = transform.position + transform.forward * (lightDetectionDistance / 2f) + Vector3.up * 1.5f;
        Vector3 detectionExtents = new Vector3(3f, 4f, lightDetectionDistance);
        Gizmos.DrawWireCube(detectionCenter, detectionExtents);
    }

    private void OnDrawGizmosSelected()
    {
        if (busStopPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform stop in busStopPoints)
            {
                if (stop != null)
                    Gizmos.DrawWireSphere(stop.position, 2f);
            }
        }

        Gizmos.color = Color.yellow;
        Vector3 flowCenter = transform.position + transform.forward * (safeFollowingDistance / 2f) + Vector3.up * 1f;
        Vector3 flowExtents = new Vector3(3f, 3f, safeFollowingDistance / 2f);
        Gizmos.DrawWireCube(flowCenter, flowExtents);
    }
}