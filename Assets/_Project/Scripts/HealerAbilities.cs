using Unity.Netcode;
using UnityEngine;

public class HealerAbilities : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private float healAmount = 15f;
    [SerializeField] private float healCooldown = 10f;
    [SerializeField] private float healRange = 3f;
    [SerializeField] private LayerMask playerLayer; // Только выжившие

    private float nextHealTime = 0f;
    private float regenAccumulator = 0f;
    private PlayerHealth myHealth;

    private void Awake() => myHealth = GetComponent<PlayerHealth>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event += TryHealTarget; // Q
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.Ability1Event -= TryHealTarget;
    }

    private void TryHealTarget()
    {
        if (Time.time < nextHealTime) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, healRange, playerLayer);
        foreach (var hit in hits)
        {
            // Не лечим самого себя этой способностью (только союзников)
            if (hit.gameObject != this.gameObject && hit.TryGetComponent(out PlayerHealth allyHealth))
            {
                if (allyHealth.isAlive.Value && allyHealth.currentHealth.Value < 100) // или maxHealth
                {
                    allyHealth.HealServerRpc((int)healAmount);
                    nextHealTime = Time.time + healCooldown;
                    
                    // TODO: Play Heal VFX/SFX locally
                    Debug.Log("Healed ally!");
                    return; // Лечим только одного (ближайшего)
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Пассивная регенерация (25 хп в минуту) работает только на сервере
        if (!IsServer || !myHealth.isAlive.Value) return;

        regenAccumulator += (25f / 60f) * Time.fixedDeltaTime;
        if (regenAccumulator >= 1f)
        {
            myHealth.HealServerRpc(1); // Добавляем по 1 хп
            regenAccumulator -= 1f;
        }
    }
}