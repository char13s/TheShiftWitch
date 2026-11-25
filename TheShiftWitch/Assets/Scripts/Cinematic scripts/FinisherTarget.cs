using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class FinisherData
{
    public string playerFinisherState = "DevourFinisher";
    public string victimReactionState = "DevouredVictim";
    public float snapDistance = 1.5f;
    public float duration = 1.8f;
    public int finishDamage = 9999; // High enough to execute regular knights; tuned for bosses
}

public class FinisherTarget : MonoBehaviour
{
    [Header("Finisher Setup")]
    public FinisherData finisherData = new FinisherData();
    public bool isStaggerBroken = false;
    [SerializeField] private GameObject staggerPromptVFX; // Glow ring or UI prompt when ready

    private Enemy enemyRef;
    private Animator anim;

    private void Awake() {
        enemyRef = GetComponent<Enemy>();
        anim = GetComponentInChildren<Animator>();
        if (staggerPromptVFX != null) staggerPromptVFX.SetActive(false);
    }

    public void SetStaggerBroken(bool broken) {
        isStaggerBroken = broken;
        if (staggerPromptVFX != null) staggerPromptVFX.SetActive(broken);
    }

    public IEnumerator ExecuteFinisherSequence(Transform playerTransform, Animator playerAnim, CinemachineCamera finisherCam = null) {
        isStaggerBroken = false;
        if (staggerPromptVFX != null) staggerPromptVFX.SetActive(false);

        if (enemyRef != null) {
            enemyRef.Timelining = true; // Locks AI & movement during finisher
        }

        // 1. Calculate exact paired anchor in front of the enemy
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;
        flatForward.Normalize();

        Vector3 targetPlayerPos = transform.position + flatForward * finisherData.snapDistance;
        targetPlayerPos.y = playerTransform.position.y;
        Quaternion targetPlayerRot = Quaternion.LookRotation(-flatForward);

        // 2. Quick 0.1s warp so player & enemy align with the Blender paired spacing
        float snapTime = 0.1f;
        float elapsed = 0f;
        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;

        while (elapsed < snapTime) {
            elapsed += Time.deltaTime;
            float t = elapsed / snapTime;
            playerTransform.position = Vector3.Lerp(startPos, targetPlayerPos, t);
            playerTransform.rotation = Quaternion.Slerp(startRot, targetPlayerRot, t);
            yield return null;
        }

        // 3. Activate dramatic finisher camera (if assigned)
        if (finisherCam != null) finisherCam.Priority = 30;

        // 4. Trigger paired animations simultaneously (safe if clips aren't assigned yet in graybox)
        if (playerAnim != null && !string.IsNullOrEmpty(finisherData.playerFinisherState)) {
            playerAnim.CrossFadeInFixedTime(finisherData.playerFinisherState, 0.05f);
        }
        if (anim != null && !string.IsNullOrEmpty(finisherData.victimReactionState)) {
            anim.CrossFadeInFixedTime(finisherData.victimReactionState, 0.05f);
        }

        // Wait until impact/bite frame (or full duration)
        yield return new WaitForSeconds(finisherData.duration);

        // 5. Apply finish damage / execution & restore camera/state
        if (finisherCam != null) finisherCam.Priority = 0;

        if (enemyRef != null) {
            enemyRef.Timelining = false;
            // Call your Enemy damage/death method here, e.g.:
            // enemyRef.TakeDamage(finisherData.finishDamage);
        }
    }
}