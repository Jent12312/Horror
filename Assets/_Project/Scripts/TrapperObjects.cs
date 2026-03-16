using Unity.Netcode;
using UnityEngine;

// --- КАПКАН ---
public class BearTrap : NetworkBehaviour
{
    [SerializeField] private float slowDuration = 3.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Если наступил Дьявол
        if (other.TryGetComponent(out DevilController devil))
        {
            devil.ApplySlowServerRpc(slowDuration);
            SnapTrapClientRpc();
            
            // Уничтожаем капкан через секунду после срабатывания
            Invoke(nameof(DespawnTrap), 1f); 
        }
    }

    [ClientRpc]
    private void SnapTrapClientRpc()
    {
        // TODO: Проиграть анимацию захлопывания и звук капкана
        Debug.Log("SNAP! Devil is trapped!");
    }

    private void DespawnTrap() => GetComponent<NetworkObject>().Despawn();
}

// --- СТЕНА ---
public class DestructibleWall : NetworkBehaviour
{
    public NetworkVariable<int> wallHealth = new NetworkVariable<int>(3);

    // Этот метод будет вызываться Дьяволом при его атаке (BasicAttack)
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        wallHealth.Value -= 1; // 1 удар = минус 1 прочность
        if (wallHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}