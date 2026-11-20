using UnityEngine;

public class PauseScreenCanvas : CanvasManager
{
    private void OnEnable() {
        GameManager.pauseScreen += CanvasControl;
    }
    private void OnDisable() {
        GameManager.pauseScreen -= CanvasControl;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

}
