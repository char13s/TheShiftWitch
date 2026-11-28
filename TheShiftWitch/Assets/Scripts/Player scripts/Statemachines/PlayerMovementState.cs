using UnityEngine;

public class PlayerMovementState : StateMachineBehaviour
{
    //[SerializeField] Vector3 direction;
    [SerializeField] float move;
    [SerializeField] private bool forward;
    [SerializeField] private float moveOn;
    [SerializeField] private float moveOff;
    Player player;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        player = Player.GetPlayer();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (stateInfo.normalizedTime > moveOn && stateInfo.normalizedTime < moveOff) {
            if (!forward) {
                player.CharCon.Move(player.transform.right * move * Time.deltaTime);
            }
            else {
                player.CharCon.Move(player.transform.forward * move * Time.deltaTime);
            }
        }
    }
}
