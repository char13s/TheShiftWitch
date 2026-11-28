using UnityEngine;

public class PlayerSpells : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform handPoint;
    [SerializeField] private Transform swordPoint;
    [SerializeField] private Transform distantTarget;
    [SerializeField] private GameObject lightningStrike;
    [SerializeField] private GameObject fireball;
    [SerializeField] private GameObject LightningStorm;
    [SerializeField] private GameObject gravityPush;
    [SerializeField] private GameObject gravitySlash;
    [SerializeField] private GameObject gravityPull;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
    }
    public void CastLightningStrike() {
        Instantiate(lightningStrike,transform.position,Quaternion.identity);
    }
    public void CastLightningStorm() {
        Instantiate(LightningStorm,distantTarget.position,Quaternion.identity);
    }
    public void CastGravityPush() {
        Instantiate(gravityPush,handPoint.position,Quaternion.identity);
    }
    public void CastGravitySlash() {
        Instantiate(gravitySlash,swordPoint.position,Quaternion.identity);
    }
    public void CastGravityPull() {
        Instantiate(gravityPull,handPoint.position,Quaternion.identity);
    }

    public void CastFireball() {
        fireball.SetActive(true);
    }
    }
