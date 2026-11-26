using UnityEngine;

public class GameOverCanvas : CanvasManager {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable() {
        Player.onPlayerDeath += PlayerDeath;
    }
    private void OnDisable() {
        Player.onPlayerDeath -=PlayerDeath;
    }
    private void PlayerDeath() {
        CanvasControl(true);
    }
}
