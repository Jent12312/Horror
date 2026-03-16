using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class DevilAbilities : NetworkBehaviour
{
    [SerializeField] private DevilDataSO data;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private DevilController devilController;
    
    // В будущем сюда перетащите префаб ловушки Скримера
    // [SerializeField] private GameObject screamerTrapPrefab;

    private float nextBasicAttackTime = 0f;
    private float nextHeavyAttackTime = 0f;
    private float nextAggressionTime = 0f;

    private int heavyAttacksRemaining;
    private int trapsRemaining;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            heavyAttacksRemaining = data.heavyAttackMaxUses;
            trapsRemaining = data.screamerTrapCount;
        }

        if (!IsOwner) return;

        inputReader.BasicAttackEvent += TryBasicAttack;
        inputReader.HeavyAttackEvent += TryHeavyAttack;
        inputReader.Ability1Event += TryPlaceTrap;       // Q
        inputReader.Ability2Event += TryActivateAggression; // E
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.BasicAttackEvent -= TryBasicAttack;
        inputReader.HeavyAttackEvent -= TryHeavyAttack;
        inputReader.Ability1Event -= TryPlaceTrap;
        inputReader.Ability2Event -= TryActivateAggression;
    }

    // --- BASIC ATTACK ---
    private void TryBasicAttack()
    {
        if (Time.time < nextBasicAttackTime) return;
        nextBasicAttackTime = Time.time + data.basicAttackCooldown;
        BasicAttackServerRpc();
    }

    [ServerRpc]
    private void BasicAttackServerRpc()
    {
        PerformAttack(data.basicAttackDamage, data.attackRadius);
    }

    // --- HEAVY ATTACK ---
    private void TryHeavyAttack()
    {
        if (Time.time < nextHeavyAttackTime) return;
        HeavyAttackServerRpc();
    }

    [ServerRpc]
    private void HeavyAttackServerRpc()
    {
        if (heavyAttacksRemaining <= 0) return;

        heavyAttacksRemaining--;
        PerformAttack(data.heavyAttackDamage, data.attackRadius * 1.3f);
        
        HeavyAttackCooldownClientRpc(); // Говорим клиенту запустить КД визуально
    }

    [ClientRpc]
    private void HeavyAttackCooldownClientRpc()
    {
        if (IsOwner) nextHeavyAttackTime = Time.time + data.heavyAttackCooldown;
    }

    // Общая логика нанесения урона по сфере
    private void PerformAttack(int damage, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, radius, data.playerLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out PlayerHealth targetHealth))
            {
                targetHealth.TakeDamageServerRpc(damage);
                // Можно добавить эффект крови/звук удара
            }
        }
    }

    // --- TRAPS ---
    private void TryPlaceTrap()
    {
        PlaceTrapServerRpc(transform.position + transform.forward * 1.5f);
    }

    [ServerRpc]
    private void PlaceTrapServerRpc(Vector3 position)
    {
        if (trapsRemaining <= 0) return;
        trapsRemaining--;
        
        // Когда создадите скример:
        // GameObject trap = Instantiate(screamerTrapPrefab, position, Quaternion.identity);
        // trap.GetComponent<NetworkObject>().Spawn();
        
        Debug.Log($"[Server] Devil placed a trap! Remaining: {trapsRemaining}");
    }

    // --- AGGRESSION ---
    private void TryActivateAggression()
    {
        if (Time.time < nextAggressionTime) return;
        ActivateAggressionServerRpc();
    }

    [ServerRpc]
    private void ActivateAggressionServerRpc()
    {
        if (devilController.isAggressionActive.Value) return;
        
        StartCoroutine(AggressionRoutine());
        AggressionCooldownClientRpc();
    }

    private IEnumerator AggressionRoutine()
    {
        devilController.isAggressionActive.Value = true;
        yield return new WaitForSeconds(data.aggressionDuration);
        devilController.isAggressionActive.Value = false;
    }

    [ClientRpc]
    private void AggressionCooldownClientRpc()
    {
        if (IsOwner) nextAggressionTime = Time.time + data.aggressionCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null && data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, data.attackRadius);
        }
    }
}