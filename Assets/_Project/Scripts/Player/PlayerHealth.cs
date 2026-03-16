using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private CharacterDataSO characterData;
    
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    public event Action<int> OnHealthChanged;
    public event Action OnDied;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = characterData.maxHealth;
        }

        currentHealth.OnValueChanged += (oldVal, newVal) => OnHealthChanged?.Invoke(newVal);
        isAlive.OnValueChanged += (oldVal, newVal) => { if (!newVal) OnDied?.Invoke(); };
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!isAlive.Value) return;

        currentHealth.Value -= damage;
        
        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            isAlive.Value = false;
            DieClientRpc();
            // В будущем: GameManager.Instance.CheckPlayersAlive();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HealServerRpc(int amount)
    {
        if (!isAlive.Value) return;
        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, characterData.maxHealth);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DieClientRpc()
    {
        if (IsOwner)
        {
            Debug.Log("YOU DIED!");
            // Отключить управление, показать экран смерти
            if (TryGetComponent(out PlayerController controller)) controller.enabled = false;
        }
    }
}