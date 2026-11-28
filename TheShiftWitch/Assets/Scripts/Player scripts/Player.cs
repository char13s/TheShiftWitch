using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static event UnityAction onPlayerDeath;
    public static event UnityAction onHealthChange;
    private int health = 100;
    private int maxHealth = 100;
    private int mp = 100;
    private int maxMp = 100;
    private static Player instance;
    public static Player GetPlayer() => instance;
    private Animator anim;
    CinemachineImpulseSource pulse;
    CharacterController charCon;
    //internal Stats stats=new Stats();
    bool isDead = false;
    bool isAttacking = false;
    bool isInvincible = false;
    bool isPlayingSkill = false;
    bool hasTarget;
    bool lockedOn;
    float moveSpeed = 7;
    
    public bool IsInvincible { get => isInvincible; set => isInvincible = value; }
    public bool IsAttacking { get => isAttacking; set => isAttacking = value; }
    public bool IsPlayingSkill { get => isPlayingSkill; set => isPlayingSkill = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public Animator Anim { get => anim; set => anim = value; }
    public int Health { get => health; set { health = value; ProcessHealth(); onHealthChange?.Invoke(); } }
    public int Mp { get => mp; set => mp = value; }
    public bool LockedOn { get => lockedOn; set => lockedOn = value; }
    public bool HasTarget { get => hasTarget; set => hasTarget = value; }
    public CharacterController CharCon { get => charCon; set => charCon = value; }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }
        else {
            instance = this;
        }
        pulse = GetComponent<CinemachineImpulseSource>();
        anim = GetComponent<Animator>();
        CharCon = GetComponent<CharacterController>();
    }
    private void OnEnable() {
        Hitbox.onImpact += PlayDynamicShake;
        HurtBox.onImpact += PlayDynamicShake;
        ParryBox.screenshakeEvent += PlayDynamicShake;
    }
    private void OnDisable() {
        Hitbox.onImpact -= PlayDynamicShake;
        HurtBox.onImpact -= PlayDynamicShake;
        ParryBox.screenshakeEvent -= PlayDynamicShake;
    }
    private void Start() {
        Health = 100;
    }
    private void PlayDynamicShake(float intensity) {
        if (pulse == null) return;
        Vector3 shakeVector = new Vector3(intensity, intensity, intensity);
        pulse.GenerateImpulseWithVelocity(shakeVector);

    }
    public void TakeDamage(int val) {
        Health -= val;
    }
    private void ProcessHealth() { 
        if(health <= 0 && !isDead) {
            isDead = true;
            onPlayerDeath?.Invoke();
            Time.timeScale = 0f;
            anim.Play("Death");
        }
    }
}
