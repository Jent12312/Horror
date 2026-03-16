using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DevilController : NetworkBehaviour
{
    [SerializeField] private DevilDataSO devilData;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform cameraHolder;
    
    public NetworkVariable<bool> isAggressionActive = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isSlowed = new NetworkVariable<bool>(false);

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalLookRotation = 0f;

    private void Awake() => rb = GetComponent<Rigidbody>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent += HandleMoveInput;
        inputReader.LookEvent += HandleLookInput;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent -= HandleMoveInput;
        inputReader.LookEvent -= HandleLookInput;
    }

    private void HandleMoveInput(Vector2 input) => moveInput = input;
    private void HandleLookInput(Vector2 input) => lookInput = input;

    private void Update()
    {
        if (!IsOwner) return;
        HandleRotation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
    }

    private void HandleRotation()
    {
        float mouseX = lookInput.x * devilData.rotationSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = lookInput.y * devilData.rotationSensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        cameraHolder.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void Move()
    {
        float currentSpeed = isAggressionActive.Value ? devilData.aggressionSpeed : devilData.moveSpeed;
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        if (isSlowed.Value) currentSpeed *= 0.5f;

        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = targetVelocity;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplySlowServerRpc(float duration)
    {
        if (isSlowed.Value) return;
        StartCoroutine(SlowRoutine(duration));
    }

    private System.Collections.IEnumerator SlowRoutine(float duration)
    {
        isSlowed.Value = true;
        yield return new WaitForSeconds(duration);
        isSlowed.Value = false;
    }
}