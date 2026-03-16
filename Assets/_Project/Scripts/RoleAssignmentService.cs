using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoleAssignmentService : NetworkBehaviour
{
    public static RoleAssignmentService Instance { get; private set; }

    [Header("Role Prefabs")]
    [SerializeField] private GameObject devilPrefab;
    [SerializeField] private GameObject trapperPrefab;
    [SerializeField] private GameObject healerPrefab;
    [SerializeField] private GameObject exorcistPrefab;
    [SerializeField] private GameObject scoutPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Вызывается только на сервере при старте игры
    public void AssignRolesAndSpawn()
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList.Select(c => c.ClientId).ToList();
        if (clients.Count == 0) return;

        // 1. Выбираем случайного игрока на роль Дьявола
        int devilIndex = Random.Range(0, clients.Count);
        ulong devilClientId = clients[devilIndex];
        clients.RemoveAt(devilIndex);

        SpawnPlayerPrefab(devilPrefab, devilClientId);

        // 2. Перемешиваем оставшиеся роли выживших
        List<GameObject> humanRoles = new List<GameObject> { trapperPrefab, healerPrefab, exorcistPrefab, scoutPrefab };
        humanRoles = humanRoles.OrderBy(x => Random.value).ToList();

        // 3. Раздаем роли остальным игрокам
        for (int i = 0; i < clients.Count; i++)
        {
            // Если игроков меньше 5, кто-то из ролей просто не появится в этой катке
            if (i < humanRoles.Count)
            {
                SpawnPlayerPrefab(humanRoles[i], clients[i]);
            }
        }
    }

    private void SpawnPlayerPrefab(GameObject prefab, ulong clientId)
    {
        // Находим текущий базовый объект игрока и удаляем его (если он был заспавнен автоматически)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
        {
            client.PlayerObject.Despawn(true);
        }

        // Спавним новый классовый префаб
        Transform spawnPoint = SpawnManager.Instance != null ? SpawnManager.Instance.GetNextSpawnPoint() : transform;
        GameObject newPlayer = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}