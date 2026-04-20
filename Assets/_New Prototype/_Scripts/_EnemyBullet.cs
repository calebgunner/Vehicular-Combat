using UnityEngine;

public class _EnemyBullet : MonoBehaviour
{
    public float damageToPlayer;

    // ==== OTHER REFERENCES ====
    _GameCanvas gC;

    void Start()
    {
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Reduce player health
            gC.playerHealthPoints -= damageToPlayer;

            // destroy bullet
            Destroy(this.gameObject);
        }
    }


}
