using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Перетащи сюда префаб Игрока в инспекторе!

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 1. Сразу спавним персонажа для самого Хоста
            SpawnPlayer(NetworkManager.ServerClientId);

            // 2. Подписываемся на подключение остальных игроков
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Когда новый клиент полностью подключился - даем ему персонажа
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        Transform spawnPoint = transform; // По умолчанию спавним там же, где стоит GameManager

        if (SpawnManager.Instance != null)
        {
            spawnPoint = SpawnManager.Instance.GetNextSpawnPoint();
        }
        else
        {
            Debug.LogWarning("SpawnManager не найден! Игрок появится в центре GameManager'а.");
        }

        // Инстанцируем префаб
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // ВАЖНО: Делаем этот объект "телом" подключившегося клиента
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}