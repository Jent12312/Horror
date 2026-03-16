using Unity.Netcode;
using UnityEngine;
using Sirenix.OdinInspector;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private CharacterDataSO characterData;
    [BoxGroup("References")]
    [SerializeField] private InputReader inputReader; 
    [BoxGroup("References")]
    [SerializeField] private Transform cameraHolder;
    [BoxGroup("References")]
    [SerializeField] private Transform groundCheck;
    [BoxGroup("References")]
    [SerializeField] private Animator animator;
    [BoxGroup("Effects")]
    [SerializeField] private SprintEffect sprintEffect;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool isGrounded;
    private float verticalLookRotation = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            inputReader.MoveEvent += HandleMoveInput;
            inputReader.LookEvent += HandleLookInput;
            inputReader.SprintEvent += HandleSprintInput;
            inputReader.JumpEvent += HandleJumpInput;
        }

        if (IsServer)
        {
            if (SpawnManager.Instance != null)
            {
                Transform point = SpawnManager.Instance.GetNextSpawnPoint();
                rb.position = point.position;
                rb.rotation = point.rotation;
            }
            else
            {
                // Если Instance нет (например, сцена еще грузится), пробуем найти "в лоб"
                var sm = FindFirstObjectByType<SpawnManager>();
                if (sm != null)
                {
                    Transform point = sm.GetNextSpawnPoint();
                    rb.position = point.position;
                    rb.rotation = point.rotation;
                }
                else
                {
                    Debug.LogError("CRITICAL: SpawnManager not found! Player spawned at (0,0,0)");
                }
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.MoveEvent -= HandleMoveInput;
            inputReader.LookEvent -= HandleLookInput;
            inputReader.SprintEvent -= HandleSprintInput;
            inputReader.JumpEvent -= HandleJumpInput;
        }
    }

    private void HandleMoveInput(Vector2 input) => moveInput = input;
    private void HandleLookInput(Vector2 input) => lookInput = input;
    private void HandleSprintInput(bool active) => isSprinting = active;

    private void HandleJumpInput()
    {
        if (isGrounded)
        {
            Jump();
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        HandleRotation();
        UpdateAnimations();
        HandleSprintEffects();
    }

    private void HandleSprintEffects()
    {
        bool isActivelySprinting = isSprinting
                                   && moveInput.magnitude > 0.1f
                                   && isGrounded;

        // Один вызов — SprintEffect сам проверяет дубли
        sprintEffect?.SetSprinting(isActivelySprinting);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        CheckGround();
        Move();
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speedMultiplier = isSprinting ? 2f : 1f;

        float targetX = moveInput.x * speedMultiplier;
        float targetY = moveInput.y * speedMultiplier;

        animator.SetFloat("MoveX", targetX, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", targetY, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
    }

    private void Jump()
    {
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;

        Vector3 currentVelocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVelocity.x, characterData.jumpForce, currentVelocity.z);
    }

    private void HandleRotation()
    {
        float mouseX = lookInput.x * characterData.rotationSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = lookInput.y * characterData.rotationSensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        cameraHolder.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, characterData.groundCheckRadius, characterData.groundLayer);
    }

    private void Move()
    {
        float currentSpeed = isSprinting ? characterData.sprintSpeed : characterData.moveSpeed;
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = targetVelocity;
    }
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, characterData.groundCheckRadius);
        }
    }
}