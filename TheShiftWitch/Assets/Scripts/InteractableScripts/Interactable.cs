using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class Interactable : MonoBehaviour
{
    public static event UnityAction<Interactable> sendThis;
    public static event UnityAction<bool> interact;
    public virtual void OnTriggerEnter(Collider other) {
        //send timeline to playerInputs
        EnterEffect();
        print("interactable entered");
    }
    private void OnTriggerStay(Collider other) {
        //Interact();    
    }
    private void OnTriggerExit(Collider other) {
        ExitEffect();
        print("interactable exited");
    }
    public abstract void Interact();
    public virtual void EnterEffect() {
        sendThis?.Invoke(this);
        interact?.Invoke(true);
    }
    public virtual void ExitEffect() {
        sendThis?.Invoke(null);
        interact?.Invoke(false);
    }
}