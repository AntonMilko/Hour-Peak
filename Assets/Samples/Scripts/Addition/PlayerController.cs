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
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpSpeed = 5f;

        [Header("Транспорт")]
        [SerializeField] private float busSpeed = 20f;
        [SerializeField] private float enterExitTime = 1.5f;
        [SerializeField] private LayerMask transportLayer;
        [SerializeField] private float transportInteractDistance = 3f;

        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        private Vector3 lookDirection;
        private bool isMoving;
        private Vector2 joystickInput;
        public bool isRunning = false;

        public enum TransportMode { Walking, Bus, Train }
        private TransportMode currentMode = TransportMode.Walking;
        private bool isEntering = false;
        private bool isExiting = false;
        private Transform currentTransport;

        public event Action<TransportMode> OnTransportModeChanged;
        public event Action OnTransportEnter;
        public event Action OnTransportExit;
        public event Action<GameObject> OnNearTransport;

        private void Awake()
        {
            rb ??= GetComponent<Rigidbody>();
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (isEntering || isExiting) return;

            HandleGroundCheck();
            HandleTransportInteraction();
        }

        private void FixedUpdate()
        {
            UpdateAnimation();
        }

        private void HandleGroundCheck()
        {
            // Простая проверка: если скорость по Y близка к 0 и мы не падаем
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
            if (isGrounded && velocity.y < 0) velocity.y = -0.01f;
        }

        public void Move(Vector2 moveInput)
        {
            if (cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0f; right.y = 0f;
                forward.Normalize(); right.Normalize();
                Vector3 target = forward * moveInput.y + right * moveInput.x;

                target.Normalize();
                lookDirection = target;
            }

            currentSpeed = walkSpeed;
            if (isRunning) currentSpeed = runSpeed;

            Vector3 moveVector = lookDirection * currentSpeed;
            rb.MovePosition(rb.position + moveVector * Time.deltaTime);
            Debug.Log(moveVector);
            HandleGravity();

            isMoving = true;
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

            if (nearestTransport != null)
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

            yield return new WaitForSeconds(enterExitTime);

            currentMode = mode;
            currentTransport = transportTransform;
            isEntering = false;

            OnTransportEnter?.Invoke();
            OnTransportModeChanged?.Invoke(currentMode);
        }

        public void TryExitTransport()
        {
            if (currentMode == TransportMode.Walking) return;
            if (isExiting || isEntering) return;

            StartCoroutine(ExitTransport());
        }

        private System.Collections.IEnumerator ExitTransport()
        {
            isExiting = true;

            Vector3 exitPosition = currentTransport.position + currentTransport.forward * 3f;
            Vector3 exitRotation = currentTransport.forward;

            yield return new WaitForSeconds(enterExitTime);

            transform.position = exitPosition;
            transform.rotation = Quaternion.LookRotation(exitRotation);

            currentMode = TransportMode.Walking;
            currentTransport = null;
            isExiting = false;

            OnTransportExit?.Invoke();
            OnTransportModeChanged?.Invoke(currentMode);
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
    }
}