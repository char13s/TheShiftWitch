using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    private int health = 100;
    private int healthMax = 100;
    private int staggerLeft = 100;
    public int Health { get => health; set => health = value; }
    public int HealthMax { get => healthMax; set => healthMax = value; }
    public int StaggerLeft { get => staggerLeft; set => staggerLeft = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
