using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private GameObject hitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AttackState.disableHitbox += DeactivateHitbox;
    }
    private void OnDisable() {
        AttackState.disableHitbox -= DeactivateHitbox;
    }
    public void ActivateHitbox() {
        hitbox.SetActive(true);
    }
    public void DeactivateHitbox() {
        hitbox.SetActive(false);
    }
}
