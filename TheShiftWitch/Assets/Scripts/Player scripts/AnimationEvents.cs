using System.ComponentModel;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private GameObject hitbox;
    [SerializeField] private GameObject parrybox;
    [SerializeField] private GameObject dodgebox;
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
}
