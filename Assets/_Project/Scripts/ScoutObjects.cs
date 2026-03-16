using Unity.Netcode;
using UnityEngine;
using System.Collections;

// --- СВЕЧА ---
public class CandleObject : NetworkBehaviour
{
    [SerializeField] private float burnDuration = 60.0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer) StartCoroutine(BurnDownRoutine());
    }

    private IEnumerator BurnDownRoutine()
    {
        yield return new WaitForSeconds(burnDuration);
        GetComponent<NetworkObject>().Despawn();
    }
}

// --- РАСТЯЖКА ---
public class TripwireObject : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out DevilController devil))
        {
            // Ищем Скаута на сервере
            var allPlayers = FindObjectsByType<ScoutAbilities>(FindObjectsSortMode.None);
            foreach (var scout in allPlayers)
            {
                // Отправляем сигнал Скауту (ClientRpc сработает только у Владельца-Скаута внутри метода)
                scout.RevealDevilPositionClientRpc(devil.transform.position, 3.0f);
            }
            
            // Ломаем растяжку
            GetComponent<NetworkObject>().Despawn();
        }
    }
}