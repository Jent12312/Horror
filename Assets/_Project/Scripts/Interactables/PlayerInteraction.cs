using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private PlayerEquipment equipment; // Ссылка на новый скрипт
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Transform holdPoint;

    public Transform HoldPoint => holdPoint;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.ToggleLightEvent += () => equipment.ToggleStone(); // Камень
        inputReader.InteractEvent += HandleInteract; // ЛКМ
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.ToggleLightEvent -= () => equipment.ToggleStone();
            inputReader.InteractEvent -= HandleInteract;
        }
    }

    private void HandleInteract()
    {
        // Если уже что-то несем - бросаем
        if (equipment.CurrentItem != null)
        {
            DropItem();
        }
        // Иначе пытаемся поднять
        else
        {
            TryPickUp();
        }
    }

    private void TryPickUp()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent(out InteractableBase item))
            {
                if (item.carrierNetworkObjectId.Value == ulong.MaxValue)
                {
                    RequestPickUpServerRpc(item.NetworkObjectId);
                }
            }
        }
    }

    private void DropItem()
    {
        Vector3 origin = cameraTransform.transform.position + cameraTransform.transform.forward * 0.5f;
        Vector3 direction = cameraTransform.transform.forward;

        RequestDropServerRpc(origin, direction);
    }

    [ServerRpc]
    private void RequestPickUpServerRpc(ulong itemNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkObjectId, out NetworkObject netObj))
        {
            var item = netObj.GetComponent<InteractableBase>();
            if (item != null && item.carrierNetworkObjectId.Value == ulong.MaxValue)
            {
                item.PickUp(this.NetworkObjectId);
                SetCurrentItemClientRpc(itemNetworkObjectId);
                equipment.SetCarriedItem(item);
            }
        }
    }

    [ServerRpc]
    private void RequestDropServerRpc(Vector3 dropOrigin, Vector3 dropDirection)
    {
        if (equipment.CurrentItem == null) return;

        Vector3 force = dropDirection * 8f;
        equipment.CurrentItem.Drop(dropOrigin, force);

        ClearCurrentItemClientRpc();
    }

    [ClientRpc]
    private void SetCurrentItemClientRpc(ulong itemId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject netObj))
        {
            var interactable = netObj.GetComponent<InteractableBase>();
            equipment.SetCarriedItem(interactable); 
        }
    }

    [ClientRpc]
    private void ClearCurrentItemClientRpc() => equipment.ClearCarriedItem();
}