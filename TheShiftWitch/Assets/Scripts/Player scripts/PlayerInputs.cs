using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnLightAttack() { 
        animator.SetTrigger("LightAttack");
    }
}
