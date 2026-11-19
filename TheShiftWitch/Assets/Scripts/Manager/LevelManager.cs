using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int currentLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void ChangeSceneDiscretely(int val) {
        currentLevel = val;
        SceneManager.LoadSceneAsync(val, LoadSceneMode.Additive);
    }
    public void ChangeSceneImmediately(int val) {
        currentLevel = val;
        SceneManager.LoadSceneAsync(val);
    }
}
