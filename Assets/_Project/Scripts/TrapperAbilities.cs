using Unity.Netcode;
using UnityEngine;

public class TrapperAbilities : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject bearTrapPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform placePoint; // Точка перед игроком

    private int bearTrapsRemaining = 3;
    private int wallsRemaining = 2;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event += TryPlaceBearTrap; // Q
        inputReader.Ability2Event += TryPlaceWall;     // E
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event -= TryPlaceBearTrap;
        inputReader.Ability2Event -= TryPlaceWall;
    }

    private void TryPlaceBearTrap()
    {
        if (bearTrapsRemaining > 0) PlaceTrapServerRpc(placePoint.position, placePoint.rotation);
    }

    private void TryPlaceWall()
    {
        if (wallsRemaining > 0) PlaceWallServerRpc(placePoint.position, placePoint.rotation);
    }

    [ServerRpc]
    private void PlaceTrapServerRpc(Vector3 pos, Quaternion rot)
    {
        if (bearTrapsRemaining <= 0) return;
        bearTrapsRemaining--;
        GameObject trap = Instantiate(bearTrapPrefab, pos, rot);
        trap.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void PlaceWallServerRpc(Vector3 pos, Quaternion rot)
    {
        if (wallsRemaining <= 0) return;
        wallsRemaining--;
        GameObject wall = Instantiate(wallPrefab, pos, rot);
        wall.GetComponent<NetworkObject>().Spawn();
    }
}