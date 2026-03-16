using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
{
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> LookEvent;
    public event UnityAction JumpEvent;
    public event UnityAction InteractEvent;
    public event UnityAction InteractCancelledEvent;
    public event UnityAction ToggleMicEvent;
    public event UnityAction ToggleLightEvent;
    public event UnityAction<bool> SprintEvent;

    // НОВЫЕ СОБЫТИЯ ДЛЯ СПОСОБНОСТЕЙ
    public event UnityAction BasicAttackEvent; // ЛКМ
    public event UnityAction HeavyAttackEvent; // ПКМ
    public event UnityAction Ability1Event;    // Q
    public event UnityAction Ability2Event;    // E
    public event UnityAction Ability3Event;    // R
    public event UnityAction Ability4Event;    // F

    private PlayerInputActions inputActions;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        EnableInput();
    }

    private void OnDisable() => DisableInput();

    [Button] public void EnableInput() => inputActions.Enable();
    [Button] public void DisableInput() => inputActions.Disable();

    public void OnMove(InputAction.CallbackContext context) => MoveEvent?.Invoke(context.ReadValue<Vector2>());
    public void OnLook(InputAction.CallbackContext context) => LookEvent?.Invoke(context.ReadValue<Vector2>());
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) JumpEvent?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) InteractEvent?.Invoke();
        if (context.phase == InputActionPhase.Canceled) InteractCancelledEvent?.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) SprintEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled) SprintEvent?.Invoke(false);
    }

    public void OnToggleMic(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) ToggleMicEvent?.Invoke();
    }

    public void OnToggleLight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) ToggleLightEvent?.Invoke();
    }

    // НОВЫЕ МЕТОДЫ ИНТЕРФЕЙСА (Убедитесь, что сгенерировали их в C# классе Input System)
    public void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) BasicAttackEvent?.Invoke();
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) HeavyAttackEvent?.Invoke();
    }
    public void OnAbility1(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) Ability1Event?.Invoke();
    }
    public void OnAbility2(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) Ability2Event?.Invoke();
    }
    public void OnAbility3(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) Ability3Event?.Invoke();
    }
    public void OnAbility4(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) Ability4Event?.Invoke();
    }
}