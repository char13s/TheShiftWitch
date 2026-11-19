using UnityEngine;

public abstract class Observer : MonoBehaviour
{
    public virtual void Update() {
        Condition();
    }
    public abstract void Condition();
}
