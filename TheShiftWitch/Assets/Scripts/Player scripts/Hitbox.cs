using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyBody>()) { 
            other.GetComponent<EnemyBody>().SetBodyColor(Color.white);
        }
    }
}
