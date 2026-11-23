using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewbieWitch : Enemy
{
    [System.Serializable]
    public class BossMove
    {
        public string moveName;
        public string animTriggerOrState;
        public float cooldown = 5f;
        [HideInInspector] public float currentCooldownTimer = 0f;

        [Header("Ideal Range")]
        public float minDistance = 0f;
        public float maxDistance = 10f;

        // How much this move wants to be used by default when in range
        public float baseScore = 10f;
    }
    [Header("Boss Settings")]
    [SerializeField] private List<BossMove> bossMoves = new List<BossMove>();
    [SerializeField] private float decisionCooldown = 1.5f;
    private float decisionTimer;
    [SerializeField] private float outTheGround = 0f;
    [SerializeField] private float lightningDelay = 0.6f;

    [Header("Close-Range Forcefield (When Not Staggered)")]
    [SerializeField] private GameObject forcefieldPrefab;
    [SerializeField] private float forcefieldTriggerRadius = 3.5f;
    [SerializeField] private float forcefieldCooldown = 4.5f;
    [SerializeField] private bool parentForcefieldToWitch = true;
    private float forcefieldCooldownTimer = 0f;

    [Header("Phase 2 (<= 50% HP) Lightning Trail")]
    [SerializeField] private float halfHealthThreshold = 0.5f;
    [SerializeField] private float halfHealthCountdownStart = 3.0f; // Timer starts counting down at <= 50% HP
    [SerializeField] private float halfHealthRepeatInterval = 6.0f; // Repeats every X seconds while <= 50% HP
    [SerializeField] private int trailStrikeCount = 5;
    [SerializeField] private float trailSpacing = 2.2f;
    [SerializeField] private float trailStepDelay = 0.14f;
    [SerializeField] private float trailTelegraphDuration = 0.55f;
    private float halfHealthTimer = -1f;
    private bool halfHealthTimerStarted = false;

    [Header("Sky Fireballs (Arc & Rain Down)")]
    [SerializeField] private int skyFireballCount = 3;
    [SerializeField] private float skySpawnSpread = 2.5f;
    [SerializeField] private float skyLaunchHeight = 9.0f;
    [SerializeField] private float skyHangTime = 0.45f;
    [SerializeField] private float skyDropSpeed = 24f;
    [SerializeField] private float skyFireballSpacing = 0.25f;

    [Header("Attack Prefabs")]
    [SerializeField] private GameObject energyWave;
    [SerializeField] private GameObject skyFireballPrefab; // Falls back to energyWave if left unassigned
    [SerializeField] private GameObject fireStormPrefab;
    [SerializeField] private GameObject lightningTelegraphPrefab;
    [SerializeField] private GameObject lightningBoltPrefab;

    [Header("Damage & Graybox Visuals")]
    [SerializeField] private int fireballDamage = 20;
    public int FireballDamage => fireballDamage;
    [SerializeField] private Transform castOrigin; // Optional child transform on capsule/rig; falls back to transform
    [SerializeField] private Renderer grayboxRenderer; // Optional capsule/mesh for telegraph flash
    [SerializeField] private Color telegraphFlashColor = new Color(1f, 0.35f, 0.85f, 1f);

    private bool isCastingSpecial;
    private bool healActive;
    private Color originalRendererColor;
    private bool hasCachedColor;

    public void Start() {

        ShieldUp();
        decisionTimer = decisionCooldown;

        if (grayboxRenderer != null && grayboxRenderer.material != null && grayboxRenderer.material.HasProperty("_Color")) {
            originalRendererColor = grayboxRenderer.material.color;
            hasCachedColor = true;
        }

        InitializeMoveset();
    }

    private void InitializeMoveset() {
        if (bossMoves.Count == 0) {
            bossMoves.Add(new BossMove { moveName = "Fireball", animTriggerOrState = "Ult", minDistance = 4f, maxDistance = 25f, baseScore = 15f, cooldown = 2f });
            bossMoves.Add(new BossMove { moveName = "SkyFireballs", animTriggerOrState = "Ult", minDistance = 3f, maxDistance = 30f, baseScore = 22f, cooldown = 7f });
            bossMoves.Add(new BossMove { moveName = "FireStorm", animTriggerOrState = "FireStormCast", minDistance = 0f, maxDistance = 4.5f, baseScore = 40f, cooldown = 6f });
            bossMoves.Add(new BossMove { moveName = "Lightning", animTriggerOrState = "LightningCastStart", minDistance = 6f, maxDistance = 30f, baseScore = 25f, cooldown = 8f });
            bossMoves.Add(new BossMove { moveName = "LightningTrail", animTriggerOrState = "LightningCastStart", minDistance = 2f, maxDistance = 30f, baseScore = 35f, cooldown = 8f });
        }
    }

    public override void States() {
        if (Anim != null) {
            Anim.SetFloat("Distance", Distance);
        }

        // Tick down all move cooldowns
        foreach (var move in bossMoves) {
            if (move.currentCooldownTimer > 0) {
                move.currentCooldownTimer -= Time.deltaTime;
            }
        }

        if (forcefieldCooldownTimer > 0f) {
            forcefieldCooldownTimer -= Time.deltaTime;
        }

        // Close-range forcefield check: only triggers when player is close and Witch is NOT staggered
        if (!Timelining && !isCastingSpecial && !healActive && !IsWitchStaggered()) {
            if (forcefieldCooldownTimer <= 0f && Distance <= forcefieldTriggerRadius) {
                StartCoroutine(CastForcefield());
                return;
            }
        }

        // Once at or below 50% HP, start and tick down the Lightning Trail timer
        HandleHalfHealthLightningTimer();

        // Emergency heal state overrides standard tactical decisions
        if (stats.Health < stats.HealthMax* 0.45f && !healActive) {
            StartCoroutine(waitToHeal());
            healActive = true;
        }

        HandleTacticalAI();
    }

    // Hook this to whatever stagger boolean/state your Enemy base class uses
    private bool IsWitchStaggered() {
        // If your Enemy base class exposes a staggered bool/property (e.g., IsStaggered or Staggered),
        // swap this check to match your Enemy script's stagger state.
        if (Anim != null && HasAnimatorParameter(Anim, "Staggered", AnimatorControllerParameterType.Bool)) {
            if (Anim.GetBool("Staggered")) return true;
        }
        return false;
    }

    private bool HasAnimatorParameter(Animator animator, string paramName, AnimatorControllerParameterType type) {
        foreach (var param in animator.parameters) {
            if (param.name == paramName && param.type == type) return true;
        }
        return false;
    }

    private void HandleHalfHealthLightningTimer() {
        if (stats.Health < stats.HealthMax * halfHealthThreshold) return;

        if (!halfHealthTimerStarted) {
            halfHealthTimerStarted = true;
            halfHealthTimer = halfHealthCountdownStart;
            Debug.Log($"[Mage] <= {halfHealthThreshold * 100f}% HP! Lightning Trail timer started: {halfHealthTimer:F1}s");
        }

        if (halfHealthTimer > 0f) {
            halfHealthTimer -= Time.deltaTime;
        }

        // When timer hits 0 and she isn't mid-special/heal/staggered, fire the Lightning Trail and reset timer
        if (halfHealthTimer <= 0f && !Timelining && !isCastingSpecial && !healActive && !IsWitchStaggered()) {
            halfHealthTimer = halfHealthRepeatInterval;
            StartCoroutine(CastLightningTrail());
        }
    }

    private void HandleTacticalAI() {
        if (Timelining || isCastingSpecial || healActive || IsWitchStaggered()) return;

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0) {
            decisionTimer = decisionCooldown;
            ExecuteBestMove();
        }
    }

    private void ExecuteBestMove() {
        BossMove bestMove = null;
        float highestScore = -1f;

        Player zend = FindFirstObjectByType<Player>();
        bool isPlayerAttacking = zend != null && zend.IsPlayingSkill;

        foreach (var move in bossMoves) {
            if (move.currentCooldownTimer > 0) continue;
            if (Distance < move.minDistance || Distance > move.maxDistance) continue;

            // LightningTrail is driven by the <= 50% HP countdown timer
            if (move.moveName == "LightningTrail") continue;

            float score = move.baseScore;

            if (move.moveName == "FireStorm" && isPlayerAttacking) {
                score += 50f;
            }

            if (move.moveName == "Lightning" && Distance > 12f) {
                score += 15f;
            }

            if (score > highestScore) {
                highestScore = score;
                bestMove = move;
            }
        }

        if (bestMove != null) {
            Debug.Log($"[Tactical AI] Decided to use: {bestMove.moveName} (Score: {highestScore})");
            TriggerBossMove(bestMove);
        }
    }

    private void TriggerBossMove(BossMove move) {
        move.currentCooldownTimer = move.cooldown;

        switch (move.moveName) {
            case "FireStorm":
                StartCoroutine(CastFireStorm());
                break;
            case "Lightning":
                StartCoroutine(CastLightningStrike());
                break;
            case "LightningTrail":
                StartCoroutine(CastLightningTrail());
                break;
            case "SkyFireballs":
                StartCoroutine(CastSkyFireballs());
                break;
            default:
                if (Anim != null && !string.IsNullOrEmpty(move.animTriggerOrState)) {
                    Anim.SetTrigger(move.animTriggerOrState);
                }
                else {
                    CastFireball();
                }
                break;
        }
    }

    #region --- CLOSE-RANGE FORCEFIELD ---

    public IEnumerator CastForcefield() {
        isCastingSpecial = true;
        forcefieldCooldownTimer = forcefieldCooldown;

        PlayAnimSafe("ForcefieldCast", false);
        StartCoroutine(FlashTelegraphColor(0.25f));

        if (forcefieldPrefab != null) {
            Vector3 spawnPos = GetCastOriginPosition();
            Transform parent = parentForcefieldToWitch ? (castOrigin != null ? castOrigin : transform) : null;
            GameObject field = Instantiate(forcefieldPrefab, spawnPos, transform.rotation, parent);

            // Forcefield handles its own collider/hitbox/pushback logic; link Enemy ref if present
            if (field.TryGetComponent<EnemyHitBox>(out var hitBox)) {
                hitBox.Enemy = this;
            }
        }

        yield return new WaitForSeconds(0.4f);
        isCastingSpecial = false;
    }

    #endregion

    #region --- FIRE SKILLS ---

    // Called by Animator event or directly in graybox testing
    public void CastFireball() {
        if (energyWave == null) return;

        Vector3 origin = GetCastOriginPosition();
        Vector3 spawnPos = origin + (transform.forward * 2f + new Vector3(0f, outTheGround, 0f));
        Quaternion spawnRot = transform.rotation;

        GameObject blast = Instantiate(energyWave, spawnPos, spawnRot);
        ConfigureFireball(blast, transform.forward);
    }

    // Launches fireballs high into the sky, then drops them toward the player so they can be dodged or deflected
    public IEnumerator CastSkyFireballs() {
        GameObject prefabToUse = skyFireballPrefab != null ? skyFireballPrefab : energyWave;
        if (prefabToUse == null) yield break;

        isCastingSpecial = true;
        PlayAnimSafe("Ult", true);
        StartCoroutine(FlashTelegraphColor(0.35f));

        Vector3 origin = GetCastOriginPosition();
        Transform player = GetPlayerTransform();

        for (int i = 0; i < skyFireballCount; i++) {
            Vector3 spreadOffset = new Vector3(
                Random.Range(-skySpawnSpread, skySpawnSpread),
                0f,
                Random.Range(-skySpawnSpread, skySpawnSpread)
            );

            Vector3 skyPos = origin + Vector3.up * skyLaunchHeight + spreadOffset;
            GameObject ballObj = Instantiate(prefabToUse, skyPos, Quaternion.identity);

            Vector3 targetPoint = player != null
                ? player.position + spreadOffset * 0.5f
                : origin + transform.forward * (6f + i * 2.5f);

            ConfigureFireball(ballObj, Vector3.up);
            StartCoroutine(ArcFireballDownward(ballObj, targetPoint));

            if (skyFireballSpacing > 0f) {
                yield return new WaitForSeconds(skyFireballSpacing);
            }
        }

        yield return new WaitForSeconds(0.6f);
        isCastingSpecial = false;
    }

    private IEnumerator ArcFireballDownward(GameObject ballObj, Vector3 targetPoint) {
        if (ballObj == null) yield break;

        float hangTimer = 0f;
        while (ballObj != null && hangTimer < skyHangTime) {
            hangTimer += Time.deltaTime;
            ballObj.transform.position += Vector3.up * (1.5f * Time.deltaTime);
            yield return null;
        }

        while (ballObj != null) {
            Vector3 toTarget = targetPoint - ballObj.transform.position;
            if (toTarget.sqrMagnitude <= 0.25f) {
                break;
            }

            Vector3 dir = toTarget.normalized;
            ballObj.transform.rotation = Quaternion.LookRotation(dir);
            ballObj.transform.position += dir * (skyDropSpeed * Time.deltaTime);

            EnemyFireball proj = ballObj.GetComponent<EnemyFireball>();
            if (proj != null) {
                proj.Direction = dir;
            }

            yield return null;
        }
    }

    private void ConfigureFireball(GameObject blast, Vector3 direction) {
        if (blast == null) return;

        if (blast.TryGetComponent<EnemyHitBox>(out var hitBox)) {
            hitBox.Enemy = this;
        }

        if (blast.TryGetComponent<EnemyFireball>(out var projectile)) {
            projectile.Direction = direction;
        }
    }

    private IEnumerator CastFireStorm() {
        isCastingSpecial = true;
        PlayAnimSafe("FireStormCast", false);
        StartCoroutine(FlashTelegraphColor(0.4f));

        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPos = transform.position;
        if (fireStormPrefab != null) {
            GameObject storm = Instantiate(fireStormPrefab, spawnPos, Quaternion.identity);
            if (storm.TryGetComponent<EnemyHitBox>(out var hitBox)) {
                hitBox.Enemy = this;
            }
        }

        yield return new WaitForSeconds(1.5f);
        isCastingSpecial = false;
    }

    #endregion

    #region --- LIGHTNING SKILLS ---

    private IEnumerator CastLightningStrike() {
        isCastingSpecial = true;
        PlayAnimSafe("LightningCastStart", false);
        StartCoroutine(FlashTelegraphColor(0.35f));

        Transform playerTransform = GetPlayerTransform();
        if (playerTransform != null && lightningTelegraphPrefab != null) {
            GameObject lightningObj = Instantiate(lightningTelegraphPrefab, playerTransform.position, Quaternion.identity);
            if (lightningObj.TryGetComponent<EnemyLightning>(out var lightning)) {
                lightning.TriggerLightningStrike(playerTransform, this, 25, 30);
            }
        }

        yield return new WaitForSeconds(1.0f);
        isCastingSpecial = false;
    }

    // Casts a forward line/trail of lightning toward the player once <= 50% HP timer triggers
    public IEnumerator CastLightningTrail() {
        isCastingSpecial = true;
        PlayAnimSafe("LightningCastStart", false);
        StartCoroutine(FlashTelegraphColor(0.45f));

        Transform playerTransform = GetPlayerTransform();
        Vector3 startPos = new Vector3(transform.position.x, transform.position.y + outTheGround, transform.position.z);

        Vector3 forwardDir = transform.forward;
        if (playerTransform != null) {
            Vector3 flatToPlayer = playerTransform.position - startPos;
            flatToPlayer.y = 0f;
            if (flatToPlayer.sqrMagnitude > 0.01f) {
                forwardDir = flatToPlayer.normalized;
            }
        }

        List<Vector3> trailPoints = new List<Vector3>();
        List<GameObject> telegraphs = new List<GameObject>();

        // 1. Lay down the forward trail of telegraphs
        for (int i = 1; i <= trailStrikeCount; i++) {
            Vector3 point = startPos + forwardDir * (i * trailSpacing);
            trailPoints.Add(point);

            if (lightningTelegraphPrefab != null) {
                GameObject tele = Instantiate(lightningTelegraphPrefab, point, Quaternion.LookRotation(forwardDir));
                telegraphs.Add(tele);
            }

            if (trailStepDelay > 0f) {
                yield return new WaitForSeconds(trailStepDelay);
            }
        }

        // Wait for telegraph reaction window
        float remainingWait = Mathf.Max(0.05f, trailTelegraphDuration - (trailStrikeCount * trailStepDelay));
        yield return new WaitForSeconds(remainingWait);

        // Clean up telegraph markers
        foreach (var tele in telegraphs) {
            if (tele != null) Destroy(tele);
        }

        // 2. Strike sequentially down the trail line
        GameObject strikePrefab = lightningBoltPrefab != null ? lightningBoltPrefab : lightningTelegraphPrefab;
        for (int i = 0; i < trailPoints.Count; i++) {
            if (strikePrefab != null) {
                GameObject bolt = Instantiate(strikePrefab, trailPoints[i], Quaternion.LookRotation(forwardDir));

                if (bolt.TryGetComponent<EnemyLightning>(out var lightning)) {
                    lightning.TriggerLightningStrike(playerTransform, this, 25, 30);
                }

                if (bolt.TryGetComponent<EnemyHitBox>(out var hitBox)) {
                    hitBox.Enemy = this;
                }
            }

            if (trailStepDelay > 0f) {
                yield return new WaitForSeconds(trailStepDelay * 0.65f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isCastingSpecial = false;
    }

    #endregion

    #region --- HEAL STATE ---

    IEnumerator waitToHeal() {
        YieldInstruction wait = new WaitForSeconds(5);
        while (isActiveAndEnabled && stats.Health < stats.HealthMax * 0.45f) {
            yield return wait;
            PlayAnimSafe("HealCast", false);
        }
        healActive = false;
    }

    #endregion

    public void TriggerFireStormVFX() {
        if (fireStormPrefab == null) return;

        Vector3 spawnPos = transform.position;
        GameObject storm = Instantiate(fireStormPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
        storm.transform.parent = null;

        if (storm.TryGetComponent<EnemyHitBox>(out var hitBox)) {
            hitBox.Enemy = this;
        }
    }

    private Vector3 GetCastOriginPosition() {
        return castOrigin != null ? castOrigin.position : transform.position;
    }

    private void PlayAnimSafe(string stateOrTrigger, bool isTrigger) {
        if (Anim == null || string.IsNullOrEmpty(stateOrTrigger)) return;

        if (isTrigger) {
            Anim.SetTrigger(stateOrTrigger);
        }
        else {
            Anim.Play(stateOrTrigger);
        }
    }

    private IEnumerator FlashTelegraphColor(float duration) {
        if (!hasCachedColor || grayboxRenderer == null || grayboxRenderer.material == null) yield break;

        grayboxRenderer.material.color = telegraphFlashColor;
        yield return new WaitForSeconds(duration);
        if (grayboxRenderer != null && grayboxRenderer.material != null) {
            grayboxRenderer.material.color = originalRendererColor;
        }
    }

    private Transform GetPlayerTransform() {
        Player player = FindFirstObjectByType<Player>();
        return player != null ? player.transform : null;
    }
}
