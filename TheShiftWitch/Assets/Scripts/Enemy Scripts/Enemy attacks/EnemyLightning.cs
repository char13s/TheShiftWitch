using System.Collections;
using UnityEngine;

public class EnemyLightning : MonoBehaviour
{
    [Header("Phase Groups")]
    [SerializeField] private GameObject telegraphGroup; // Parent of all charging/warning VFX
    [SerializeField] private GameObject strikeGroup;    // Parent of the actual lightning strike VFX

    [Header("Timing Settings")]
    [SerializeField] private float warningDuration = 0.6f; // How long player has to react
    [SerializeField] private float strikeDuration = 0.15f; // How long the active hitbox hurts the player

    [Header("Components")]
    [SerializeField] private Collider strikeCollider; // The trigger collider that hits Zend
    private EnemyHitBox hitBox;

    public EnemyHitBox HitBox { get => hitBox; set => hitBox = value; }

    private void Awake() {
        if (strikeCollider != null) {
            hitBox = strikeCollider.GetComponent<EnemyHitBox>();
        }

        // Ensure we start in the warning phase
        if (telegraphGroup != null) telegraphGroup.SetActive(true);
        if (strikeGroup != null) strikeGroup.SetActive(false);
        if (strikeCollider != null) strikeCollider.enabled = false;
    }

    public void TriggerLightningStrike(Transform targetTransform, Enemy mageReference, int damageValue, int elementalDamageValue = 0) {
        // Safely find the hitbox if Awake missed it
        if (hitBox == null) {
            hitBox = GetComponentInChildren<EnemyHitBox>();
        }

        // Initialize using our updated robust method
        if (hitBox != null) {
            hitBox.InitializeHitBox(mageReference, damageValue, elementalDamageValue);
        }
        else {
            Debug.LogError($"[Lightning Error] No EnemyHitBox component found on {gameObject.name} or its children!");
        }

        // Snap the strike position to the player's ground position
        transform.position = targetTransform.position;

        StartCoroutine(ExecuteStrikeSequence());
    }

    private IEnumerator ExecuteStrikeSequence() {
        // 1. WARNING PHASE: Let the telegraph particles play
        yield return new WaitForSeconds(warningDuration);
        print("STRIKKEEEEEEEEEEEEE");
        // 2. TRANSITION: Turn off telegraph, turn on the massive lightning bolt!
        if (telegraphGroup != null) telegraphGroup.SetActive(false);
        if (strikeGroup != null) strikeGroup.SetActive(true);
        if (strikeCollider != null) strikeCollider.enabled = true;

        // 3. DAMAGE WINDOW: Keep the collider active briefly so it feels snappy
        yield return new WaitForSeconds(strikeDuration);
        if (strikeCollider != null) strikeCollider.enabled = false;

        // 4. CLEANUP: Wait for the lightning sparks to fully fade before deleting
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}