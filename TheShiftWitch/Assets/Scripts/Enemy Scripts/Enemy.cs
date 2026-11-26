using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.Rendering.DebugUI;

public class Enemy : MonoBehaviour
{
    public enum EnemyHealthStatus { FullHealth, MeduimHealth, LowHealth }
    public enum StatusEffectType { None, Burned, Frozen, Paralyzed }
    public enum EnemyAiStates { Null, Idle, Attacking, Chasing, ReturnToSpawn, Dead, Hit, StatusEffect, Grabbed, Staggered };

    [SerializeField]private GameObject shieldObj;
    [SerializeField] private EnemyAiStates state;
    #region data
    private float distance;
    private bool dead;
    private bool attacking;
    float attackDistance = 2f;
    float attackDelay = 0f;
    bool hit;
    bool walk;
    bool lowHealth;
    bool timelining;
    float MoveSpeed = 2f;
    int shield;
    bool grounded;
    int cooldown;
    Coroutine hitCoroutine;
    #endregion

    internal EnemyStats stats = new EnemyStats();
    

    private Animator anim;
    private Player player;
    private CharacterController charCon;

    public Animator Anim { get => anim; set => anim = value; }
    public float Distance { get => distance; set => distance = value; }
    public float AttackDelay { get => attackDelay; set => attackDelay = value; }
    public bool Hit { get => hit; set => hit = value; }
    public EnemyAiStates State { get => state; set => state = value; }
    public bool Walk { get => walk; set => walk = value; }
    public bool Timelining { get => timelining; set => timelining = value; }
    public CharacterController CharCon { get => charCon; set => charCon = value; }
    public int Shield { get => stats.StaggerLeft; set => shield = value; }
    public Player Player { get => player; set => player = value; }
    public bool Grounded { get => grounded; set => grounded = value; }
    public int Cooldown { get => cooldown; set { cooldown = value; Anim.SetInteger("CoolDown",cooldown); } }
    public int Health { get => stats.Health; set { stats.Health = value; DetermineIfDead(); } }
    public int StaggerLeft { get => stats.StaggerLeft; set { stats.StaggerLeft = value; TrackStagger(); } }

    public bool Dead { get => dead; set { dead = value; Anim.SetBool("Dead",value); }
}
    private void Awake() {
        Anim=GetComponent<Animator>();
        charCon = GetComponent<CharacterController>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print("start");
        Player=Player.GetPlayer();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        States();
        if(Player!=null)
            Distance = Vector3.Distance(Player.transform.position, transform.position);
        if (!timelining)
            CharCon.Move(new Vector3(0, -4, 0) * Time.deltaTime);
        Grounded = CharCon.isGrounded;
    }
    private IEnumerator WaitForStaggerRecovery() {
        yield return new WaitForSeconds(4f);
        State = EnemyAiStates.Idle;
    }
    private IEnumerator WaitForCoolDown() { 
        yield return new WaitForSeconds(1f);
        Cooldown--;
    }
    public void TakeDamage(int damage) {

        Debug.Log("Enemy took " + damage + " damage!");
        Health -= damage;
    }
    public virtual void ShieldUp() {

        if (shieldObj != null) {
            //Shield = stats.StaggerLeft;
            shieldObj.SetActive(true);
        }
    }
    private void DetermineIfDead() { 
    if(Health <= 0) {
            Dead = true;
            State = EnemyAiStates.Dead;
            Anim.Play("Dead");
        }
    }
    public void StartCooldown() {
        StartCoroutine(WaitForCoolDown());
    }
    private void TrackStagger() { 
        if(stats.StaggerLeft <= 0) {
            State = EnemyAiStates.Staggered;
            Anim.Play("Stagger");
            StartCoroutine(WaitForStaggerRecovery());
        }
    }
    #region Reaction Animations & States

    public void Parried() => PlayAnimation("Parried");
    public void StunHit() => PlayAnimation("StunHit");
    public void PullIn() => PlayAnimation("Pull In");

    public void KnockedUp() {
        
        PlayAnimation("KnockedUp");
    }

    public void KnockedBack() {
        
        PlayAnimation("KnockedBack");
    }

    public virtual void OnHit() {
        

        if (!Grounded) {
            PlayAnimation("AirHit");
        }
        else {
            anim.SetTrigger("Hurt");
            PlayAnimation("Hurt");
        }
    }

    #endregion

    #region Knockback & Physics Logic

    public void KnockBack(HitBoxType type) {
       RotateToPlayer();

        

        switch (type) {
            case HitBoxType.Normal:
                OnHit();
                StartHitMovement(ApplyKnockback(distance: 0.5f, duration: 0.15f));
                break;

            case HitBoxType.Heavy:
                PlayAnimation("KnockedBack");
                StartHitMovement(ApplyKnockback(distance: 3.5f, duration: 0.25f));
                break;

            case HitBoxType.Magic:
                OnHit();
                // Magic causes micro-stutter/stagger without heavy displacement
                StartHitMovement(ApplyKnockback(distance: 0f, duration: 0.05f));
                break;
        }
    }

    public void KnockDown() {
        
        StartHitMovement(PushDown(speed: 50f));
        Anim.Play("Dropping");
    }
    public void CancelKnocked() {



    }
    public void KnockUpABit() {
        
        StartHitMovement(PushUp(distance: 1.15f, duration: 0.2f));
    }

    #endregion

    #region Coroutine Drivers & Helpers

