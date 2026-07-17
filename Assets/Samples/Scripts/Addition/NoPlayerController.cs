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

    [Header("Rigidbody")]
    [Tooltip("Reference to Rigidbody component")]
    [SerializeField] private Rigidbody rb;

    [Tooltip("Whether to apply gravity")]
    [SerializeField] private bool applyGravity = true;

    [Header("Movement")]
    [Tooltip("Speed of movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Distance to pick a new target point")]
    [SerializeField] private float targetDistance = 5f;

    [Tooltip("Range for random target offset")]
    [SerializeField] private float randomOffset = 3f;

    [Header("NavMesh")]
    [Tooltip("Whether to use NavMesh movement when available")]
    [SerializeField] private bool useNavMesh = true;

    [Tooltip("NavMesh layer mask for movement. Use ~0 for all layers.")]
    [SerializeField] private int navMeshLayerMask = ~0;

    #endregion

    #region Fields

    private NavMeshAgent navMeshAgent;
    private Transform targetPoint;
    private float nextTargetTime;
    private bool hasNavMesh;

    // Rigidbody fallback fields
    private Vector3 moveDirection;
    private float nextDirectionChangeTime;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        animator ??= GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
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

        PickNewTarget();
    }

    private void Update()
    {
        if (hasNavMesh && useNavMesh && navMeshAgent != null && navMeshAgent.enabled)
        {
            MoveWithNavMesh();
        }
        else
        {
            MoveWithRigidbody();
        }

        // Update target periodically
        if (Time.time >= nextTargetTime)
        {
            PickNewTarget();
        }
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

    /// <summary>
    /// Moves the NPC using Rigidbody.Movement when no NavMesh is available.
    /// Moves in a straight line without rotation.
    /// </summary>
    private void MoveWithRigidbody()
    {
        if (rb == null || !rb) return;

        // Choose random direction if needed
        if (Time.time >= nextDirectionChangeTime)
        {
            float angle = Random.value * Mathf.PI * 2f;
            moveDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
            nextDirectionChangeTime = Time.time + Random.Range(2f, 5f);
        }

        // Move in straight line
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // Apply gravity if needed
        if (applyGravity)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y -= Physics.gravity.y * Time.deltaTime;
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
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

    #region Cleanup

    private void OnDestroy()
    {
        // Clean up target objects only during play mode
        if (!Application.isPlaying) return;

        if (targetPoint != null && targetPoint.parent == transform)
        {
            Destroy(targetPoint.gameObject);
        }
    }

    #endregion
}