using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewDevilData", menuName = "Game/Devil Data")]
public class DevilDataSO : ScriptableObject
{
    [BoxGroup("Stats")] public int maxHealth = 150;
    
    [BoxGroup("Movement")] public float moveSpeed = 6.0f;
    [BoxGroup("Movement")] public float aggressionSpeed = 9.0f;
    [BoxGroup("Movement")] public float rotationSensitivity = 0.5f;

    [BoxGroup("Combat")] public int basicAttackDamage = 15;
    [BoxGroup("Combat")] public float basicAttackCooldown = 1.25f;
    [BoxGroup("Combat")] public float attackRadius = 2.0f;

    [BoxGroup("Heavy Attack")] public int heavyAttackDamage = 30;
    [BoxGroup("Heavy Attack")] public float heavyAttackCooldown = 37.5f;
    [BoxGroup("Heavy Attack")] public int heavyAttackMaxUses = 3;

    [BoxGroup("Abilities")] public int screamerTrapCount = 4;
    [BoxGroup("Abilities")] public float aggressionDuration = 15.0f;
    [BoxGroup("Abilities")] public float aggressionCooldown = 120.0f;

    [BoxGroup("Physics")] public LayerMask playerLayer;
}