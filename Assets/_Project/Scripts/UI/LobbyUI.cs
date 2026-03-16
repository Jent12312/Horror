using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ConnectionManager connectionManager;

    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_InputField ipInputField;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        connectButton.onClick.AddListener(OnConnectClicked);
    }

    private void OnHostClicked()
    {
        connectionManager.StartHost();
        HideUI();
    }

    private void OnConnectClicked()
    {
        string ip = ipInputField.text;
        connectionManager.StartClient(ip);
        HideUI();
    }

    private void HideUI()
    {
        hostButton.gameObject.SetActive(false);
        connectButton.gameObject.SetActive(false);
        ipInputField.gameObject.SetActive(false);
    }
}