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
        
        hitbox.GetComponent<EnemyHitBox>().Hittype = EnemyHitBox.hitType.normal;
        hitbox.GetComponent<EnemyHitBox>().Dmg = 10;
        hitbox.SetActive(true);
    }
    public void DeactivateHitbox() {
        hitbox.SetActive(false);
    }
    public void ActivateStrongHitbox() {
        hitbox.GetComponent<EnemyHitBox>().Hittype = EnemyHitBox.hitType.heavyHit;
        hitbox.GetComponent<EnemyHitBox>().Dmg=30;
        hitbox.SetActive(true);
    }
}
