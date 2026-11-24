using System.Collections.Generic;
using UnityEngine;

public class LightningStrikeSpawner : MonoBehaviour
{
    [Tooltip("Prefab with a Trigger Collider and your damage script")]
    public GameObject impactColliderPrefab;

    [Tooltip("How long the collider stays active before disappearing")]
    public float colliderDuration = 0.15f;

    private ParticleSystem partSystem;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start() {
        partSystem = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other) {
        // Get all collision points that happened this frame
        int numCollisionEvents = partSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++) {
            Vector3 hitPosition = collisionEvents[i].intersection;

            // Spawn the hitbox at the exact point the lightning touched the ground
            GameObject hitbox = Instantiate(impactColliderPrefab, hitPosition, Quaternion.identity);
            Destroy(hitbox, colliderDuration);
        }
    }
}