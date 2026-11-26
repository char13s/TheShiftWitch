using UnityEngine;
using UnityEngine.Events;

public class Hitbox : MonoBehaviour
{
    int hitStopFrames = 2;
    [SerializeField]private bool shakeOrNot;
    public static event UnityAction<float> onHitStop;
    public static event UnityAction<float> onImpact;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<EnemyBody>()) {
            other.GetComponent<EnemyBody>().SetBodyColor(Color.white);
        }
        onHitStop?.Invoke(hitStopFrames * Time.fixedDeltaTime);
        if (shakeOrNot) {
            onImpact?.Invoke(1);
        }
        else {
            onImpact?.Invoke(0.1f);
        }
    }
}
