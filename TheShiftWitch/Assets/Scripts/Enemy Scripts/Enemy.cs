using UnityEngine;

public class Enemy : MonoBehaviour
{
    internal EnemyStats stats = new EnemyStats();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage(int damage) {
        Debug.Log("Enemy took " + damage + " damage!");
        stats.Health -= damage;
    }
}
