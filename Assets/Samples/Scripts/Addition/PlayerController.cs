using UnityEngine;
using System;

namespace HourPeak.Addition
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Компоненты")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform cameraTransform;

        [Header("Движение пешком")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpSpeed = 5f;

        [Header("Транспорт")]
        [SerializeField] private float busSpeed = 20f;
        [SerializeField] private float enterExitTime = 1.5f;
        [SerializeField] private LayerMask transportLayer;
        [SerializeField] private float transportInteractDistance = 3f;

        [Header("Анимация")]
        [SerializeField] private int upperBodyLayerIndex = 1;

        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        private Vector3 lookDirection;
        private bool isMoving;
        private Vector2 joystickInput;

        public enum TransportMode { Walking, Bus, Train }
        private TransportMode currentMode = TransportMode.Walking;
        private bool isEntering = false;
        private bool isExiting = false;
        private Transform currentTransport;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveZ = Animator.StringToHash("MoveZ");
        private static readonly int IsLookingForRoute = Animator.StringToHash("IsLookingForRoute");
        private static readonly int TransportModeHash = Animator.StringToHash("TransportMode");
        private static readonly int IsEnteringHash = Animator.StringToHash("IsEntering");
        private static readonly int IsExitingHash = Animator.StringToHash("IsExiting");

        public event Action<TransportMode> OnTransportModeChanged;
        public event Action OnTransportEnter;
        public event Action OnTransportExit;
        public event Action<GameObject> OnNearTransport;

        private void Awake()
        {
            rb ??= GetComponent<Rigidbody>();
            animator ??= GetComponent<Animator>();
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            if (animator != null && upperBodyLayerIndex < animator.layerCount)
                animator.SetLayerWeight(upperBodyLayerIndex, 0f);
        }

        private void Update()
        {
            if (isEntering || isExiting) return;

            HandleGroundCheck();
            HandleTransportInteraction();
            
            if (currentMode == TransportMode.Walking)
            {
                HandleWalking();
            }

            UpdateAnimation();
        }

        private void HandleGroundCheck()
        {
            // Простая проверка: если скорость по Y близка к 0 и мы не падаем
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
            if (isGrounded && velocity.y < 0) velocity.y = -0.01f;
        }

        private void HandleWalking()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0f; right.y = 0f;
                forward.Normalize(); right.Normalize();
                Vector3 target = (forward * vertical + right * horizontal);
                
                if (target.sqrMagnitude > 0.01f)
                {
                    target.Normalize();
                    lookDirection = target;
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                }
            }

            currentSpeed = walkSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = sprintSpeed;
            else if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = runSpeed;

            Vector3 moveVector = isMoving ? lookDirection * currentSpeed : Vector3.zero;
            rb.MovePosition(rb.position + moveVector * Time.deltaTime);
            HandleGravity();
        }

        private void HandleGravity()
        {
            velocity.y += gravity * Time.deltaTime;
            rb.MovePosition(rb.position + new Vector3(0, velocity.y * Time.deltaTime, 0));
        }

        private void HandleTransportInteraction()
        {
            if (currentMode != TransportMode.Walking) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, transportInteractDistance, transportLayer);
            GameObject nearestTransport = null;
            float nearestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestTransport = hit.gameObject;
                }
            }

            OnNearTransport?.Invoke(nearestTransport);

            if (nearestTransport != null && Input.GetKeyDown(KeyCode.E))
            {
                TryEnterTransport(nearestTransport);
            }
        }

        private void TryEnterTransport(GameObject transport)
        {
            var busMovement = transport.GetComponent<BusSplineMovement>();

            if (busMovement != null)
            {
                StartCoroutine(EnterTransport(TransportMode.Bus, transport.transform));
            }
        }

        private System.Collections.IEnumerator EnterTransport(TransportMode mode, Transform transportTransform)
        {
            isEntering = true;
            animator?.SetBool(IsEnteringHash, true);

            yield return new WaitForSeconds(enterExitTime);

            currentMode = mode;
            currentTransport = transportTransform;
            isEntering = false;
            animator?.SetBool(IsEnteringHash, false);

            OnTransportEnter?.Invoke();
            OnTransportModeChanged?.Invoke(currentMode);
        }

        public void TryExitTransport()
        {
            if (currentMode == TransportMode.Walking) return;
            if (isExiting) return;

            StartCoroutine(ExitTransport());
        }

        private System.Collections.IEnumerator ExitTransport()
        {
            isExiting = true;
            animator?.SetBool(IsExitingHash, true);

            Vector3 exitPosition = currentTransport.position + currentTransport.forward * 3f;
            Vector3 exitRotation = currentTransport.forward;

            yield return new WaitForSeconds(enterExitTime);

            transform.position = exitPosition;
            transform.rotation = Quaternion.LookRotation(exitRotation);

            currentMode = TransportMode.Walking;
            currentTransport = null;
            isExiting = false;
            animator?.SetBool(IsExitingHash, false);

            OnTransportExit?.Invoke();
            OnTransportModeChanged?.Invoke(currentMode);
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            float normalizedSpeed = Mathf.InverseLerp(walkSpeed, sprintSpeed, currentSpeed);
            
            animator.SetFloat(Speed, isMoving ? normalizedSpeed : 0f);
            animator.SetBool(IsMoving, isMoving);
            animator.SetFloat(MoveX, isMoving ? lookDirection.x : 0f);
            animator.SetFloat(MoveZ, isMoving ? lookDirection.z : 0f);

            int modeValue = (int)currentMode;
            animator.SetInteger(TransportModeHash, modeValue);
        }

        public void StartLookingForRoute()
        {
            if (animator == null) return;
            animator.SetBool(IsLookingForRoute, true);
            if (upperBodyLayerIndex < animator.layerCount)
                animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            currentSpeed = walkSpeed * 0.5f;
        }

        public void StopLookingForRoute()
        {
            if (animator == null) return;
            animator.SetBool(IsLookingForRoute, false);
            if (upperBodyLayerIndex < animator.layerCount)
                animator.SetLayerWeight(upperBodyLayerIndex, 0f);
            currentSpeed = walkSpeed;
        }

        public void TriggerInteractAnimation() => animator?.SetTrigger("Interact");
        public void TriggerWaitAnimation() => animator?.SetTrigger("Wait");

        public TransportMode GetCurrentMode() => currentMode;
        public bool IsInTransport() => currentMode != TransportMode.Walking;
        public bool IsOnBus() => currentMode == TransportMode.Bus;
        public bool IsOnTrain() => currentMode == TransportMode.Train;

        /// <summary>
        /// Ввод джойстика движения (для мобильного управления)
        /// </summary>
        public Vector2 JoystickMoveInput
        {
            get => joystickInput;
            set
            {
                joystickInput = value;
                if (value.sqrMagnitude > 0.01f)
                {
                    lookDirection = new Vector3(value.x, 0f, value.y).normalized;
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                }
            }
        }

        /// <summary>
        /// Запрос прыжка (вызывается из UI)
        /// </summary>
        public void RequestJump()
        {
            if (isGrounded)
            {
                velocity.y = jumpSpeed;
                animator?.SetTrigger("Jump");
            }
        }

        /// <summary>
        /// Показывать ли кнопку выхода (игрок в автобусе и автобус стоит на остановке)
        /// </summary>
        public bool ShouldShowBusExitButton
        {
            get
            {
                if (currentMode != TransportMode.Bus || currentTransport == null)
                    return false;

                var busMovement = currentTransport.GetComponent<BusSplineMovement>();
                return busMovement != null && busMovement.IsWaitingAtBusStop;
            }
        }

        /// <summary>
        /// Выход из автобуса (вызывается из UI)
        /// </summary>
        public void ExitFromBus()
        {
            if (currentMode != TransportMode.Bus) return;
            TryExitTransport();
        }

        public void SetTransportMode(TransportMode mode)
        {
            if (currentMode == mode) return;
            currentMode = mode;
            OnTransportModeChanged?.Invoke(currentMode);
        }

        public float GetTransportSpeed()
        {
            return currentMode switch
            {
                TransportMode.Bus => busSpeed,
                _ => walkSpeed
            };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, transportInteractDistance);

            if (currentTransport != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentTransport.position, 1f);
            }
        }

        #region Editor Helpers

        private void OnGUI()
        {
            // Отладочная информация (можно отключить в релизе)
            #if UNITY_EDITOR
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.Label($"Speed: {currentSpeed:F2}");
            GUILayout.Label($"Mode: {currentMode}");
            GUILayout.Label($"Moving: {isMoving}");
            GUILayout.Label($"Grounded: {isGrounded}");
            GUILayout.EndArea();
            #endif
        }

        #endregion
    }
}