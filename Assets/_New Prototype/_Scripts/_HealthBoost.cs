using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class _HealthBoost : MonoBehaviour
{
    public GameObject healthBoostEffect;

    _GameCanvas gC;

    void Start()
    {
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Spawn it as a GameObject
            GameObject effectInstance = Instantiate(healthBoostEffect, transform.position, transform.rotation);

            // Get the ParticleSystem component from it if you need to play/stop it
            ParticleSystem effect = effectInstance.GetComponent<ParticleSystem>();

            // Restore player health
            RestorePlayerHealth();

            // Make the player healthbar green for a short period
            gC.boostEffectIsActive = true;

            // switch off the object
            this.gameObject.SetActive(false);
        }
    }


    void RestorePlayerHealth()
    {
        gC.playerHealthPoints = 100;
        gC.damageBarPoints = 100;
    }
}
