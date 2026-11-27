using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    public static event UnityAction click;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>() .onClick.AddListener(click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
