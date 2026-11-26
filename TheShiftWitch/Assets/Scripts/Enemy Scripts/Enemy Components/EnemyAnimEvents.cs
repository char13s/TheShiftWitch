using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EnemyAnimEvents : MonoBehaviour
{
    [SerializeField] private GameObject hitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void ActivateHitbox() {
        hitbox.SetActive(true);
    }
    public void DeactivateHitbox() {
        hitbox.SetActive(false);
    }
}
