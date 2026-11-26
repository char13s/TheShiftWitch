using System.Collections;

using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    bool pause;
    private bool isWaiting;

    public static event UnityAction<bool> pauseScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable() {
        PlayerInputs.onPause += PauseGameControls;
        Hitbox.onHitStop += StartHitStop;
    }
    private void OnDisable() {
        PlayerInputs.onPause -= PauseGameControls;
        Hitbox.onHitStop -= StartHitStop;
    }
    void Start()
    {
        
    }
    private void PauseGame() {
        pause = true;
        Time.timeScale = 0;
        //CurrentState = GameState.Paused;
        pauseScreen?.Invoke(pause);
    }

    private void UnpauseGame() {
        pause = false;
        Time.timeScale = 1;
        pauseScreen?.Invoke(pause);
       // isWaiting = false;
        //CurrentState = GameState.PlayMode;
    }
    public void PauseGameControls() {
        if (pause) { UnpauseGame(); }
        else {
            PauseGame();
            //resetPauseScreen?.Invoke();
        }
        pauseScreen?.Invoke(pause);
    }
    private void StartHitStop(float duration) { 
    StartCoroutine(HitStop(duration));
    }
    private IEnumerator HitStop(float duration) {
        isWaiting = true;
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(duration);
        //if (currentState == GameState.PlayMode) {
            Time.timeScale = 1.0f;
       // }
        isWaiting = false;
    }
}
