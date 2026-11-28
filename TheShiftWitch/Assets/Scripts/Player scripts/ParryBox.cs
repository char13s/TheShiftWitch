using UnityEngine;

public class ParryBox : MonoBehaviour
{
    Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player=GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other) {
        
        if (other.GetComponent<EnemyHitBox>()) {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            //vfx
            //sfx
            //hitstop
            //screenshake
            //enemy staggered
            enemy.Anim.Play("STaggered");
            //player animation
            // player invincibility frames
            //post processing effects
        }
    }
}
