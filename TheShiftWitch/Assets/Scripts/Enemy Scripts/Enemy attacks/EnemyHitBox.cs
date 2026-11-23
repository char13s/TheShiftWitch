using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
#pragma warning disable 0649
public class EnemyHitBox : MonoBehaviour
{
    public enum EnemyHitBoxType { Normal, Fireball, Slam, Beam, Slash, Stab, Lightning, IceShard }
    public enum hitType
    {
        normal,
        left,
        right,
        stab,
        heavyHit,
        down
    }
    [SerializeField] private Elements element;
    [SerializeField] private EnemyHitBoxType type;
    [SerializeField] private hitType hittype;
    [SerializeField] private Enemy enemy;
    private Player player;
    //[SerializeField] private GameObject effect;
    public static event UnityAction<int> sendDmg;
    public static event UnityAction sendSignal;
    [SerializeField] private bool isProjectile;
    [SerializeField] private int dmg;
    [SerializeField] private int elementalDmg;
    public Enemy Enemy { get => enemy; set => enemy = value; }
    public EnemyHitBoxType Type { get => type; set => type = value; }
    public Elements Element { get => element; set => element = value; }
    public int ElementalDmg { get => elementalDmg; set => elementalDmg = value; }
    public hitType Hittype { get => hittype; set => hittype = value; }
    // Start is called before the first frame update
    void Start() {
        player = Player.GetPlayer();
        //if(!isProjectile)
        //if (enemy == null)
        //    Enemy = GetComponentInParent<Enemy>();
    }
    private void OnEnable() {
        StartCoroutine(waitToReset());
        sendSignal?.Invoke();
        //TimeSlowCollider.queVolume += TimeFroze;
    }
    private void OnDisable() {
        //TimeSlowCollider.queVolume -= TimeFroze;
    }
    private IEnumerator waitToReset() {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }
    private void TimeFroze(float val) {
        gameObject.SetActive(false);
    }
    public void InitializeHitBox(Enemy caster, int damageValue, int elementalDamageValue = 0) {
        this.enemy = caster;
        this.dmg = damageValue;
        this.elementalDmg = elementalDamageValue;
    }
    private void OnTriggerEnter(Collider other) {
        // 1. Guard check for Player states
        if (other.CompareTag("Player")) {

            var zendComponent = other.GetComponent<Player>();
            if (zendComponent != null && zendComponent.IsInvincible)
                return;
        }

        // 2. THE FIX: Fallback safety if InitializeHitBox wasn't called
        if (enemy == null) {
            enemy = GetComponentInParent<Enemy>();
        }

        // 3. Ultimate safety belt: If it's STILL null, log it clearly so it doesn't break silently
        if (enemy == null) {
            Debug.LogWarning($"[HitBox Warning] {gameObject.name} collided with {other.name}, but has no Enemy caster assigned! Skipping switch.");
            return;
        }

        // 4. Run the damage switch
        switch (type) {
            case EnemyHitBoxType.Normal:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    DecidedHowToSendDamage();
                }
                break;

            case EnemyHitBoxType.Fireball:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    print("Calc attack ran for Fireball!");
                    DecidedHowToSendDamage();
                }
                break;

            case EnemyHitBoxType.Slam:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    if (!player.IsInvincible) {
                        DecidedHowToSendDamage();
                    }
                }
                break;

            case EnemyHitBoxType.Beam:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    if (    player.IsInvincible) {
                        DecidedHowToSendDamage();
                    }
                }
                if (other.CompareTag("Parry") && isProjectile) {
                    Destroy(gameObject);
                }
                break;

            case EnemyHitBoxType.Slash:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    DecidedHowToSendDamage();
                }
                break;

            case EnemyHitBoxType.Stab:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    DecidedHowToSendDamage();
                }
                break;

            case EnemyHitBoxType.Lightning:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    DecidedHowToSendDamage();
                    print("STRUCK");
                }
                break;

            case EnemyHitBoxType.IceShard:
                if (other.CompareTag("HurtBox") || other.CompareTag("Player")) {
                    DecidedHowToSendDamage();
                }
                break;
        }
    }
    private void DecidedHowToSendDamage() {
        if (enemy != null) {
            if (isProjectile) {
                DetermineDamage();
            }
            else {
                DetermineDamageRemotely();
            }
        }
    }
    private void DetermineDamage() {
        if (!player.IsInvincible) {
            Enemy.CalculateAttack(dmg);
        }
    }
    private void DetermineDamageRemotely() {
        if (!player.IsInvincible) {
            Enemy.CalculateAttack(dmg);
        }

    }
}
