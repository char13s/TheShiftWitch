using UnityEngine;


public class CombatLockStateBehaviour : StateMachineBehaviour
{
    [SerializeField] private bool lockMove = true;
    [SerializeField] private bool lockRot = true;

    private BaseMovementAndCamera _movement;
    private bool _appliedMove;
    private bool _appliedRot;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (_movement == null) {
            _movement = animator.GetComponentInParent<BaseMovementAndCamera>();
        }
        if (_movement == null) return;

        if (lockMove) {
            _movement.lockMovement = true;
            _appliedMove = true;
        }

        if (lockRot) {
            _movement.lockRotation = true;
            _appliedRot = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (_movement == null) return;

        if (_appliedMove) {
            _movement.lockMovement = false;
            _appliedMove = false;
        }

        if (_appliedRot) {
            _movement.lockRotation = false;
            _appliedRot = false;
        }
    }
}