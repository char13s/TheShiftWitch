using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player instance;
    public static Player GetPlayer() => instance;

    bool isDead = false;
    bool isAttacking = false;
    bool isInvincible = false;
    bool isPlayingSkill = false;
    float moveSpeed = 7;
    public bool IsInvincible { get => isInvincible; set => isInvincible = value; }
    public bool IsAttacking { get => isAttacking; set => isAttacking = value; }
    public bool IsPlayingSkill { get => isPlayingSkill; set => isPlayingSkill = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }
        else {
            instance = this;
        }
    }
}
