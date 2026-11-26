using Unity.VisualScripting;
using UnityEngine;

public class KnightClass : Enemy
{

    public override void States() {
        base.States();

       // IsVisible = IsOnScreen();
        Anim.SetFloat("Distance", Distance);
       // Anim.SetBool("IsVisible", IsVisible);

        // OFF-SCREEN MOVEMENT: Steer back into camera frustum using CharacterController

    }

}
