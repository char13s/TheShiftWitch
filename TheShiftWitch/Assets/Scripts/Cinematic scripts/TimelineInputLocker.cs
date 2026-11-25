using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using StarterAssets;
public class TimelineInputLocker : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayerInput playerInput; // Optional: if using Unity Input System
    [SerializeField] private Behaviour[] scriptsToDisableDuringTimeline; // e.g., movement & combat scripts
    [SerializeField] private ThirdPersonController targetController; // Optional: to clear velocity on start

    private void OnEnable() {
        if (director != null) {
            director.played += OnTimelinePlayed;
            director.stopped += OnTimelineStopped;
        }
    }

    private void OnDisable() {
        if (director != null) {
            director.played -= OnTimelinePlayed;
            director.stopped -= OnTimelineStopped;
        }

        // Safety restore if disabled mid-cutscene
        SetGameplayInputEnabled(true);
    }

    private void OnTimelinePlayed(PlayableDirector _) {

       targetController.SetTimelineLock(true); // Lock the player controller to prevent movement during the timeline
        SetGameplayInputEnabled(false);
        Player.GetPlayer().GetComponent<CharacterController>().Move(Vector3.zero); // Clear velocity to prevent sliding
        Player.GetPlayer().MoveSpeed = 0f; // Optional: stop player movement speed
    }

    private void OnTimelineStopped(PlayableDirector _) {
        targetController.SetTimelineLock(false); // Unlock the player controller after the timeline
        SetGameplayInputEnabled(true);
    }

    public void SetGameplayInputEnabled(bool enabled) {
        if (playerInput != null) {
            playerInput.enabled = enabled;
        }

        if (scriptsToDisableDuringTimeline == null) return;

        for (int i = 0; i < scriptsToDisableDuringTimeline.Length; i++) {
            if (scriptsToDisableDuringTimeline[i] != null) {
                scriptsToDisableDuringTimeline[i].enabled = enabled;
            }
        }
    }
}