using UnityEngine;

public class EnemyFinisherHit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.GetComponent<Enemy>().Cooldown = 10;
        animator.GetComponent<Enemy>().StartCooldown();
    }
}
