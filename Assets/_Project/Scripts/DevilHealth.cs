using Unity.Netcode;
using UnityEngine;
using System;

public class DevilHealth : NetworkBehaviour
{
    [SerializeField] private DevilDataSO data;
    
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public event Action OnDied;

    public override void OnNetworkSpawn()
    {
        if (IsServer) currentHealth.Value = data.maxHealth;
    }

    // Прямой урон дьяволу в игре не предусмотрен диздоком, 
    // но метод оставляем на будущее (например, если изменят механику)
    // Урон будет получать Сердце Дьявола.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(int damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke();
        Debug.Log("[Server] DEVIL DIED! HUMANS WIN.");
        // GameManager.Instance.EndGame(GameResult.HumansWin_KilledDevil);
    }
}