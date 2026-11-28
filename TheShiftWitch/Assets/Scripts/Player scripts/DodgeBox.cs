using UnityEngine;
using UnityEngine.Events;

public class DodgeBox : MonoBehaviour
{
    Player player;
    public static event UnityAction<float> hitstopEvent;
    public static event UnityAction<float> screenshakeEvent;
    public static event UnityAction<float, float> rumbleEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyHitBox>()) {
            print("Dodged");
            //vfx
            //sfx
            //hitstop
            hitstopEvent?.Invoke(0.3f);
            //screenshake
            screenshakeEvent?.Invoke(0.1f);
            rumbleEvent?.Invoke(0.1f, 0.1f);
            //player teleport animation
            // player invincibility frames
            //post processing effects
        }
    }
}
