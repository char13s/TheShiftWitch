using System.Collections;
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

    private void Awake() {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player=Player.GetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        States();
        Distance = Vector3.Distance(player.transform.position, transform.position);
    }
    public void TakeDamage(int damage) {
        Debug.Log("Enemy took " + damage + " damage!");
        stats.Health -= damage;
    }
    public virtual void ShieldUp() {

        if (shieldObj != null) {
            //Shield = stats.StaggerLeft;
            shieldObj.SetActive(true);
        }
    }
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
        if (!Timelining && !dead) {
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
        if (Distance < attackDistance && !dead && !Hit) {
            State = EnemyAiStates.Attacking;
        }
        else {
            attacking = false;
        }
    }
    public virtual void ChasePlayer() {
        if (Distance > attackDistance && !dead && !Hit) {
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
        Vector3 targetPos = GetCameraAwareMoveTarget(player.transform.position);
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
            Vector3 delta =player.transform.position - transform.position;
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
