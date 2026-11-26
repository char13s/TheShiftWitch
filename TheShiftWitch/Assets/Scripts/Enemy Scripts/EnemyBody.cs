using System.Collections;
using UnityEngine;

public class EnemyBody : MonoBehaviour
{
    private Enemy enemy;
    private MeshRenderer body;
    public Enemy Enemy { get => enemy; set => enemy = value; }
    private void Start() {
        enemy = GetComponentInParent<Enemy>();
        body = enemy.GetComponent<MeshRenderer>();
    }
    public void SetBodyColor(Color color) {
        //StartCoroutine(ChangeColorTemporarily(color, 0.2f));
        Enemy.TakeDamage(50);
        //if(!enemy.Dead)
            enemy.Anim.Play("HitReaction");
    }
    IEnumerator ChangeColorTemporarily(Color color, float duration) {
        Color originalColor = body.material.color;
        body.material.color = color;
        yield return new WaitForSeconds(duration);
        body.material.color = originalColor;
    }
}
