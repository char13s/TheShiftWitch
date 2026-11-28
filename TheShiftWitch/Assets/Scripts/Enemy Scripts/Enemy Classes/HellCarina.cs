using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HellCaina : Enemy
{
    public static readonly List<HellCaina> ActiveCainas = new List<HellCaina>();

    [Header("Hell Caina Swarm & Spacing")]
    [SerializeField] private float flankRadius = 4.2f;
    [SerializeField] private float chaseSpeedMultiplier = 1.4f;
    [SerializeField] private float cainaAttackDistance = 2.2f;
    [SerializeField] private int maxSimultaneousAttackers = 2;

    [Header("Attack Pacing")]
    [SerializeField] private float attackCooldown = 2.2f;
    private float _nextAttackTime;
    private bool _isExecutingAttack;

    [Header("Familiar / Finisher Hook")]
    [SerializeField] private int familiarConsumeHealthThreshold = 25;

    private static readonly HashSet<HellCaina> _attackingCainas = new HashSet<HellCaina>();
    private Coroutine _attackRoutine;
    private float _orbitAngle;
    private int _orbitDir = 1;
    private bool _hasAttackToken;

    public bool CanBeConsumedByFamiliar =>
        !Dead && (State == EnemyAiStates.Staggered || Health <= familiarConsumeHealthThreshold);

    private void OnEnable() {
        // Register with PlayerLockOn from base Enemy
        if (!PlayerLockOn.Enemies.Contains(this)) {
            PlayerLockOn.Enemies.Add(this);
        }

        if (!ActiveCainas.Contains(this)) {
            ActiveCainas.Add(this);
        }

        _orbitAngle = Random.Range(0f, 360f);
        _orbitDir = Random.value > 0.5f ? 1 : -1;
    }

    private void OnDisable() {
        ActiveCainas.Remove(this);
        ReleaseAttackToken();
    }

    public override void States() {
        if (Dead || Timelining) return;

        // Drive state transitions using your Enemy StateSwitch flow
        StateSwitch();

        switch (State) {
            case EnemyAiStates.Idle:
                Idle();
                FlankAroundPlayer();
                break;

            case EnemyAiStates.Chasing:
                Chasing();
                break;

            case EnemyAiStates.Attacking:
                Walk = false;
                if (_attackRoutine == null) {
                    _attackRoutine = StartCoroutine(CainaAttackRoutine());
                }
                break;

            case EnemyAiStates.Staggered:
                Walk = false;
                ReleaseAttackToken();
                break;

            default:
                Walk = false;
                break;
        }

        if (Anim != null) {
            Anim.SetFloat("Distance", Distance);
            Anim.SetBool("Walk", Walk);
        }
    }

    public override void StateSwitch() {
        if (Timelining || Dead || Hit || State == EnemyAiStates.Staggered) return;

        // Lock state while actively winding up, swinging, or recovering
        if (_isExecutingAttack) return;

        bool canAttackNow = Time.time >= _nextAttackTime && Distance <= cainaAttackDistance;

        if (canAttackNow && TryAcquireAttackToken()) {
            State = EnemyAiStates.Attacking;
            if (_attackRoutine == null) {
                _attackRoutine = StartCoroutine(CainaAttackRoutine());
            }
        }
        else {
            ReleaseAttackToken();
            State = Distance > flankRadius + 1.5f ? EnemyAiStates.Chasing : EnemyAiStates.Idle;
        }
    }

    private IEnumerator CainaAttackRoutine() {
        _isExecutingAttack = true;
        Walk = false;
        RotateToPlayer();

        int pattern = Random.Range(0, 3);
        string triggerName = pattern switch {
            0 => "Slash",
            1 => "Overhead",
            _ => "Lunge"
        };

        if (Anim != null) {
            Anim.Play(triggerName);
        }

        // Windup + active swing
        yield return new WaitForSeconds(0.65f);

        // Release token right after the strike finishes so another Caina can step in
        ReleaseAttackToken();

        // Recovery + personal cooldown before this Caina can attack again
        yield return new WaitForSeconds(0.75f);
        _nextAttackTime = Time.time + attackCooldown;

        _isExecutingAttack = false;
        _attackRoutine = null;
        State = EnemyAiStates.Idle;
    }

    private Vector3 _desiredHorizontalVelocity;

    private void SmoothFaceDirection(Vector3 flatDir, float turnSpeed = 10f) {
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude < 0.0025f) return;

        Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    public override void Chasing() {
        if (Player == null || CharCon == null || !CharCon.enabled) return;

        Vector3 targetPos = GetCameraAwareMoveTarget(Player.transform.position);
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.25f) {
            Walk = true;
            Vector3 dir = toTarget.normalized;
            _desiredHorizontalVelocity = dir * (2f * chaseSpeedMultiplier);
            SmoothFaceDirection(dir, 10f);
        }
        else {
            Walk = false;
            _desiredHorizontalVelocity = Vector3.zero;
            SmoothFaceDirection(Player.transform.position - transform.position, 12f);
        }
    }

    private void FlankAroundPlayer() {
        if (Player == null || CharCon == null || !CharCon.enabled) return;

        _orbitAngle += _orbitDir * 55f * Time.deltaTime;
        float rad = _orbitAngle * Mathf.Deg2Rad;

        Vector3 desiredSlot = Player.transform.position +
            new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * flankRadius;
        desiredSlot = GetCameraAwareMoveTarget(desiredSlot);

        Vector3 toSlot = desiredSlot - transform.position;
        toSlot.y = 0f;

        // Deadzone prevents micro-stepping and jitter once near the orbit slot
        if (toSlot.sqrMagnitude > 0.36f) {
            Walk = true;
            Vector3 dir = toSlot.normalized;
            _desiredHorizontalVelocity = dir * 3.0f;
            SmoothFaceDirection(dir, 9f);
        }
        else {
            Walk = false;
            _desiredHorizontalVelocity = Vector3.zero;
            SmoothFaceDirection(Player.transform.position - transform.position, 10f);
        }
    }

    public void ConsumeByFamiliar() {
        if (!CanBeConsumedByFamiliar) return;

        ReleaseAttackToken();
        if (_attackRoutine != null) {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        // Trigger death/dissolve through your existing Health setter
        Health = 0;
    }

    private bool TryAcquireAttackToken() {
        if (_hasAttackToken) return true;
        if (_attackingCainas.Count >= maxSimultaneousAttackers) return false;

        _attackingCainas.Add(this);
        _hasAttackToken = true;
        return true;
    }

    private void ReleaseAttackToken() {
        if (!_hasAttackToken) return;
        _attackingCainas.Remove(this);
        _hasAttackToken = false;
    }
}