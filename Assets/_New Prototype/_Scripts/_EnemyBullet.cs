using Unity.Cinemachine;
using UnityEngine;

public class _EnemyBullet : MonoBehaviour
{
    public float damageToPlayer;
    public ParticleSystem collisionEffect;

    // ==== OTHER REFERENCES ====
    _GameCanvas gC;
    _CameraImpulseShake cIS;
    _ControllerRumble cR;

    void Start()
    {
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();
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

            // CAMERA movement to show that it hit the player
            cIS.ScreenShake(Vector3.right, 0.4f, 0.25f, CinemachineImpulseDefinition.ImpulseShapes.Bump);

            //CONTROLLER RUMBLE (VIBRATION) when player is hit
            cR.Rumble(0.4f, 0.6f, 0.2f);

            // destroy bullet
            Destroy(this.gameObject);
        }
    }


}
