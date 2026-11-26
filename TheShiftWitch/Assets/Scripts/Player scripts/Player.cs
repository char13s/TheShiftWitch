using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player instance;
    public static Player GetPlayer() => instance;

    CinemachineImpulseSource pulse;

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
        pulse = GetComponent<CinemachineImpulseSource>();
    }
    private void OnEnable() {
        Hitbox.onImpact += PlayDynamicShake;
    }
    private void OnDisable() {
        Hitbox.onImpact -= PlayDynamicShake;
    }
    private void PlayDynamicShake(float intensity) {
        if (pulse == null) return;
        Vector3 shakeVector = new Vector3(intensity, intensity, intensity);
        pulse.GenerateImpulseWithVelocity(shakeVector);

    }
}
