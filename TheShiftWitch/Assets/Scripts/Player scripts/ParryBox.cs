using UnityEngine;
using UnityEngine.Events;

public class ParryBox : MonoBehaviour
{
    Player player;
    public static event UnityAction<float> hitstopEvent;
    public static event UnityAction<float> screenshakeEvent;
    public static event UnityAction<float,float> rumbleEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player=GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other) {
        
        if (other.GetComponent<EnemyHitBox>()) {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            other.GetComponent<EnemyHitBox>().gameObject.SetActive(false);
            //vfx
            //sfx
            //hitstop
            hitstopEvent?.Invoke(0.1f);
            //screenshake
            screenshakeEvent?.Invoke(0.1f);
            //enemy staggered
            enemy.Anim.Play("Staggered");
            //player animation
            player.Anim.Play("ParryCounter");
            // player invincibility frames
            //post processing effects
            //controller rumble
            rumbleEvent?.Invoke(0.1f,0.1f);
        }
    }
}
