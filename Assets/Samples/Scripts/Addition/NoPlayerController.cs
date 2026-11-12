using HourPeak.Characters;
using HourPeak.Transport;
using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class NoPlayerController : MonoBehaviour
{
    #region Configuration

    [Header("Animation")]
    [Tooltip("Reference to Animator component")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [Tooltip("Speed of movement")]
    [SerializeField] private float moveSpeed = 9f;

    [Tooltip("Distance to stop moving")]
    [SerializeField] private float stopDistance = 4f;

    #endregion

    #region Fields

    private NavMeshAgent navMeshAgent;
    private Transform targetPoint;
    private bool isMoving;
    private LightTrafficSystem activeSignalObject;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        animator ??= GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        PickNewTarget();
    }

    private void Update()
    {
        CheckPedestrianLights();
        CheckTargetDistance();
        UpdateAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        PedestrianCrossing crossing = other.GetComponent<PedestrianCrossing>();
        if (crossing != null)
        {
            activeSignalObject = crossing.lightTraffic;
            navMeshAgent.speed = 0;
        }
    }

    #endregion

    #region Movement

    private void CheckTargetDistance()
    {
        float distance = Vector3.Distance(transform.position, targetPoint.position);
        if (distance < stopDistance)
        {
            PickNewTarget();
        }
    }

    private void UpdateAnimation()
    {
        if (isMoving)
        {
            animator.SetBool ("IsWalking", true);
        }
        else
        {
            animator.SetBool ("IsWalking", false);
        }
    }

    private void CheckPedestrianLights()
    { 
        if (activeSignalObject != null)
        {
            if (activeSignalObject.GetComponent<LightTrafficSystem>().IsRed())
            {
                navMeshAgent.speed = moveSpeed;
                activeSignalObject = null;
            }
        }
    }

    #endregion

    #region Target Selection

    /// <summary>
    /// Picks a new random target point within range.
    /// </summary>
    private void PickNewTarget()
    {
        targetPoint = CrowdSpawner._targetPositions[Random.Range(0, CrowdSpawner._targetPositions.Count)];
        navMeshAgent.SetDestination(targetPoint.position);
    }

    #endregion
}