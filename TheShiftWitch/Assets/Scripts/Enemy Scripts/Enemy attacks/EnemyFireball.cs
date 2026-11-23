using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;
public class EnemyFireball : MonoBehaviour
{
    [SerializeField] private GameObject fireballHit;
    public static event UnityAction<int> dmg;
    private Vector3 direction;
    [SerializeField] private GameObject boom;
    private EnemyHitBox enemyHitBox;

    public Vector3 Direction { get => direction; set => direction = value; }

    // Start is called before the first frame update
    private void Start() {
        enemyHitBox = GetComponent<EnemyHitBox>();

        // Only set default if Direction hasn't been set by the spawner yet
        if (Direction == Vector3.zero) {
            Direction = transform.forward;
        }

        Destroy(gameObject, 10);
    }

    // Update is called once per frame
    private void Update() {
        transform.position += Direction * 25 * Time.deltaTime;
    }

    //private int AdditionPower()=>Player.GetPlayer().stats.Level 
    private void OnTriggerEnter(Collider other) {
        if (fireballHit)
            Instantiate(fireballHit, transform.position, transform.rotation);

        if (boom)
            Instantiate(boom, transform.position, transform.rotation);

        // Safely extract the damage from the caster
        if (enemyHitBox != null && enemyHitBox.Enemy != null) {
            // Cast the base enemy reference to a Mage to grab its specific fireball damage
            if (enemyHitBox.Enemy is NewbieWitch mage) {
                int finalDamage = mage.FireballDamage;
                //dmg?.Invoke(finalDamage); // Fire off your damage event!
            }
        }

        Destroy(gameObject);

    }
}
