using Unity.Netcode;
using UnityEngine;

public class ExorcistAbilities : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject teleportDevicePrefab;
    [SerializeField] private Transform placePoint;

    private NetworkVariable<ulong> spawnedDeviceId = new NetworkVariable<ulong>(ulong.MaxValue);
    private bool devicePlaced = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event += TryPlaceDevice; // Q
        inputReader.Ability2Event += TryTeleport;    // E
        // Чтение заклинания (Ability3 / R) добавим в Этапе 5, так как нужен Алтарь
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event -= TryPlaceDevice;
        inputReader.Ability2Event -= TryTeleport;
    }

    private void TryPlaceDevice()
    {
        if (!devicePlaced) 
        {
            PlaceDeviceServerRpc(placePoint.position);
            devicePlaced = true;
        }
    }

    private void TryTeleport()
    {
        if (spawnedDeviceId.Value != ulong.MaxValue)
        {
            TeleportServerRpc();
            devicePlaced = false; // После ТП устройство ломается (по желанию баланса)
        }
    }

    [ServerRpc]
    private void PlaceDeviceServerRpc(Vector3 pos)
    {
        if (spawnedDeviceId.Value != ulong.MaxValue) return;

        GameObject device = Instantiate(teleportDevicePrefab, pos, Quaternion.identity);
        NetworkObject netObj = device.GetComponent<NetworkObject>();
        netObj.Spawn();
        
        spawnedDeviceId.Value = netObj.NetworkObjectId;
    }

    [ServerRpc]
    private void TeleportServerRpc()
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spawnedDeviceId.Value, out NetworkObject netObj))
        {
            // Телепортируем игрока (используем возможности NGO)
            transform.position = netObj.transform.position;
            
            // Уничтожаем прибор после использования (или оставляем, если он многоразовый)
            netObj.Despawn();
            spawnedDeviceId.Value = ulong.MaxValue;
        }
    }
}