using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SlowTime() {
        Time.timeScale = 0.35f;
    }
    public void ResumeTime() {
        Time.timeScale = 1.0f;
    }
}
