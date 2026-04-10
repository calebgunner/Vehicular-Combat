using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class _Enemy : MonoBehaviour
{
    [Header("enemy health bar settings")]
    public Slider enemyHealthBar;
    public float startingEnemyHealth = 100;
    public float playerDamage;

    [Header("turret head movement")]
    public Transform headPivot; // Change to "head" when you import the final product
    public bool enemyIsTurret = false;
    Transform player;


    private void Awake()
    {
        enemyHealthBar.value = startingEnemyHealth;
        

        if (headPivot != null) //we only check for the player target if there is a HEADPIVOT TRANSFORM
        {
            player = GameObject.FindWithTag("Player").transform;

            enemyIsTurret = true; //this means that this "ENEMY" is a TURRET
        }
    }


    private void Update()
    {
        if (enemyIsTurret)
        {
            HeadMovement();
        }
        
    }


    #region ENEMY TAKES DAMAGE:

    //This is called in the "TankControl" script when the ENEMY TAKES DAMAGE
    public void EnemyTakesDamage() 
    {
        enemyHealthBar.value -= playerDamage;

        if (enemyHealthBar.value <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    #endregion


    #region HEAD FOLLOWS PLAYER:

    void HeadMovement()
    {
        // 1. Get the target's current position
        Vector3 playerPos = player.position;

        // 2. Lock the target's Y to our current Y position
        playerPos.y = headPivot.position.y;

        // 3. Look at the modified position
        headPivot.LookAt(playerPos);
    }

    #endregion
}
