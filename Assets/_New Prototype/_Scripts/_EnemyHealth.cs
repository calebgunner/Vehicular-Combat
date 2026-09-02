using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class _EnemyHealth : MonoBehaviour
{
    [Header("enemy health bar settings")]
    public Slider enemyHealthBar;
    public float startingEnemyHealth = 100;
    public float playerDamage;
    public GameObject parentObject; // This script is on EACH child object of the Enemy

    [Header("death effect")]
    public Transform explosionPosition;
    public GameObject explosionEffect;

    _CameraImpulseShake cIS;
    _ControllerRumble cR;


    private void Awake()
    {
        enemyHealthBar.value = startingEnemyHealth;


        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();
    }


    #region ENEMY TAKES DAMAGE:

    //This is called in the "TankControl" script when the ENEMY TAKES DAMAGE
    public void EnemyTakesDamage()
    {
        enemyHealthBar.value -= playerDamage;

        if (enemyHealthBar.value <= 0)
        {
            parentObject.SetActive(false);

            // SCREEN SHAKE for theexplosion
            cIS.ScreenShake(Vector3.up, 0.8f, 0.6f, CinemachineImpulseDefinition.ImpulseShapes.Explosion);

            //CONTROLLER VIBRATION WHEN THERE'S AN EXPLOSION
            cR.Rumble(0.8f, 0.4f, 0.5f);

            // Add the explosion effect
            GameObject spawnedInstance = Instantiate(explosionEffect, explosionPosition.position, Quaternion.identity);
        }
    }

    #endregion
}
