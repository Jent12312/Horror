using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using DG.Tweening;

public enum ItemHoldType { smallOneHanded, LargeTwoHanded }

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class InteractableBase : NetworkBehaviour
{
    public string itemName = "Item";
    public ItemHoldType holdType;

    [Header("Grip Points")]
    public Transform rightHandGrip;
    public Transform leftHandGrip;

    // Храним NetworkObjectId игрока, который несет предмет
    public NetworkVariable<ulong> carrierNetworkObjectId = new NetworkVariable<ulong>(ulong.MaxValue);

    private Rigidbody rb;
    private Collider col;
    private NetworkTransform netTransform;
    private Transform currentHoldPoint;
    private bool isAnimatingPickup = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        netTransform = GetComponent<NetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        carrierNetworkObjectId.OnValueChanged += OnCarrierChanged;

        // Если кто-то уже несет предмет при нашем подключении
        if (carrierNetworkObjectId.Value != ulong.MaxValue)
        {
            FindHoldPoint(carrierNetworkObjectId.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        carrierNetworkObjectId.OnValueChanged -= OnCarrierChanged;
    }

    // Вызывается СЕРВЕРОМ
    public void PickUp(ulong playerNetworkObjectId)
    {
        // 1. Устанавливаем ID носителя (это вызовет OnCarrierChanged у всех)
        carrierNetworkObjectId.Value = playerNetworkObjectId;

        // 2. Отключаем физику и NetworkTransform
        SetPhysicsClientRpc(false);
    }

    // Вызывается СЕРВЕРОМ
    public void Drop(Vector3 dropPosition, Vector3 dropForce)
    {
        // 1. Очищаем ID (куб перестанет прилипать к руке)
        carrierNetworkObjectId.Value = ulong.MaxValue;
        currentHoldPoint = null;

        // 2. Включаем NetworkTransform и физику обратно
        SetPhysicsClientRpc(true);

        // 3. Телепортируем и кидаем
        if (netTransform != null)
        {
            netTransform.Teleport(dropPosition, Quaternion.identity, transform.localScale);
        }
        else
        {
            transform.position = dropPosition;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dropForce, ForceMode.Impulse);
    }

    [ClientRpc]
    private void SetPhysicsClientRpc(bool isEnabled)
    {
        rb.isKinematic = !isEnabled;
        col.enabled = isEnabled;

        if (netTransform != null) netTransform.enabled = isEnabled;

        // Если мы только что отключили физику (значит нас подобрали)
        if (!isEnabled && currentHoldPoint != null)
        {
            PlayPickupAnimation();
        }
    }

    private void PlayPickupAnimation()
    {
        isAnimatingPickup = true;

        // DOTween: Летим в точку currentHoldPoint за 0.2 секунды
        // Используем DOMove (глобально), так как объект не child
        transform.DOMove(currentHoldPoint.position, 0.2f).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(currentHoldPoint.rotation, 0.2f);

        // После завершения анимации включаем жесткую привязку
        DOVirtual.DelayedCall(0.2f, () => {
            isAnimatingPickup = false;
        });
    }

    private void OnCarrierChanged(ulong oldId, ulong newId)
    {
        if (newId == ulong.MaxValue)
        {
            currentHoldPoint = null;
        }
        else
        {
            FindHoldPoint(newId);
        }
    }

    private void FindHoldPoint(ulong playerNetId)
    {
        // Ищем объект игрока по его NetworkObjectId
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out NetworkObject playerObj))
        {
            var interaction = playerObj.GetComponent<PlayerInteraction>();
            if (interaction != null && interaction.HoldPoint != null)
            {
                currentHoldPoint = interaction.HoldPoint;
            }
        }
    }

    private void LateUpdate()
    {
        if (isAnimatingPickup) return;

        if (carrierNetworkObjectId.Value != ulong.MaxValue && currentHoldPoint != null)
        {
            transform.position = currentHoldPoint.position;
            transform.rotation = currentHoldPoint.rotation;
        }
    }
}