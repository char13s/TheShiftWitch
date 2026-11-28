using UnityEngine;
using UnityEngine.Events;

public class Hitbox : MonoBehaviour
{
    int hitStopFrames = 2;
    [SerializeField] private HitBoxType type;
    [SerializeField] private Elements elements;
    [SerializeField] private bool shakeOrNot;

    public HitBoxType Type { get => type; set => type = value; }
    public Elements Elements { get => elements; set => elements = value; }

    public static event UnityAction<float> onHitStop;
    public static event UnityAction<float> onImpact;
    public static event UnityAction<float, float> impulse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }
    private void OnTriggerEnter(Collider other) {

        if (other.GetComponent<EnemyBody>()) {
            EnemyBody body = other.GetComponent<EnemyBody>();
            body.HitReactionType(type);
            body.SetBodyColor(Color.white);
            impulse?.Invoke(0.1f, 0.1f);
            onHitStop?.Invoke(hitStopFrames * Time.fixedDeltaTime);
            if (shakeOrNot) {
                onImpact?.Invoke(1);
            }
            else {
                onImpact?.Invoke(0.1f);
            }

        }


    }
}
