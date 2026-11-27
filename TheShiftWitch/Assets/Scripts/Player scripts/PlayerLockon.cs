using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerLockOn : MonoBehaviour
{
    [SerializeField] private static List<Enemy> enemies = new List<Enemy>(16);

    private Camera cam;
    private Player player;
    private Transform cachedTransform;
    private Enemy enemyTarget;
    private Enemy softTarget;
    private Enemy closestEnemy;


    // Events
    public static event UnityAction<bool> onLockOn;
    public static event UnityAction<Enemy> onTargetFound;
    public static event UnityAction<int> playBattleTheme;
    public static event UnityAction<GameObject> switchTarget;

    // Variables
    private bool locked;
    private float rotateSpeed;
    [SerializeField] private GameObject aimPoint;
    private bool rotLock;
    private float rotationSpeed;
    private bool takedown;
    private int t;
    [Header("UI Reticle Visuals")]
    [SerializeField] private Image reticleImage; // Drag your UI Image component here
    [SerializeField] private Color softTargetColor = new Color(1f, 1f, 1f, 0.35f);  // Semi-transparent white
    [SerializeField] private Color hardLockColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);    // Sharp Red
    [SerializeField] private Color offScreenColor = new Color(1f, 0.6f, 0f, 0.9f);     // Warning Orange

    // Properties
    public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = Mathf.Clamp(value, 5f, 8f); }
    public float RotateSpeed { get => rotateSpeed; set => rotateSpeed = Mathf.Clamp(value, 5f, 8f); }
    public static List<Enemy> Enemies { get => enemies; set => enemies = value; }
    public int T { get => t; set => t = Mathf.Clamp(value, 0, Enemies.Count); }

    public Enemy ClosestEnemy { get => closestEnemy; set => closestEnemy = value; }
    public bool Takedown { get => takedown; set => takedown = value; }
    public bool RotLock { get => rotLock; set => rotLock = value; }

    // Hard Lock-On Target Definition
    public Enemy EnemyTarget {
        get => enemyTarget;
        set {
            if (enemyTarget != null) enemyTarget.LockedOn = false;
            enemyTarget = value;
            if (value != null) {
                onTargetFound?.Invoke(value);
                value.LockedOn = true;
            }
        }
    }

    public Enemy SoftTarget {
        get => softTarget;
        set => softTarget = value;
    }

    public void SetSoftTargetFromTransform(Transform targetTransform) {
        if (targetTransform == null) {
            softTarget = null;
            return;
        }

        if (targetTransform.TryGetComponent<Enemy>(out Enemy enemy)) {
            softTarget = enemy;
        }
        else {
            softTarget = targetTransform.GetComponentInParent<Enemy>();
        }
    }

    // Dynamic target resolution based on current combat state
    public Enemy CurrentCombatTarget => EnemyTarget != null ? EnemyTarget : SoftTarget;
    public bool HasAnyTarget => CurrentCombatTarget != null;

    private void OnEnable() {
        //Enemy.onAnyDefeated += RemoveTheDead;
        //NewZend.findEnemy += FirstLocked;
        //AttackStates.locked += LockToTarget;
        //LockedOnState.autoLocked += LockToTarget;
        //NewZend.onPlayerDeath += RemoveAllEnemies;
        //ThirdPersonCameraWithLockOn.ThirdPersonCamera.sendTarget += EnemyLockedTo;
        //AttackStates.autoLocked += ToggleHardLockOn;
    }

    private void OnDisable() {
        //Enemy.onAnyDefeated -= RemoveTheDead;
        //NewZend.findEnemy -= FirstLocked;
        //AttackStates.locked -= LockToTarget;
        //LockedOnState.autoLocked -= LockToTarget;
        //NewZend.onPlayerDeath -= RemoveAllEnemies;
        //ThirdPersonCameraWithLockOn.ThirdPersonCamera.sendTarget -= EnemyLockedTo;
        //AttackStates.autoLocked -= ToggleHardLockOn;
        RemoveAllEnemies();
    }

    void Start() {
        cam = Camera.main;
        player = Player.GetPlayer();
        cachedTransform = transform;
        //reticleImage = FindFirstObjectByType<LockOnReticleUI>().ReticleImage;

    }

    void Update() {
        enemies.RemoveAll(e => e == null || e.Dead);
        int enemyCount = enemies.Count;
        player.HasTarget = enemyCount > 0;

        if (enemyCount == 0) {
            SoftTarget = null;
            EnemyTarget = null;
            if (aimPoint != null) aimPoint.SetActive(false);
            return;
        }

        // --- RETICLE VISUAL STATE UPDATES ---
        Enemy activeTarget = CurrentCombatTarget;

        if (activeTarget != null && aimPoint != null) {
            aimPoint.SetActive(true);

            Vector3 targetPos = activeTarget.CenterPoint != null ?
                activeTarget.CenterPoint.transform.position :
                activeTarget.transform.position + new Vector3(0, 1.5f, 0);

            aimPoint.transform.position = targetPos;

            // Determine visual style based on Lock Type and Camera Viewport
            if (reticleImage != null) {
                if (EnemyTarget != null) {
                    // Check if Hard Target is off-screen
                    Vector3 vpPos = cam.WorldToViewportPoint(targetPos);
                    bool isOffScreen = vpPos.z < 0 || vpPos.x < 0.05f || vpPos.x > 0.95f || vpPos.y < 0.05f || vpPos.y > 0.95f;

                    reticleImage.color = isOffScreen ? offScreenColor : hardLockColor;

                    // Scale pulse for hard lock feedback
                    float pulse = 1.0f + Mathf.Sin(Time.unscaledTime * 12f) * 0.1f;
                    aimPoint.transform.localScale = Vector3.one * pulse;
                }
                else {
                    // Dimmer reticle for passive Soft Target tracking
                    reticleImage.color = softTargetColor;
                    aimPoint.transform.localScale = Vector3.one * 0.85f;
                }
            }
        }
        else if (aimPoint != null) {
            aimPoint.SetActive(false);
        }

        EvaluateDeadTargetSwitch();
    }

    // --- CONTINUOUS SOFT TARGET EVALUATION ---
    public void EvaluateSoftTarget(Vector2 moveInput) {
        if (enemies.Count == 0) {
            SoftTarget = null;
            return;
        }

        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 searchDir = cachedTransform.forward;

        if (input != Vector3.zero && cam != null) {
            Vector3 camFwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            searchDir = (camRight * input.x + camFwd * input.z).normalized;
        }

        Enemy bestCandidate = null;
        float highestScore = -100f;
        Vector3 playerPos = cachedTransform.position;

        foreach (Enemy en in enemies) {
            if (en == null || en.Dead) continue;

            Vector3 toEnemy = en.transform.position - playerPos;
            toEnemy.y = 0;
            float dist = toEnemy.magnitude;
            if (dist < 0.001f) continue;

            Vector3 dirToEnemy = toEnemy / dist;
            float dot = Vector3.Dot(searchDir, dirToEnemy);

            float distScore = 1.0f - Mathf.Clamp01(dist / 15.0f);
            float totalScore = (dot * 0.7f) + (distScore * 0.3f);

            if (totalScore > highestScore) {
                highestScore = totalScore;
                bestCandidate = en;
            }
        }

        SoftTarget = bestCandidate;
        if (SoftTarget != null) {
            onTargetFound?.Invoke(SoftTarget);
        }
    }

    // --- GET TARGET FOR ATTACK INITIATION (STICK OVERRIDE SUPPORT) ---
    public Transform GetActiveAttackTarget(Vector2 moveInput) {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

        // Neutral stick: Prefer Hard Target, fall back to Soft Target
        if (inputDir.sqrMagnitude < 0.1f) {
            if (EnemyTarget != null) return EnemyTarget.transform;
            return SoftTarget != null ? SoftTarget.transform : null;
        }

        // Stick pushed: Find enemy aligned with stick input vector
        if (cam != null) {
            Vector3 camFwd = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            Vector3 desiredDir = (camRight * inputDir.x + camFwd * inputDir.z).normalized;

            Enemy manualTarget = null;
            float bestDot = 0.5f; // Requires pointing within ~60 deg cone toward target

            foreach (Enemy en in enemies) {
                if (en == null || en.Dead) continue;
                Vector3 toEnemy = en.transform.position - cachedTransform.position;
                toEnemy.y = 0;
                float dot = Vector3.Dot(desiredDir, toEnemy.normalized);

                if (dot > bestDot) {
                    bestDot = dot;
                    manualTarget = en;
                }
            }

            if (manualTarget != null) return manualTarget.transform;
        }

        return EnemyTarget != null ? EnemyTarget.transform : (SoftTarget != null ? SoftTarget.transform : null);
    }

    // --- ONE-TIME ORIENTATION SNAP AT SWING START ---
    public void OrientPlayerToSoftTarget(Vector2 moveInput) {
        if (rotLock || player.IsAttacking || player.IsPlayingSkill) return;

        Transform targetTransform = GetActiveAttackTarget(moveInput);
        if (targetTransform == null) return;

        Vector3 dir = targetTransform.position - cachedTransform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f) {
            cachedTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    private void EvaluateDeadTargetSwitch() {
        if (Enemies.Count > 0 && EnemyTarget && EnemyTarget.Dead) {
            SwitchTarget(-1);
        }
    }
    // --- FLICK SWITCHING LOGIC ---
    // Screen-Space Directional Switch triggered by OnRightStick callback
    public void SwitchTargetDirectional(Vector2 stickDirection) {
        if (EnemyTarget == null || Enemies.Count <= 1 || cam == null) return;

        Vector3 currentTargetScreenPos = cam.WorldToScreenPoint(
            EnemyTarget.CenterPoint != null ? EnemyTarget.CenterPoint.transform.position : EnemyTarget.transform.position
        );

        Vector2 inputDir = stickDirection.normalized;
        Enemy bestCandidate = null;
        float highestScore = -1000f;

        foreach (Enemy en in Enemies) {
            if (en == null || en == EnemyTarget || en.Dead) continue;

            Vector3 candidateScreenPos = cam.WorldToScreenPoint(
                en.CenterPoint != null ? en.CenterPoint.transform.position : en.transform.position
            );

            // Skip enemies behind the camera lens
            if (candidateScreenPos.z < 0) continue;

            Vector2 screenDelta = (Vector2)candidateScreenPos - (Vector2)currentTargetScreenPos;
            Vector2 candidateDir = screenDelta.normalized;

            // Measure alignment between flick direction and relative enemy position on screen
            float dot = Vector2.Dot(inputDir, candidateDir);

            // Only consider enemies roughly in the direction of the flick (>45 deg cone)
            if (dot > 0.4f) {
                float dist = screenDelta.magnitude;

                // Prioritize directional alignment while preferring closer screen targets
                float score = (dot * 800f) - dist;

                if (score > highestScore) {
                    highestScore = score;
                    bestCandidate = en;
                }
            }
        }

        if (bestCandidate != null) {
            EnemyTarget = bestCandidate;
            switchTarget?.Invoke(EnemyTarget.gameObject);
        }
    }
    // Inside PlayerLockOn.cs

    public Transform AcquireSpellTarget(Vector2 moveInput) {
        // 1. Check for stick-overridden target first
        Transform attackTarget = GetActiveAttackTarget(moveInput);
        if (attackTarget != null) return attackTarget;

        // 2. Fall back to current combat target (Hard -> Soft)
        if (CurrentCombatTarget != null) return CurrentCombatTarget.transform;

        // 3. Last fallback: Closest enemy in range
        if (ClosestEnemy != null) return ClosestEnemy.transform;

        return null;
    }

    // Executes a dedicated spell lock snap
    public Enemy BeginSpellLock(Vector2 moveInput) {
        Transform targetTransform = AcquireSpellTarget(moveInput);

        if (targetTransform != null) {
            // Snap rotation immediately toward spell target
            Vector3 dir = targetTransform.position - cachedTransform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f) {
                cachedTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            // Lock rotation so stick movement won't rotate Zend during the cast
            RotLock = true;

            if (targetTransform.TryGetComponent<Enemy>(out Enemy enemy)) {
                return enemy;
            }
        }

        return null;
    }

    public void EndSpellLock() {
        RotLock = false;
    }
    public void SwitchTarget(int direction) {
        if (Enemies.Count == 0) return;

        if (EnemyTarget != null) enemyTarget.LockedOn = false;

        int currentIndex = (EnemyTarget != null && Enemies.Contains(EnemyTarget)) ? Enemies.IndexOf(EnemyTarget) : 0;
        int newIndex = currentIndex + direction;

        if (newIndex >= Enemies.Count) {
            newIndex = 0;
        }
        else if (newIndex < 0) {
            newIndex = Enemies.Count - 1;
        }

        EnemyTarget = Enemies[newIndex];
        switchTarget?.Invoke(EnemyTarget.gameObject);
    }
    public void ToggleHardLockOn() {
        if (EnemyTarget != null) {
            // Unlock if already locked on
            EnemyTarget = null;
            onLockOn?.Invoke(false);
            print("Lock Off");
        }
        else {
            // Find best target: SoftTarget first, then ClosestEnemy
            Enemy targetToLock = SoftTarget != null ? SoftTarget : ClosestEnemy;
            if (targetToLock != null) {
                EnemyTarget = targetToLock;
                onLockOn?.Invoke(true);
            }
            print("Lock On");
        }
    }
    private void FirstLocked() {
        if (enemies.Count > 0) EnemyTarget = enemies[0];
    }

    private void EnemyLockedTo(GameObject target) {
        if (target != null && target.TryGetComponent<Enemy>(out Enemy enemy)) {
            EnemyTarget = enemy;
        }
    }

    public void Findtarget() {
        EnemyTarget = ClosestEnemy;
    }

    private void getClosestEnemy(bool forceTargetUpdate = false) {
        if (Enemies.Count == 0) {
            ClosestEnemy = null;
            return;
        }

        if (ClosestEnemy != null && !forceTargetUpdate && Enemies.Contains(ClosestEnemy)) {
            return;
        }

        float minDistance = float.MaxValue;
        Enemy bestTarget = null;
        Vector3 playerPos = player.transform.position;

        foreach (Enemy en in Enemies) {
            if (en == null) continue;
            float distance = Vector3.Distance(en.transform.position, playerPos);

            if (distance < minDistance) {
                minDistance = distance;
                bestTarget = en;
            }
        }

        if (bestTarget != null) {
            ClosestEnemy = bestTarget;
            T = Enemies.IndexOf(bestTarget);
        }
    }

    public float EnDist(GameObject target) => EnemyTarget != null ? Vector3.Distance(target.transform.position, player.transform.position) : 0f;

    private void LockToTarget() {
        if (EnemyTarget == null) return;
        Vector3 delta = EnemyTarget.transform.position - player.transform.position;
        delta.y = 0;
    }

    private void RemoveTheDead(Enemy enemy) {
        bool wasTarget = (EnemyTarget == enemy);
        bool wasSoftTarget = (SoftTarget == enemy);

        Enemies.Remove(enemy);

        if (wasSoftTarget) SoftTarget = null;

        if (wasTarget && Enemies.Count > 0) {
            getClosestEnemy(true);
            SwitchTarget(0);
        }
        else if (Enemies.Count == 0) {
            player.LockedOn = false;
            EnemyTarget = null;
            SoftTarget = null;
        }
    }

    private void RemoveAllEnemies() {
        LockOffOfAllEnemies();
        Enemies.Clear();
        SoftTarget = null;
        EnemyTarget = null;
    }

    public void LockOffOfAllEnemies() {
        foreach (Enemy en in Enemies) {
            if (en != null) en.LockedOn = false;
        }
    }
}