    private void StartHitMovement(IEnumerator routine) {
        if (hitCoroutine != null) {
            StopCoroutine(hitCoroutine);
        }
        hitCoroutine = StartCoroutine(routine);
    }

    private void PlayAnimation(string stateName) {
        if (Anim != null) {
            Anim.Play(stateName);
        }
    }
    
    /// <summary>
    /// Smoothly pushes the enemy backward along their local forward axis using an ease-out curve.
    /// </summary>
    private IEnumerator ApplyKnockback(float distance, float duration) {
        float elapsed = 0f;
        Vector3 moveDirection = -transform.forward;

        while (elapsed < duration && isActiveAndEnabled) {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Ease-out curve formula: starts fast, slows down smoothly
            float speedMultiplier = 1f - Mathf.Pow(progress - 1f, 2);
            float stepDistance = (distance / duration) * (1f - progress) * 2f * Time.deltaTime;
            if (charCon.enabled)
                charCon.Move(moveDirection * stepDistance);
            yield return null;
        }
    }

    private IEnumerator PushDown(float speed) {
        while (isActiveAndEnabled && !Grounded) {
            charCon.Move(-Vector3.up * speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator PushUp(float distance, float duration) {
        float elapsed = 0f;

        while (elapsed < duration && isActiveAndEnabled) {
            elapsed += Time.deltaTime;
            float stepDistance = (distance / duration) * Time.deltaTime;
            CharCon.Move(Vector3.up * stepDistance);
            yield return null;
        }
    }
    #endregion
    public virtual void States() {

        switch (State) {
            case EnemyAiStates.Idle:
                Idle();
                break;
            case EnemyAiStates.Attacking:
                //Rbody.velocity = new Vector3(0, 0, 0);
                StartCoroutine(waitToAttack());
                break;
            //LowHealth();
            case EnemyAiStates.Chasing:
                Walk = true;

                //Chasing();
                break;
            //case EnemyAiStates.Staggered:
            //    //Call recovery coroutine
            //    recoveryCoroutine = StartCoroutine(WaitForStaggerRecovery());
            //    //set animation to stunned
            //    anim.SetBool("Stagger", true);
            //    break;
            default:
                break;
        }
    }
    public virtual void StateSwitch() {
        if (!Timelining && !Dead) {
            if (stats.Health < stats.HealthMax / 4) {
                lowHealth = true;
                //finisherTrigger.SetActive(true);
            }
            if (State == EnemyAiStates.Chasing) {
                SwitchToAttack();
                BackToIdle();
            }
            if (State == EnemyAiStates.Idle) {//What happens in Idle
                SwitchToAttack();
                ChasePlayer();
            }
            if (State == EnemyAiStates.Attacking) {
                ChasePlayer();
                BackToIdle();
                //SwitchToAttack();
            }
        }
    }
    public virtual void SwitchToAttack() {
        if (Distance < attackDistance && !Dead && !Hit) {
            State = EnemyAiStates.Attacking;
        }
        else {
            attacking = false;
        }
    }
    public virtual void ChasePlayer() {
        if (Distance > attackDistance && !Dead && !Hit) {
            State = EnemyAiStates.Chasing;
        }
        else {
            Walk = false;
        }
    }
    public virtual void BackToIdle() {
        if (Distance > 10f | AttackDelay > 0) {
            State = EnemyAiStates.Idle;
        }
    }
    IEnumerator waitToSuperAttack() {
        int num = Random.Range(7, 10);
        YieldInstruction wait = new WaitForSeconds(num);
        yield return wait;
        Anim.Play("Super");
    }
    IEnumerator waitToAttack() {
        int num = Random.Range(1, 3);
        YieldInstruction wait = new WaitForSeconds(num);
        yield return wait;
        Anim.SetTrigger("Attack 0");
        AttackDelay = 3;
    }
    public virtual void Idle() {
        Walk = false;
        attackDelay = 0;
    }
    public virtual void Chasing() {

        Walk = true;
        Vector3 targetPos = GetCameraAwareMoveTarget(Player.transform.position);
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f; // Keep on ground plane
        if (moveDir.sqrMagnitude > 0.05f) {
            Vector3 normalizedDir = moveDir.normalized;

            // Smooth rotation toward camera entry vector
            Quaternion targetRot = Quaternion.LookRotation(normalizedDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);

            // Drive physical movement via CharacterController
            Vector3 velocity = normalizedDir * MoveSpeed * 2 + Vector3.up * Physics.gravity.y;
            CharCon.Move(velocity * Time.deltaTime);

            //RotateToPlayer();
            //CharCon.Move(transform.forward * speed * Time.deltaTime);
        }
    }
    public virtual void AttackRot() {
        RotateToPlayer();
    }

    public void RotateToPlayer() {
        if (Distance > 0.1f) {
            Vector3 delta =Player.transform.position - transform.position;
            delta.y = 0;
            transform.rotation = Quaternion.LookRotation(delta);
        }
    }
    public Vector3 GetCameraAwareMoveTarget(Vector3 playerPosition) {
        if (Camera.main == null) return playerPosition;

        // Raycast into center of screen space at player depth
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        float distToPlayer = Vector3.Distance(Camera.main.transform.position, playerPosition);
        Vector3 screenCenterPoint = cameraRay.GetPoint(distToPlayer);

        // Pull 60% toward center of camera view, 40% toward player
        return Vector3.Lerp(playerPosition, screenCenterPoint, 0.6f);
    }
    public void CalculateAttack(int dmg) { 
    
    }

}
