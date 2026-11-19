using UnityEngine;
using UnityEngine.Events;

public class AttackState :StateMachineBehaviour
{
    public static event UnityAction disableHitbox;
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        disableHitbox?.Invoke();
    }
}
