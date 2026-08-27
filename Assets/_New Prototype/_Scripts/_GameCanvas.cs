using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class _GameCanvas : MonoBehaviour
{
    [Header("reticle control")]
    public Image reticleImage;
    public Sprite[] differentReticles;

    [Header("player health")]
    public Slider playerHealthBar;
    [Range(0f, 100f)] public float playerHealthPoints;

    [Header("death effect")]
    public Transform explosionPosition;
    public GameObject explosionEffect;

    [Header("dodge indicator")]
    public GameObject dodgeIndicator;

    [Space]
    _TankControl tControl;
    _CameraImpulseShake cIS;


    private void Awake()
    {
        //SET THE FPS
        Application.targetFrameRate = 60;

        //REFERENCE OBJECTS
        tControl = GameObject.FindWithTag("Player").GetComponent<_TankControl>();
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();

        //SET SLIDER VALUE
        playerHealthPoints = 100f;
        playerHealthBar.value = playerHealthPoints;
    }


    void Update()
    {
        //CONTROL PLAYER HEALTH
        playerHealthBar.value = playerHealthPoints;

        //CONTROL DODGE INDICATOR
        dodgeIndicator.SetActive(tControl.canDodge);

        PlayerDies();
    }

    void PlayerDies()
    {
        if (playerHealthBar.value <= 0)
        {
            // Remove the game object
            GameObject.FindWithTag("Player").SetActive(false);

            // SCREEN SHAKE for theexplosion
            cIS.ScreenShake(Vector3.up, 0.8f, 1f, CinemachineImpulseDefinition.ImpulseShapes.Explosion);

            // Add the explosion effect
            GameObject spawnedInstance = Instantiate(explosionEffect, explosionPosition.position, Quaternion.identity);
        }
    }
}
