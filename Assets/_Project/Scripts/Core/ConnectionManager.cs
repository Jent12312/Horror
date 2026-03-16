using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private ushort port = 7777;

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void StartHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", port);

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log($"[ConnectionManager] Host started on port {port}");
            NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
        }
        else Debug.LogError("[ConnectionManager] Failed to start Host!");
    }

    public void StartClient(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) ipAddress = "127.0.0.1";

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);

        Debug.Log($"[ConnectionManager] Connecting to {ipAddress}:{port}...");
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId) => Debug.Log($"[ConnectionManager] Client connected! ClientId: {clientId}");
    private void OnClientDisconnected(ulong clientId) => Debug.Log($"[ConnectionManager] Client disconnected! ClientId: {clientId}");
}