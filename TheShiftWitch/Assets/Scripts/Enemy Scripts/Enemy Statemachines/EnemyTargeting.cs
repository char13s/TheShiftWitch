using UnityEngine;

public class EnemyTargeting : StateMachineBehaviour
{
    [SerializeField] private float start = 0;
    [SerializeField] private float end = 1;
    [SerializeField] private bool looped = false;
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

        Enemy enemy = animator.gameObject.GetComponent<Enemy>();
        if (enemy == null||enemy.Player==null) return;
        if (stateInfo.normalizedTime >= start && (stateInfo.normalizedTime < end||looped)) {
            Transform target = enemy.Player.transform;
            if (target != null) {
                Vector3 direction = (target.position - enemy.transform.position).normalized;
                direction.y = 0; // Keep only horizontal rotation
                if (direction != Vector3.zero) {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 15);
                }
            }
        }
    }
}
