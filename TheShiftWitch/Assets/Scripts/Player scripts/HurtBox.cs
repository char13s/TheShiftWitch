using UnityEngine;
using UnityEngine.Events;

public class HurtBox : MonoBehaviour
{
    public static event UnityAction<float> onImpact;
    Player player;
    private void Start() {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyHitBox>()) { 
            WhichReaction(other.GetComponent<EnemyHitBox>());
            player.TakeDamage(other.GetComponent<EnemyHitBox>().Dmg);
            onImpact?.Invoke(0.2f);
        }
        print("something hit me");
    }
    private void WhichReaction(EnemyHitBox box) {
        switch (box.Hittype) {
            case EnemyHitBox.hitType.normal:
                print("normal hit");
                player.Anim.Play("Hurt");
                break;
            case EnemyHitBox.hitType.left:
                print("left hit");
                player.Anim.Play("HitToLeft");
                break;
            case EnemyHitBox.hitType.right:
                print("right hit");
                player.Anim.Play("HitToRight");
                break;
            case EnemyHitBox.hitType.stab:
                print("stab hit");
                player.Anim.Play("Stabbed");
                break;
            case EnemyHitBox.hitType.heavyHit:
                print("heavy hit");
                player.Anim.Play("KnockedBack");
                break;
        }

    }
}
