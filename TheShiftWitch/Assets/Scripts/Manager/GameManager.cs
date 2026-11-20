using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    bool pause;
    public static event UnityAction<bool> pauseScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable() {
        PlayerInputs.onPause += PauseGameControls;
    }
    private void OnDisable() {
        PlayerInputs.onPause -= PauseGameControls;
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
}
