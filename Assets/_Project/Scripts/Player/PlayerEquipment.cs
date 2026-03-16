using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerEquipment : NetworkBehaviour
{
    public event Action<bool> OnStoneToggled;
    public event Action<bool> OnItemCarriedChanged;

    public NetworkVariable<bool> isStoneActive = new NetworkVariable<bool>(false);

    public InteractableBase CurrentItem { get; private set; }

    public override void OnNetworkSpawn()
    {
        isStoneActive.OnValueChanged += (prev, current) => { OnStoneToggled?.Invoke(current); };

        OnStoneToggled?.Invoke(isStoneActive.Value);
    }

    public void ToggleStone()
    {
        if (CurrentItem != null) return; 
        ToggleStoneServerRpc(!isStoneActive.Value);
    }

    [ServerRpc]
    private void ToggleStoneServerRpc(bool state) => isStoneActive.Value = state;

    public void ForceHideStone()
    {
        if (IsServer) isStoneActive.Value = false;
        else ToggleStoneServerRpc(false);
    }

    public void SetCarriedItem(InteractableBase item)
    {
        CurrentItem = item;
        OnItemCarriedChanged?.Invoke(item != null);

        if (item != null) ForceHideStone(); 
    }

    public void ClearCarriedItem()
    {
        CurrentItem = null;
        OnItemCarriedChanged?.Invoke(false);
    }
}