using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class KnightAi : Enemy
{
    public enum AIState
    {
        Chase,
        GuardStrafe,
        Telegraph,
        Attacking,
        Recovery,
        Hitstun,
        StaggerBroken
    }

    [Header("Archetype Config")]
    [SerializeField] private KnightArchetypeProfile profile;
    [SerializeField] private AIState currentState = AIState.Chase;

    [Header("Hitboxes & Spawn Points")]
    [SerializeField] private GameObject meleeHitboxObj;      // Weapon hitbox toggled during swings
    [SerializeField] private GameObject shieldBashHitboxObj; // Optional shield hitbox
    [SerializeField] private GameObject aoeSlamPrefab;       // Heavy General shockwave ring
    [SerializeField] private GameObject magicCastPrefab;     // Mage Knight bolt or ground rune
    [SerializeField] private Transform castOrSpawnPoint;

    [Header("Graybox / Telegraph Visuals")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Color parryTelegraphColor = new Color(1f, 0.85f, 0.2f, 1f); // Gold = Parryable
    [SerializeField] private Color unblockableColor = new Color(1f, 0.2f, 0.2f, 1f);     // Red = Dodge/Jump AoE

    private FinisherTarget finisherTarget;
    private Transform playerTransform;
    private float stateTimer;
    private float specialTimer;
    private int strafeDirection = 1;
    private bool isGuarding;
    private Color baseColor;
    private bool hasBaseColor;

    public bool IsGuarding => isGuarding;
    public bool HasHyperArmor => profile != null && profile.hasLightAttackHyperArmor;

    public void Start() {

        finisherTarget = GetComponent<FinisherTarget>();

        Player player = FindFirstObjectByType<Player>();
        if (player != null) playerTransform = player.transform;

        if (meleeHitboxObj != null) meleeHitboxObj.SetActive(false);
        if (shieldBashHitboxObj != null) shieldBashHitboxObj.SetActive(false);

        if (bodyRenderer != null && bodyRenderer.material != null && bodyRenderer.material.HasProperty("_Color")) {
            baseColor = bodyRenderer.material.color;
            hasBaseColor = true;
        }

        if (profile != null && profile.hasShield) {
            ShieldUp();
        }
    }

    public override void States() {
        if (profile == null) return;
        print("State updating");
        // Lock everything while player is executing Devour or cutscene
        if (Timelining) {
            DisableAllHitboxes();
            return;
        }

        if (specialTimer > 0f) specialTimer -= Time.deltaTime;
        if (stateTimer > 0f) stateTimer -= Time.deltaTime;

        if (Anim != null) {
            Anim.SetFloat("Distance", Distance);
            Anim.SetBool("IsGuarding", isGuarding);
        }

        switch (currentState) {
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.GuardStrafe:
                UpdateGuardStrafe();
                break;
            case AIState.Telegraph:
            case AIState.Attacking:
            case AIState.Recovery:
            case AIState.Hitstun:
            case AIState.StaggerBroken:
                // Handled by active coroutines / timers
                break;
        }
    }

    #region --- MOVEMENT & SPACING ---

    private void UpdateChase() {
        isGuarding = false;
        FacePlayer();

        float targetRange = (profile.canCastMagic && specialTimer <= 0f)
            ? profile.castPreferredRange
            : profile.preferredCombatRange;

        if (Distance > targetRange) {
            MoveTowardsPlayer(profile.moveSpeed);
            PlayAnimSafe("Run", false);
        }
        else {
            // Reached range: Shield Knights enter GuardStrafe; others attack immediately
            if (profile.hasShield && Shield > 0) {
                EnterGuardStrafe();
            }
            else {
                ChooseAndExecuteAttack();
            }
        }
    }

    private void EnterGuardStrafe() {
        currentState = AIState.GuardStrafe;
        isGuarding = true;
        stateTimer = profile.decisionInterval;
        strafeDirection = Random.value > 0.5f ? 1 : -1;
        PlayAnimSafe("BlockIdle", false);
    }

    private void UpdateGuardStrafe() {
        FacePlayer();

        if (Distance > profile.preferredCombatRange + 1.5f) {
            currentState = AIState.Chase;
            return;
        }

        // Circle player with shield raised
        Vector3 strafe = transform.right * (strafeDirection * profile.strafeSpeed * Time.deltaTime);
        transform.position += strafe;

        if (stateTimer <= 0f) {
            ChooseAndExecuteAttack();
        }
    }

    private void FacePlayer() {
        if (playerTransform == null) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, profile.rotationSpeed * Time.deltaTime);
        }
    }

    private void MoveTowardsPlayer(float speed) {
        if (playerTransform == null) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f) {
            transform.position += dir.normalized * (speed * Time.deltaTime);
        }
    }

    #endregion

    #region --- ARCHETYPE ATTACK SELECTION ---

    private void ChooseAndExecuteAttack() {
        switch (profile.knightType) {
            case KnightType.BaseKnight:
                if (Distance > profile.preferredCombatRange + 0.8f)
                    StartCoroutine(PerformLungeThrust());
                else
                    StartCoroutine(PerformSlashCombo());
                break;

            case KnightType.ShieldKnight:
                if (Distance <= 2.1f && specialTimer <= 0f)
                    StartCoroutine(PerformShieldBash());
                else
                    StartCoroutine(PerformGuardCounterSlash());
                break;

            case KnightType.MageKnight:
                if (specialTimer <= 0f && Distance >= 4.5f)
                    StartCoroutine(PerformMagicCast());
                else
                    StartCoroutine(PerformEnchantedDashSlash());
                break;

            case KnightType.HeavyGeneral:
                if (specialTimer <= 0f) {
                    if (Distance > 5.5f)
                        StartCoroutine(PerformChargedBullRush());
                    else
                        StartCoroutine(PerformChargedAoeSlam());
                }
                else {
                    StartCoroutine(PerformHeavyCleave());
                }
                break;
        }
    }

    #endregion

    #region --- ATTACK COROUTINES ---

    private IEnumerator PerformSlashCombo() {
        currentState = AIState.Telegraph;
        isGuarding = false;
        FacePlayer();
        FlashColor(parryTelegraphColor, profile.meleeWindup);
        PlayAnimSafe("SlashCombo", true);

        yield return new WaitForSeconds(profile.meleeWindup);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        // Hit 1
        yield return ToggleHitbox(meleeHitboxObj, profile.lightComboDamage, 0.18f);
        yield return new WaitForSeconds(0.18f);
        if (IsInterrupted()) yield break;

        // Hit 2
        FacePlayer();
        yield return ToggleHitbox(meleeHitboxObj, profile.lightComboDamage, 0.2f);

        yield return EnterRecovery(profile.meleeRecovery);
    }

    private IEnumerator PerformLungeThrust() {
        currentState = AIState.Telegraph;
        isGuarding = false;
        FacePlayer();
        FlashColor(parryTelegraphColor, profile.meleeWindup);
        PlayAnimSafe("LungeThrust", true);

        yield return new WaitForSeconds(profile.meleeWindup);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        SetHitboxActive(meleeHitboxObj, true, profile.heavyOrBashDamage);

        float lungeTime = 0.25f;
        float t = 0f;
        while (t < lungeTime && !IsInterrupted()) {
            t += Time.deltaTime;
            transform.position += transform.forward * (10f * Time.deltaTime);
            yield return null;
        }

        SetHitboxActive(meleeHitboxObj, false, 0);
        yield return EnterRecovery(profile.meleeRecovery);
    }

    private IEnumerator PerformShieldBash() {
        currentState = AIState.Telegraph;
        specialTimer = profile.specialCooldown;
        isGuarding = true;
        FacePlayer();
        FlashColor(parryTelegraphColor, 0.35f);
        PlayAnimSafe("ShieldBash", true);

        yield return new WaitForSeconds(0.35f);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        GameObject bashBox = shieldBashHitboxObj != null ? shieldBashHitboxObj : meleeHitboxObj;
        yield return ToggleHitbox(bashBox, profile.heavyOrBashDamage, 0.22f);

        isGuarding = false;
        yield return EnterRecovery(profile.meleeRecovery);
    }

    private IEnumerator PerformGuardCounterSlash() {
        currentState = AIState.Telegraph;
        isGuarding = false;
        FacePlayer();
        FlashColor(parryTelegraphColor, profile.meleeWindup);
        PlayAnimSafe("OverhandSlash", true);

        yield return new WaitForSeconds(profile.meleeWindup);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        yield return ToggleHitbox(meleeHitboxObj, profile.heavyOrBashDamage, 0.22f);

        yield return EnterRecovery(profile.meleeRecovery * 1.15f);
    }

    private IEnumerator PerformMagicCast() {
        currentState = AIState.Telegraph;
        specialTimer = profile.specialCooldown;
        FacePlayer();
        FlashColor(unblockableColor, 0.55f);
        PlayAnimSafe("MagicCast", true);

        yield return new WaitForSeconds(0.55f);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        if (magicCastPrefab != null) {
            Vector3 spawnPos = castOrSpawnPoint != null ? castOrSpawnPoint.position : transform.position + transform.forward * 1.2f;
            
            Vector3 targetPos = playerTransform != null ? new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z) : spawnPos;
            GameObject spell = Instantiate(magicCastPrefab, spawnPos, transform.rotation);

            if (spell.TryGetComponent<EnemyHitBox>(out var hb)) hb.Enemy = this;
            if (spell.TryGetComponent<EnemyFireball>(out var proj)) proj.Direction = transform.forward;
        }

        yield return EnterRecovery(profile.meleeRecovery);
    }

    private IEnumerator PerformEnchantedDashSlash() {
        currentState = AIState.Telegraph;
        FacePlayer();
        FlashColor(parryTelegraphColor, 0.4f);
        PlayAnimSafe("DashSlash", true);

        yield return new WaitForSeconds(0.4f);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        SetHitboxActive(meleeHitboxObj, true, profile.heavyOrBashDamage);

        float dashTime = 0.28f;
        float t = 0f;
        while (t < dashTime && !IsInterrupted()) {
            t += Time.deltaTime;
            transform.position += transform.forward * (12f * Time.deltaTime);
            yield return null;
        }

        SetHitboxActive(meleeHitboxObj, false, 0);
        yield return EnterRecovery(profile.meleeRecovery);
    }

    private IEnumerator PerformHeavyCleave() {
        currentState = AIState.Telegraph;
        FacePlayer();
        FlashColor(parryTelegraphColor, 0.75f);
        PlayAnimSafe("GreatSwordSlash", true);

        yield return new WaitForSeconds(0.75f);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        yield return ToggleHitbox(meleeHitboxObj, profile.heavyOrBashDamage, 0.3f);

        yield return EnterRecovery(profile.meleeRecovery * 1.25f);
    }

    private IEnumerator PerformChargedAoeSlam() {
        currentState = AIState.Telegraph;
        specialTimer = profile.specialCooldown;
        FacePlayer();
        FlashColor(unblockableColor, profile.heavyChargeDuration);
        PlayAnimSafe("ChargedSlam", true);

        
        yield return new WaitForSeconds(profile.heavyChargeDuration);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        if (aoeSlamPrefab != null) {
            Vector3 slamPos = castOrSpawnPoint != null ? castOrSpawnPoint.position : transform.position + transform.forward * 1.5f;
            GameObject shockwave = Instantiate(aoeSlamPrefab, slamPos, Quaternion.identity);
            if (shockwave.TryGetComponent<EnemyHitBox>(out var hb)) hb.Enemy = this;
        }

        
        yield return EnterRecovery(profile.heavyRecoveryDuration);
    }

    private IEnumerator PerformChargedBullRush() {
        currentState = AIState.Telegraph;
        specialTimer = profile.specialCooldown;
        FacePlayer();
        FlashColor(unblockableColor, profile.heavyChargeDuration * 0.8f);
        PlayAnimSafe("BullRush", true);

        yield return new WaitForSeconds(profile.heavyChargeDuration * 0.8f);
        if (IsInterrupted()) yield break;

        currentState = AIState.Attacking;
        SetHitboxActive(meleeHitboxObj, true, profile.magicOrAoeDamage);

        float t = 0f;
        while (t < profile.rushDuration && !IsInterrupted()) {
            t += Time.deltaTime;
            transform.position += transform.forward * (profile.rushSpeed * Time.deltaTime);
            yield return null;
        }

        SetHitboxActive(meleeHitboxObj, false, 0);

        
        yield return EnterRecovery(profile.heavyRecoveryDuration);
    }

    private IEnumerator EnterRecovery(float duration) {
        currentState = AIState.Recovery;
        yield return new WaitForSeconds(duration);
        if (!IsInterrupted()) {
            currentState = AIState.Chase;
        }
    }

    #endregion

    #region --- HITSTUN & STAGGER BREAK HOOKS ---

    
    public void OnLightHitReceived(float stunDuration = 0.35f) {
        if (currentState == AIState.StaggerBroken || Timelining) return;

        // Shield Knights deflect Lights while guarding; Heavy General ignores Light hitstun
        if ((profile.hasShield && isGuarding && Shield > 0) || profile.hasLightAttackHyperArmor) {
            return;
        }

        StopAllCoroutines();
        DisableAllHitboxes();
        StartCoroutine(HitstunRoutine(stunDuration));
    }

    private IEnumerator HitstunRoutine(float duration) {
        currentState = AIState.Hitstun;
        PlayAnimSafe("HitImpact", true);
        yield return new WaitForSeconds(duration);
        if (currentState == AIState.Hitstun) {
            currentState = AIState.Chase;
        }
    }

    // Call this when Stagger/Posture gauge hits 0 -> enables Devour Finisher!
    public void TriggerStaggerBreak(float staggerWindow = 3.5f) {
        StopAllCoroutines();
        DisableAllHitboxes();
        isGuarding = false;
        StartCoroutine(StaggerBrokenRoutine(staggerWindow));
    }

    private IEnumerator StaggerBrokenRoutine(float duration) {
        currentState = AIState.StaggerBroken;
        PlayAnimSafe("Stunned", false);

        if (finisherTarget != null) {
            finisherTarget.SetStaggerBroken(true);
        }

        yield return new WaitForSeconds(duration);

        // If player didn't Devour in time, recover out of stagger
        if (currentState == AIState.StaggerBroken && !Timelining) {
            if (finisherTarget != null) finisherTarget.SetStaggerBroken(false);
            currentState = AIState.Chase;
        }
    }

    #endregion

    #region --- HELPERS ---

    private bool IsInterrupted() {
        return Timelining || currentState == AIState.Hitstun || currentState == AIState.StaggerBroken;
    }

    private IEnumerator ToggleHitbox(GameObject hbObj, int damage, float activeTime) {
        SetHitboxActive(hbObj, true, damage);
        yield return new WaitForSeconds(activeTime);
        SetHitboxActive(hbObj, false, 0);
    }

    private void SetHitboxActive(GameObject hbObj, bool active, int damage) {
        if (hbObj == null) return;
        hbObj.SetActive(active);
        if (active && hbObj.TryGetComponent<EnemyHitBox>(out var hb)) {
            hb.Enemy = this;
        }
    }

    private void DisableAllHitboxes() {
        if (meleeHitboxObj != null) meleeHitboxObj.SetActive(false);
        if (shieldBashHitboxObj != null) shieldBashHitboxObj.SetActive(false);
    }

    private void FlashColor(Color color, float duration) {
        if (hasBaseColor && bodyRenderer != null) {
            StartCoroutine(FlashRoutine(color, duration));
        }
    }

    private IEnumerator FlashRoutine(Color color, float duration) {
        bodyRenderer.material.color = color;
        yield return new WaitForSeconds(duration);
        if (bodyRenderer != null && hasBaseColor) {
            bodyRenderer.material.color = baseColor;
        }
    }

    private void PlayAnimSafe(string stateOrTrigger, bool isTrigger) {
        if (Anim == null || string.IsNullOrEmpty(stateOrTrigger)) return;
        if (isTrigger) Anim.SetTrigger(stateOrTrigger);
        else Anim.Play(stateOrTrigger);
    }
    #endregion
}