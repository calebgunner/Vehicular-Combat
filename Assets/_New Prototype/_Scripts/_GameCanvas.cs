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


    private void Awake()
    {
        //REFERENCE OBJECTS
        tControl = GameObject.FindWithTag("Player").GetComponent<_TankControl>();

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

        ReticleImageControl();
        PlayerDies();
    }


    void ReticleImageControl()
    {
        //Control the RETICLE APPEARENCE

        if (tControl.theReticleControl == ReticleControl.onTarget)
            reticleImage.sprite = differentReticles[0];

        else if (tControl.theReticleControl == ReticleControl.onTargetAndShoot)
            reticleImage.sprite = differentReticles[1];

        else
            reticleImage.sprite = differentReticles[2];
    }

    void PlayerDies()
    {
        if (playerHealthBar.value <= 0)
        {
            // Remove the game object
            GameObject.FindWithTag("Player").SetActive(false);

            // Add the explosion effect
            GameObject spawnedInstance = Instantiate(explosionEffect, explosionPosition.position, Quaternion.identity);
        }
    }
}
