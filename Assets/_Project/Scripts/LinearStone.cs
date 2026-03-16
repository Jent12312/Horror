using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LinearStone : NetworkBehaviour
{
    [SerializeField] private float activeIntensity = 2.0f;
    [SerializeField] private float activeRange = 6.0f;
    [SerializeField] private float activeDuration = 10.0f;
    [SerializeField] private float cooldownDuration = 20.0f;

    public NetworkVariable<bool> isOnCooldown = new NetworkVariable<bool>(false);

    private Light pointLight;
    private float timer;

    private void Awake()
    {
        pointLight = GetComponent<Light>();
    }

    public override void OnNetworkSpawn()
    {
        isOnCooldown.OnValueChanged += OnCooldownStateChanged;
        if (transform.parent.TryGetComponent<PlayerEquipment>(out var equipment))
        {
            equipment.OnStoneToggled += HandleEquipmentToggle;
            UpdateVisuals(!equipment.isStoneActive.Value); // Инициализация
        }
        UpdateVisuals(isOnCooldown.Value);
    }

    private void HandleEquipmentToggle(bool isActive)
    {
        // Если игрок выключил камень — сбрасываем таймер
        if (!isActive) timer = 0f;
    }

    public override void OnNetworkDespawn()
    {
        isOnCooldown.OnValueChanged -= OnCooldownStateChanged;
        if (transform.parent.TryGetComponent<PlayerEquipment>(out var equipment))
        {
            equipment.OnStoneToggled -= HandleEquipmentToggle;
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Логика таймеров только на сервере

        if (isOnCooldown.Value)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isOnCooldown.Value = false;
            }
        }
        else if (gameObject.activeInHierarchy) // Если игрок включил свет
        {
            timer += Time.deltaTime;
            if (timer >= activeDuration)
            {
                isOnCooldown.Value = true;
                timer = cooldownDuration; // Запускаем кулдаун

            }
        }
    }

    private void OnCooldownStateChanged(bool previous, bool current)
    {
        UpdateVisuals(current);
    }

    private void UpdateVisuals(bool inCooldown)
    {
        // Если в кулдауне, камень тускнеет и краснеет/сереет
        if (inCooldown)
        {
            pointLight.intensity = 0.1f;
            pointLight.color = Color.gray;
        }
        else
        {
            pointLight.intensity = activeIntensity;
            pointLight.range = activeRange;
            pointLight.color = Color.white;
        }
    }
}