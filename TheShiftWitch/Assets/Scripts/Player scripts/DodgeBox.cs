using UnityEngine;

public class DodgeBox : MonoBehaviour
{
    Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyHitBox>()) {

        }
    }
}
