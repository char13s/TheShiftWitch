using UnityEngine;
using UnityEngine.Playables;

public class BossDefeatedEvent : Observer
{
    [SerializeField] private PlayableDirector eventHolder;
    [SerializeField] private Enemy boss;
    bool done = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        done = false;
    }
    public override void Condition()
    {
        if (boss.stats.Health <= 0&&!done)
        {
            eventHolder.Play();
            done = true;
        }
    }
}
