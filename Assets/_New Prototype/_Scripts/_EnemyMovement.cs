using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class _EnemyMovement : MonoBehaviour
{
    // *** This is placed on the PARENT OBJECT OF THE ENEMY ***

    [Header("turret head movement")]
    public Transform headPivot; // Change to "head" when you import the final product
    public bool enemyIsTurret = false;
    public bool canMove;
    Transform player;


    private void Awake()
    {
        //canMove = true;

        if (headPivot != null) //we only check for the player target if there is a HEADPIVOT TRANSFORM
        {
            player = GameObject.FindWithTag("Player").transform;

            enemyIsTurret = true; //this means that this "ENEMY" is a TURRET
        }
    }


    private void Update()
    {
        if (enemyIsTurret && canMove)
        {
            HeadMovement();
        }
        
    }


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
