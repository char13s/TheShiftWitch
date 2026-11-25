using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public static event UnityAction onPause;
    Animator animator;
    private PlayerInput map;
    CharacterController charcon;
    Interactable currentInteractable;
    private void OnEnable() {
        FinisherTimelineTrigger.freezeControls += SwitchMaps;
        Interactable.sendThis += (interactable) => { currentInteractable = interactable; print("interactable found"); };
    }
    private void OnDisable() {
        FinisherTimelineTrigger.freezeControls -= SwitchMaps;
        Interactable.sendThis -= (interactable) => { currentInteractable = interactable; print("interactable lost"); };
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charcon=GetComponent<CharacterController>();
        map = GetComponent<PlayerInput>();
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
    private void OnDevour() {
        animator.SetTrigger("Devour");
    }
    private void OnPause() { 
        onPause?.Invoke();
    }
    private void OnInteract() {
        print("interactable touched");
        if (currentInteractable != null) {
            currentInteractable.Interact();
        }
    }
    private void SwitchMaps(int val) {
        charcon.Move(Vector3.zero);
        ////charcon.enabled = false;
        //map.DeactivateInput();
        ////mapping = val;
        //switch (val) {
        //    case 0: map.SwitchCurrentActionMap("Player"); break;
        //    case 1: map.SwitchCurrentActionMap("UI"); break;
        //    case 2: map.SwitchCurrentActionMap("Timeline"); break;
        //    case 3: map.SwitchCurrentActionMap("Dialogue Controls"); break;
        //    case 4: map.SwitchCurrentActionMap("TimelineControls"); break;
        //    case 5: map.SwitchCurrentActionMap("BatControls"); break;
        //    case 6: map.SwitchCurrentActionMap("RewardCanvasControls"); break;
        //    case 7: map.SwitchCurrentActionMap("QTE Controls"); break;
        //    case 99: map.SwitchCurrentActionMap("EmptyControls"); break;
        //}
        //print("Map switched");
        ////controls = val;
    }
}
