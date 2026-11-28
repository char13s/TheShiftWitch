using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider healthbar;
    [SerializeField] private TextMeshProUGUI healthText;
    Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable() {
        Player.onHealthChange += DisplayHealth;
        
    }

    private void OnDisable() {
        Player.onHealthChange -= DisplayHealth;
    }
    void Start()
    {
        player= Player.GetPlayer();
        healthbar.maxValue = 100;
    }
    private void DisplayHealth() { 
         
        healthbar.value = player.Health;
        healthText.text = healthbar.value.ToString()+"/"+"100";
    }
}
