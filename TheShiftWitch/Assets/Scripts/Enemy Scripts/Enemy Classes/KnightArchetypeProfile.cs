using UnityEngine;

public enum KnightType
{
    BaseKnight,
    ShieldKnight,
    MageKnight,
    HeavyGeneral
}

[CreateAssetMenu(fileName = "NewKnightProfile", menuName = "SHIFT/Knight Archetype Profile")]
public class KnightArchetypeProfile : ScriptableObject
{
    [Header("Identity & Capabilities")]
    public KnightType knightType = KnightType.BaseKnight;
    public bool hasShield = false;
    public bool canCastMagic = false;
    public bool hasLightAttackHyperArmor = false;

    [Header("Movement & Spacing")]
    public float moveSpeed = 4.2f;
    public float strafeSpeed = 2.4f;
    public float preferredCombatRange = 2.4f;
    public float castPreferredRange = 9.0f;
    public float rotationSpeed = 10f;

    [Header("Attack & Cooldown Timings")]
    public float decisionInterval = 1.1f;
    public float meleeWindup = 0.45f;
    public float meleeRecovery = 0.65f;
    public float heavyChargeDuration = 1.5f;   // Used by Heavy General AoE / Rush
    public float heavyRecoveryDuration = 1.8f; // Big punish window after General's charged moves
    public float specialCooldown = 5.0f;

    [Header("Damage Values")]
    public int lightComboDamage = 12;
    public int heavyOrBashDamage = 22;
    public int magicOrAoeDamage = 28;
    public float rushSpeed = 13f;
    public float rushDuration = 0.75f;
}