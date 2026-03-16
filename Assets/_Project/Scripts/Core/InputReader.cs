using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
{
    // События, на которые будут подписываться скрипты
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> LookEvent;
    public event UnityAction JumpEvent;
    public event UnityAction InteractEvent;
    public event UnityAction InteractCancelledEvent; // Для того чтобы знать когда отпустили кнопку
    public event UnityAction ToggleMicEvent; // Микрофон
    public event UnityAction ToggleLightEvent; // Фонарик
    public event UnityAction<bool> SprintEvent; // true - нажал, false - отпустил

    private PlayerInputActions inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            // Этот класс сам становится слушателем событий
            inputActions.Player.SetCallbacks(this);
        }

        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
    }

    [Button]
    public void EnableInput()
    {
        inputActions.Enable();
    }

    [Button]
    public void DisableInput()
    {
        inputActions.Disable();
    }

    // --- Реализация интерфейса IPlayerActions (автоматически вызывается Unity) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        { 
            JumpEvent?.Invoke();
            Debug.Log("Jump Event Fired!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            InteractEvent?.Invoke();

        if (context.phase == InputActionPhase.Canceled)
            InteractCancelledEvent?.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            SprintEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            SprintEvent?.Invoke(false);
    }

    public void OnToggleMic(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            ToggleMicEvent?.Invoke();
    }

    public void OnToggleLight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            ToggleLightEvent?.Invoke();
    }
}