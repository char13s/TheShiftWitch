using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyHitBox>())
            print("something hit me");
    }
}
