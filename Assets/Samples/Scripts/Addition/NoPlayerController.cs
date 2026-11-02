using HourPeak.Characters;
using HourPeak.Transport;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC Pedestrian Controller with dual movement support.
/// Uses NavMeshAgent when NavMesh is available, falls back to
/// Rigidbody.Movement when no NavMesh is detected.
/// </summary>
[ExecuteAlways]
public class NoPlayerController : MonoBehaviour
{
    #region Configuration

    [Header("Animation")]
    [Tooltip("Reference to Animator component")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [Tooltip("Speed of movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float slowMoveSpeed = 15f;

    [Tooltip("Distance to pick a new target point")]
    [SerializeField] private float targetDistance = 5f;

    [Tooltip("Range for random target offset")]
    [SerializeField] private float randomOffset = 3f;
    [SerializeField] private float deceleration = 7f;

    [Header("Светофоры")]
    [SerializeField] private float lightDetectionDistance = 15f;
    [SerializeField] private float lightStopDistance = 5f;
    [SerializeField] private List<Transform> pedestrianLightTraffics = new List<Transform>();

    [Header("NavMesh")]
    [Tooltip("Whether to use NavMesh movement when available")]
    [SerializeField] private bool useNavMesh = true;

    [Tooltip("NavMesh layer mask for movement. Use ~0 for all layers.")]
    [SerializeField] private int navMeshLayerMask = ~0;

    [Header("Условие бега")]
    [SerializeField] private bool isRunning = false;

    [Header("Поток")]
    [SerializeField] private bool enableTrafficFlow = true;
    [SerializeField] private float safeFollowingDistance = 30f;

    #endregion

    #region Fields

    private NavMeshAgent navMeshAgent;
    private Transform targetPoint;
    private float nextTargetTime;
    private bool hasNavMesh;
    private bool isMoving;
    private bool isStopped;
    private float currentSpeed;
    private GameObject activeSignalObject;
    private CrowdSpawner crowdInFront;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        animator ??= GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        hasNavMesh = CheckNavMeshExists();
        if (hasNavMesh)
        {
            Debug.Log("NavMesh detected. Using NavMeshAgent movement.", this);
        }
        else
        {
            Debug.LogWarning("NavMesh not found! Falling back to Rigidbody.Movement.", this);
        }

        if (Time.time >= nextTargetTime)
        {
            PickNewTarget();
        }

        if (hasNavMesh && useNavMesh && navMeshAgent != null && navMeshAgent.enabled)
        {
            MoveWithNavMesh();
        }

        CheckPedestrianLights();
        if (enableTrafficFlow)
            CheckPedestrianTrafficFlow();
        MoveWithNavMesh();
    }

    private void FixedUpdate()
    {
        UpdateAnimation();
    }

    #endregion

    #region Movement

    /// <summary>
    /// Moves the NPC using NavMeshAgent toward the target point.
    /// </summary>
    private void MoveWithNavMesh()
    {
        if (targetPoint != null)
        {
            navMeshAgent.SetDestination(targetPoint.position);
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.angularSpeed = 180f;

            // Pick a new target when close enough
            float distance = Vector3.Distance(transform.position, targetPoint.position);
            if (distance < navMeshAgent.stoppingDistance + 0.5f)
            {
                PickNewTarget();
            }
        }
    }

    private void UpdateAnimation()
    {
        if (isMoving)
        {
            if (isRunning)
            {
                animator.SetBool ("IsRunning", true);
                animator.SetBool ("IsWalking", false);
            }
            else
            {
                animator.SetBool ("IsRunning", false);
                animator.SetBool ("IsWalking", true);
            }
        }

        else
        {
            animator.SetBool ("IsRunning", false);
            animator.SetBool ("IsWalking", false);
        }

        isMoving = false;
    }

    private void CheckPedestrianLights()
    {
        if (pedestrianLightTraffics.Count == 0)
            return;

        GameObject closestSignal = null;
        float closestDist = float.MaxValue;

        // Проверяем только светофоры из назначенного списка
        foreach (Transform light in pedestrianLightTraffics)
        {
            float dist = Vector3.Distance(transform.position, light.position);
            if (dist <= lightDetectionDistance)
            {
                var signal = light.GetComponent<LightTrafficController>();
                if (!signal.IsGreen() && dist < closestDist)
                {
                    closestSignal = light.gameObject;
                    closestDist = dist;
                }
            }
        }

        if (closestSignal != null)
        {
            activeSignalObject = closestSignal;
            if (currentSpeed <= 0)
            {
                isStopped = true;
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
                if (closestDist <= lightStopDistance)
                    currentSpeed = 0;
            }
        }
        else if (isStopped && activeSignalObject != null)
        {
            var signal = activeSignalObject.GetComponent<LightTrafficController>();
            if (signal != null && signal.IsGreen())
            {
                StartCoroutine(ReleaseFromSignal());
            }
        }
    }

    private IEnumerator ReleaseFromSignal()
    {
        Debug.Log("ReleaseFromSignal");
        isStopped = false;
        activeSignalObject = null;
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            currentSpeed = Mathf.Lerp(0f, moveSpeed, t);
            yield return null;
        }
        currentSpeed = moveSpeed;
    }

    private void CheckPedestrianTrafficFlow()
    {
        crowdInFront = null;
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, safeFollowingDistance))
        {
            var other = hit.collider.GetComponent<CrowdSpawner>();
            if (other != null && other != this)
                crowdInFront = other;
        }

        if (crowdInFront != null)
        {
            float dist = Vector3.Distance(transform.position, crowdInFront.transform.position);
            if (dist < safeFollowingDistance * 0.5f)
                isStopped = true;
            else if (dist < safeFollowingDistance)
                currentSpeed = Mathf.Lerp(currentSpeed, slowMoveSpeed, deceleration * Time.deltaTime);
        }
    }

    #endregion

    #region Target Selection

    /// <summary>
    /// Picks a new random target point within range.
    /// </summary>
    private void PickNewTarget()
    {
        Vector3 origin = transform.position;

        if (hasNavMesh && useNavMesh)
        {
            // Try to find a valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(origin, out hit, targetDistance, navMeshLayerMask))
            {
                Vector3 randomDirection = Random.insideUnitCircle * randomOffset;
                Vector3 target = hit.position + new Vector3(randomDirection.x, 0, randomDirection.y);

                // Clamp to NavMesh
                if (NavMesh.SamplePosition(target, out hit, 2f, navMeshLayerMask))
                {
                    targetPoint = CreateTargetObject(hit.position);
                }
            }
        }

        // Fallback: random position on ground plane
        if (targetPoint == null)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float distance = Random.Range(1f, targetDistance);
            Vector3 randomPos = origin + new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
            targetPoint = CreateTargetObject(randomPos);
        }

        nextTargetTime = Time.time + Random.Range(1f, 3f);
    }

    /// <summary>
    /// Creates a temporary target object.
    /// </summary>
    private Transform CreateTargetObject(Vector3 position)
    {
        GameObject targetObj = new GameObject("TargetPoint");
        targetObj.transform.position = position;
        targetObj.transform.SetParent(transform);
        return targetObj.transform;
    }

    #endregion

    #region NavMesh Check

    /// <summary>
    /// Checks if NavMesh exists near the NPC position.
    /// </summary>
    private bool CheckNavMeshExists()
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(transform.position, out hit, 5f, navMeshLayerMask);
    }

    #endregion
}