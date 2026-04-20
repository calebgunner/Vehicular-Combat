using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public enum EnemyAttackSequence
{
    FollowPlayer,
    StopAndReadyToShoot,
    Shoot
}


public class _EnemyAttack : MonoBehaviour
{
    //add box coliner to start/end of line renderer to see if it hit the player and is on target

    [Header("enemy shooting controls")]
    public EnemyAttackSequence theEnemyAttackSequence;
    public BoxCollider lineBoxCollider;
    public Transform spawnPoint;
    public Rigidbody enemyBullet;

    [Space]
    public float followTime;
    public float shotDelayTime;
    public float shotTime;
    public float bulletSpeed;

    [Header("enemy aim")]
    public LineRenderer lineRenderer;
    Color color_OnTarget = Color.cyan;
    Color color_OnTargetReadyToShoot = Color.red;
    Color color_OffTarget = Color.yellow;

    [Header("other references")]
    public _EnemyMovement eM;
    public _LineCollider lC;
    Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(ControlTheAttackSequence()); //In START FUNCTION because START FUNCTION starst it once.... only needs to be started once in this case
    }


    void Update()
    {
        LineRendererActivation();


        Debug.Log("CURRENT ENEMY SEQUENCE IS = " + theEnemyAttackSequence);
    }


    // ====== CONTROLLING THE ATTACK SEQUENCE ======
    IEnumerator ControlTheAttackSequence()
    {
        while (eM.enemyIsTurret)
        {
            // MOVE
            theEnemyAttackSequence = EnemyAttackSequence.FollowPlayer;
            eM.canMove = true;
            yield return new WaitForSeconds(followTime);

            // STOP / PREP (LINE RENDERER ACTIVATION)
            theEnemyAttackSequence = EnemyAttackSequence.StopAndReadyToShoot;
            eM.canMove = false;
            yield return new WaitForSeconds(shotDelayTime);

            // SHOOT PHASE (THIS IS THE IMPORTANT FIX)
            theEnemyAttackSequence = EnemyAttackSequence.Shoot;
            eM.canMove = false;

            EnemyShoots(); // MUST happen here if you want consistency

            yield return new WaitForSeconds(shotTime);
        }
    }


    // ====== ACTIVATE THE LINE-RENDERER ======
    void LineRendererActivation() //ontar - blue, ontar & redy2shoot - red, offtarget - yellow
    {
        // ==== Line Renderer Colour ====
        if (lC.playerInCrosshair)
        {
            // colour change = visual indicator that the ENEMY is about to shoot
            if (theEnemyAttackSequence == EnemyAttackSequence.StopAndReadyToShoot)
                lineRenderer.material.color = color_OnTargetReadyToShoot;
            else
                lineRenderer.material.color = color_OnTarget;
        }
        else
        {
            lineRenderer.material.color = color_OffTarget;
        }


        // ==== Activate the line renderer ====
        if (theEnemyAttackSequence == EnemyAttackSequence.FollowPlayer || theEnemyAttackSequence == EnemyAttackSequence.StopAndReadyToShoot)
        {
            //Activate the line renderer
            lineRenderer.enabled = true;
            lineRenderer.useWorldSpace = true;

            // Position
            Vector3 start = spawnPoint.position;
            Vector3 end = spawnPoint.position + spawnPoint.forward * 1000f;
            Vector3 direction = end - start;

            // Line
            lineRenderer.startWidth = lineRenderer.endWidth = 0.4f;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // Collider
            lineBoxCollider.transform.position = (start + end) / 2f;
            lineBoxCollider.transform.rotation = Quaternion.LookRotation(direction);

            lineBoxCollider.size = new Vector3(lineBoxCollider.size.x, lineBoxCollider.size.y, direction.magnitude);

            lineBoxCollider.enabled = true;
        }

        else
        {
            lineRenderer.enabled = false;
            lineBoxCollider.enabled = false;
        }
            
    }


    // ====== ENEMY SHOOTS ======
    void EnemyShoots()
    {
        // Instantiate the projectile at the position and rotation of this transform
        Rigidbody clone;
        clone = Instantiate(enemyBullet, spawnPoint.position, spawnPoint.rotation);


        // Calculate direction
        Vector3 direction = spawnPoint.forward; //just  shoots forward in the spawn-point direction


        // Apply velocity to the projectile
        clone.linearVelocity = direction * bulletSpeed;
    }
}
