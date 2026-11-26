using UnityEngine;

public class EnemyMovement : StateMachineBehaviour
{
    [SerializeField] bool isMovingForward;
    [SerializeField] private Vector3 move;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float start;
    [SerializeField] private float finish;
    [SerializeField] private bool loop;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (stateInfo.normalizedTime >= start && (stateInfo.normalizedTime <= finish||loop)) {
            if (isMovingForward && animator.GetComponent<CharacterController>())
                animator.GetComponent<CharacterController>().Move(animator.gameObject.transform.forward * moveSpeed * Time.deltaTime);
            else
                animator.GetComponent<CharacterController>().Move(move * moveSpeed * Time.deltaTime);
        }
    }
}
