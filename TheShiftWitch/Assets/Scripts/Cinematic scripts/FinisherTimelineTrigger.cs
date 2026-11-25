using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider))]
public class FinisherTimelineTrigger : Interactable
{
    [Header("Timeline & Alignment")]
    [SerializeField] private PlayableDirector finisherDirector;
    [SerializeField] private Transform finisherAnchor; // Exact offset & facing for the clip
    [SerializeField] private float stepInDuration = 0.12f;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional Stagger / State Gate")]
    [SerializeField] private bool requireStaggerBroken = false;
    [SerializeField] private bool isStaggerBroken = true;

    private bool playerInZone = false;
    private Transform cachedPlayer;
    private Enemy ownerEnemy;
    private bool isPlayingFinisher = false;
    public static event UnityAction<int> freezeControls;

    private void Awake() {
        GetComponent<Collider>().isTrigger = true;
        ownerEnemy = GetComponentInParent<Enemy>();

        if (finisherDirector != null) {
            finisherDirector.stopped += OnFinisherTimelineStopped;
        }
    }

    private void OnDestroy() {
        if (finisherDirector != null) {
            finisherDirector.stopped -= OnFinisherTimelineStopped;
        }
    }

    public override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);
        if (other.CompareTag(playerTag)) {
            Debug.Log("player in finisher");
            playerInZone = true;
            cachedPlayer = other.transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(playerTag)) {
            playerInZone = false;
            if (!isPlayingFinisher) {
                cachedPlayer = null;
            }
        }
    }

    public void SetStaggerBroken(bool broken) {
        isStaggerBroken = broken;
    }

    // Call this from your player input/combat script when the finisher button is pressed
    public bool TryStartFinisher(Transform playerOverride = null) {
        if (isPlayingFinisher || finisherDirector == null) return false;
        if (requireStaggerBroken && !isStaggerBroken) return false;

        Transform player = playerOverride != null ? playerOverride : cachedPlayer;
        if (player == null || (!playerInZone && playerOverride == null)) return false;

        StartCoroutine(StepInAndPlayTimeline(player));
        return true;
    }

    private IEnumerator StepInAndPlayTimeline(Transform player) {
        isPlayingFinisher = true;
        isStaggerBroken = false;

        if (ownerEnemy != null) {
            ownerEnemy.Timelining = true;
        }
        freezeControls?.Invoke(2);
        Vector3 targetPos = finisherAnchor != null
            ? finisherAnchor.position
            : transform.position + transform.forward * 1.5f;
        targetPos.y = player.position.y;

        Quaternion targetRot = finisherAnchor != null
            ? finisherAnchor.rotation
            : Quaternion.LookRotation((transform.position - targetPos).normalized);

        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        float elapsed = 0f;

        while (elapsed < stepInDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stepInDuration);
            player.position = Vector3.Lerp(startPos, targetPos, t);
            player.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        player.position = targetPos;
        player.rotation = targetRot;

        finisherDirector.time = 0;
        finisherDirector.Play();
    }

    private void OnFinisherTimelineStopped(PlayableDirector director) {
        if (director != finisherDirector) return;

        isPlayingFinisher = false;
        if (ownerEnemy != null) {
            ownerEnemy.Timelining = false;
        }
    }

    public override void Interact() {
        TryStartFinisher();
    }
}