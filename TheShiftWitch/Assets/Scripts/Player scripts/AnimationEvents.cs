using System.ComponentModel;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private GameObject hitbox;
    [SerializeField] private GameObject parrybox;
    [SerializeField] private GameObject dodgebox;
    [SerializeField] private GameObject hurtBox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AttackState.disableHitbox += DeactivateHitbox;
    }
    private void OnDisable() {
        AttackState.disableHitbox -= DeactivateHitbox;
    }
    public void ActivateHitbox() {
        hitbox.GetComponent<Hitbox>().Type = HitBoxType.Normal;
        hitbox.SetActive(true);
    }
    public void ActivateStrongHitBox() {
        hitbox.GetComponent<Hitbox>().Type = HitBoxType.Heavy;
        hitbox.SetActive(true);
    }
    public void DeactivateHitbox() {
        hitbox.SetActive(false);
    }
    public void Parry() { 
        parrybox.SetActive(true);
    }
    public void Dodge() { 
        dodgebox.SetActive(true);
    }
    public void CancelParry() { 
        parrybox?.SetActive(false);
    }
    public void CancelDodge() { 
        dodgebox?.SetActive(false);
    }
    public void ActivateHurtBox() { 
        hurtBox.SetActive(true);
    }
    public void DeactivateHurtBox() { 
        hurtBox.SetActive(false);
    }
}
