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

    [Space]
    public float followTime;
    public float shotDelayTime;
    public float shotTime;

    [Header("enemy aim")]
    public LineRenderer lineRenderer;
    public Color onTargetColor = Color.red;
    public Color offTargetColor = Color.yellow;

    [Header("other references")]
    public _EnemyMovement eM;
    public _LineCollider lC;


    void Start()
    {
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

            //Shoot(); // MUST happen here if you want consistency

            yield return new WaitForSeconds(shotTime);
        }
    }


    // ====== ACTIVATE THE LINE-RENDERER ======
    void LineRendererActivation()
    {
        // ==== Line Renderer Colour ====
        if (lC.playerInCrosshair)
        {
            lineRenderer.material.color = onTargetColor;
        }
        else
            lineRenderer.material.color = offTargetColor;


        // ==== Activate the linerendere ====
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
}
