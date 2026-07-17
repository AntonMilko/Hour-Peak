using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CarSplineMovement : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private Transform pathContainer;
    [SerializeField] private float cruiseSpeed = 40f;
    [SerializeField] private float slowDownSpeed = 15f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private bool loopPath = false;

    [Header("Светофоры")]
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float stopDistance = 4f;
    [SerializeField] private List<Transform> lightTraffics = new List<Transform>();

    [Header("Поток")]
    [SerializeField] private bool enableTrafficFlow = false;
    [SerializeField] private float safeFollowingDistance = 30f;
    [SerializeField] private LayerMask vehicleLayer;

    private List<Transform> waypoints = new List<Transform>();
    private float currentDistance = 0f, totalLength = 0f, currentSpeed = 0f;
    private bool isStopped = false, reachedEnd = false;
    private LightTraficSignal activeSignal = null;
    private GameObject carInFront;

    private void Start()
    {
        if (!InitPath()) { enabled = false; return; }
        currentSpeed = cruiseSpeed;
    }

    private void Update()
    {
        if (reachedEnd || waypoints.Count < 2) return;
        
        CheckLights();
        if (enableTrafficFlow)
            CheckCarAhead();
        UpdateSpeed();
        if (!isStopped) Move();
    }

    private bool InitPath()
    {
        if (!pathContainer) return false;
        waypoints.Clear();
        for (int i = 0; i < pathContainer.childCount; i++)
        {
            Transform child = pathContainer.GetChild(i);
            if (child != null) waypoints.Add(child);
        }
        if (waypoints.Count < 2) return false;
        totalLength = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
            totalLength += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        return true;
    }

    private Vector3 GetPosition(float dist)
    {
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
        if (lightTraffics == null || lightTraffics.Count == 0) return;

        LightTraficSignal closest = null;
        float dist = float.MaxValue;

        foreach (Transform light in lightTraffics)
        {
            if (light == null) continue;

            float d = Vector3.Distance(transform.position, light.position);
            if (d <= detectionDistance)
            {
                var s = light.GetComponent<LightTraficSignal>();
                if (s && s.IsRed() && d < dist)
                {
                    closest = s;
                    dist = d;
                }
            }
        }

        if (closest && dist <= detectionDistance)
        {
            activeSignal = closest;
            if (dist <= stopDistance + 3f && !isStopped) isStopped = true;
        }
        else if (isStopped && activeSignal != null && !activeSignal.IsRed())
        {
            StartCoroutine(Accelerate());
        }
    }

    private void CheckCarAhead()
    {
        carInFront = null;
        Ray ray = new Ray(transform.position + Vector3.up * 1.5f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, safeFollowingDistance, vehicleLayer))
        {
            carInFront = hit.collider.gameObject;
        }
    }

    private void UpdateSpeed()
    {
        float target = cruiseSpeed;

        if (activeSignal != null && activeSignal.IsRed())
        {
            float distToLight = Vector3.Distance(transform.position, activeSignal.transform.position);
            if (distToLight <= stopDistance) { target = 0f; isStopped = true; }
            else if (distToLight <= stopDistance + 5f) target = slowDownSpeed;
        }

        if (enableTrafficFlow && carInFront != null)
        {
            float distToCar = Vector3.Distance(transform.position, carInFront.transform.position);
            if (distToCar < safeFollowingDistance * 0.5f)
            {
                target = 0f;
                isStopped = true;
            }
            else if (distToCar < safeFollowingDistance)
            {
                target = slowDownSpeed;
            }
        }

        currentSpeed = currentSpeed > target ? Mathf.Lerp(currentSpeed, target, deceleration * Time.deltaTime) : Mathf.Lerp(currentSpeed, target, acceleration * Time.deltaTime);
        if (isStopped && currentSpeed < 0.1f) currentSpeed = 0f;
    }

    private void Move()
    {
        currentDistance += currentSpeed * Time.deltaTime;
        if (currentDistance >= totalLength)
        {
            if (loopPath) currentDistance %= totalLength;
            else { currentDistance = totalLength; reachedEnd = true; return; }
        }
        Vector3 pos = GetPosition(currentDistance);
        Vector3 next = GetPosition(currentDistance + 0.5f);
        transform.position = pos;
        if ((next - pos).sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(next - pos), 10f * Time.deltaTime);
    }

    private IEnumerator Accelerate() { isStopped = false; activeSignal = null; float t = 0f; while (t < 1.5f) { t += Time.deltaTime; currentSpeed = Mathf.Lerp(0f, cruiseSpeed, t / 1.5f); yield return null; } currentSpeed = cruiseSpeed; }

    public float GetCurrentSpeed() => currentSpeed;
    public bool IsStopped() => isStopped;
    public void ForceStop() { isStopped = true; currentSpeed = 0f; }
    public void ResumeMovement() { isStopped = false; currentSpeed = slowDownSpeed; }
    public void SetCruiseSpeed(float speed) { cruiseSpeed = Mathf.Max(speed, 1f); }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; waypoints != null && i < waypoints.Count - 1; i++) Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireCube(transform.position + transform.forward * detectionDistance / 2f + Vector3.up * 1.5f, new Vector3(4f, 6f, detectionDistance));
    }
}