using UnityEngine;

public class _EnemyBullet : MonoBehaviour
{
    public float damageToPlayer;
    public ParticleSystem collisionEffect;

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
            // Find the exact point on this trigger zone closest to the entering object
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            // Calculate a 180-degree rotation relative to the entering object
            Quaternion oppositeRotation = transform.rotation * Quaternion.Euler(0, 180, 0);

            // Spawn and play the effect
            ParticleSystem effect = Instantiate(collisionEffect, hitPoint, oppositeRotation);
            effect.Play();

            // Reduce player health
            gC.playerHealthPoints -= damageToPlayer;

            // destroy bullet
            Destroy(this.gameObject);
        }
    }


}
