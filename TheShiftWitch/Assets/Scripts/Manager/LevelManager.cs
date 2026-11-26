using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static event UnityAction unPauseGame;
    private int currentLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLevel = SceneManager.GetActiveScene().buildIndex;
    }
    public void ChangeSceneDiscretely(int val) {
        currentLevel = val;
        SceneManager.LoadSceneAsync(val, LoadSceneMode.Additive);
    }
    public void ChangeSceneImmediately(int val) {
        currentLevel = val;
        SceneManager.LoadSceneAsync(val);
    }
    public void ReloadCurrent() {
       unPauseGame?.Invoke();
        SceneManager.LoadScene(currentLevel);
        //SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
    }
}
