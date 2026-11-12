using UnityEngine;
using System;

namespace HourPeak.Addition
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        #region Configuration

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
        [SerializeField] private float enterExitTime = 1.5f;

        [Header("Условие бега")]
        public bool isRunning = false;

        #endregion

        #region Fields

        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        private Vector3 lookDirection;
        private bool isMoving;
        private BusSplineMovement currentTransport;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rb ??= GetComponent<Rigidbody>();
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (currentTransport != null)
            {
                transform.position = currentTransport.transform.position + Vector3.up * 2;
                return;
            }

            HandleGroundCheck();
            UpdateAnimation();
        }

        #endregion

        #region Movement

        private void HandleGroundCheck()
        {
            // Простая проверка: если скорость по Y близка к 0 и мы не падаем
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.1f;
            if (isGrounded && velocity.y < 0) velocity.y = -0.01f;
        }

        public void Move(Vector2 moveInput)
        {
            if (currentTransport != null)
            {
                return;
            }

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();
            Vector3 target = forward * moveInput.y + right * moveInput.x;

            target.Normalize();
            lookDirection = target;

            currentSpeed = walkSpeed;
            if (isRunning) currentSpeed = runSpeed;

            Vector3 moveVector = lookDirection * currentSpeed;
            rb.MovePosition(rb.position + moveVector * Time.deltaTime);
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

        #endregion

        #region Bus Mobility

        private void OnTriggerEnter(Collider collider)
        {
            BusSplineMovement bus = collider.GetComponent<BusSplineMovement>();
            if (bus != null)
            {
                if (bus.IsWaitingAtBusStop)
                {
                    cameraTransform.gameObject.SetActive(false);
                    bus._camera.SetActive(true);
                    currentTransport = bus;
                }
            }
        }

        public void BusExit()
        {
            cameraTransform.gameObject.SetActive(true);
            currentTransport._camera.SetActive(false);
            currentTransport = null;
        }

        #endregion

        #region Input

        /// <summary>
        /// Запрос прыжка (вызывается из UI)
        /// </summary>
        public void RequestJump()
        {
            if (isGrounded)
            {
                velocity.y = jumpSpeed;
                animator?.SetTrigger("IsJumping");
            }
        }

        /// <summary>
        /// Показывать ли кнопку выхода (игрок в автобусе и автобус стоит на остановке)
        /// </summary>
        public bool ShouldShowBusExitButton()
        {
            if (currentTransport == null)
                return false;

            return currentTransport.IsWaitingAtBusStop;
        }

        #endregion
    }
}