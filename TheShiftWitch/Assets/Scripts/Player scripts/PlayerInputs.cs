using UnityEngine;
using UnityEngine.Events;

public class PlayerInputs : MonoBehaviour
{
    public static event UnityAction onPause;
    Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnLightAttack() { 
        animator.SetTrigger("LightAttack");
    }
    private void OnParry() {
        animator.Play("Parry");
    }
    private void OnDodge() {
        animator.Play("Dodge");
    }
    private void OnPause() { 
        onPause?.Invoke();
    }
}
