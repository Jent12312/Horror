using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.enabled = true;
            audioListener.enabled = true;

            playerCamera.gameObject.tag = "MainCamera";
        }
        else
        {
            playerCamera.enabled = false;
            audioListener.enabled = false;
        }
    }
}