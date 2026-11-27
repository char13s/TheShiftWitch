using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
public class CheckEnemyCount : Observer
{
    public static event UnityAction<int> sendOrbs;
    int orbWorth = 50;
    [SerializeField] private PlayableDirector eventHolder;
    bool done;
    bool waitToRead;
    private void Start() {
        done = false;
        waitToRead = true;
        StartCoroutine(waitABitOfLoadingToRead());
    }
    public override void Condition() {

        if (PlayerLockOn.Enemies.Count <= 0 && !done && !waitToRead) {
            eventHolder.Play();
            done = true;
        }
    }
    IEnumerator waitABitOfLoadingToRead() {
        YieldInstruction wait = new WaitForSeconds(2);
        yield return wait;
        waitToRead = false;
    }
}
