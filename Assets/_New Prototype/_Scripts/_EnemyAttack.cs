using System.Collections;
using Unity.Cinemachine;
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
    public float bulletSpeed;

    [Space]
    public ParticleSystem muzzleEffectEnemy;

    [Header("enemy bullet damage")]
    float damageToPlayer;
    public ParticleSystem collisionEffect;
    public LineRenderer trailPrefab;
    public float bulletTrailDuration = 0.15f;

    [Header("enemy aim")]
    public LineRenderer lineRenderer;
    Color color_OnTarget = Color.cyan;
    Color color_OnTargetReadyToShoot = Color.red;
    Color color_OffTarget = Color.yellow;

    [Header("other references")]
    public _EnemyMovement eM;
    public _LineCollider lC;
    _GameCanvas gC;
    _CameraImpulseShake cIS;
    _ControllerRumble cR;
    Transform playerTransform;


    void Awake()
    {
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void OnEnable()
    {
        // RANDOMISE ATTACK TIMES FOR THE CURRENT WAVE
        AttackeSequenceTimes();

        // RESTART THE ENEMY'S ATTACK SEQUENCE
        StartCoroutine(ControlTheAttackSequence());

        //SELECT THE DAMAGE TO THE PLAYER ACCORDING TO THE WAVE
        DamageToPlayer();
    }

    void OnDisable()
    {
        // STOP THE ATTACK SEQUENCE WHEN THE ENEMY IS DISABLED
        StopAllCoroutines();

        // RESET THE ATTACK STATE
        theEnemyAttackSequence = EnemyAttackSequence.FollowPlayer;

        // STOP THE MUZZLE EFFECT
        muzzleEffectEnemy.Stop();

        // DISABLE THE AIMING LINE AND COLLIDER
        lineRenderer.enabled = false;
        lineBoxCollider.enabled = false;
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
            muzzleEffectEnemy.Stop();
            yield return new WaitForSeconds(followTime);

            // STOP / PREP (LINE RENDERER ACTIVATION)
            theEnemyAttackSequence = EnemyAttackSequence.StopAndReadyToShoot;
            eM.canMove = false;
            yield return new WaitForSeconds(shotDelayTime);

            // SHOOT PHASE (THIS IS THE IMPORTANT FIX)
            theEnemyAttackSequence = EnemyAttackSequence.Shoot;
            eM.canMove = false;

            // DISABLE THE AIMING COLLIDER BEFORE THE RAYCAST
            lineBoxCollider.enabled = false;

            muzzleEffectEnemy.Play();
            EnemyShoots();

            yield return new WaitForSeconds(shotTime);
        }
    }


    // Attacke sequence management
    void AttackeSequenceTimes()
    {
        // PICK RANDOM VALUES DEPENDING ON THE WAVE TO PREVENT ENEMIES FROM HAVING A UNIFIED ATTACK SEQUENCE (USE ARRAYS TO KEEP TRACK OF ALL OF THE DIFF NUMBERS)
        float[] minFollowTimes = { 2.7f, 2.7f, 2.7f, 2.7f, 2.7f, 2.7f, 2.7f, 2.5f, 2.2f, 2f, 1.7f };
        float[] maxFollowTimes = { 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.5f, 4.2f, 4f, 3.7f, 3.5f };

        float[] minShotDelayTimes = { 1.4f, 1.4f, 1.4f, 1.4f, 1.4f, 1.4f, 1.4f, 1.3f, 1.3f, 1.2f, 1.2f };
        float[] maxShotDelayTimes = { 2.2f, 2.2f, 2.2f, 2.2f, 2.2f, 2.2f, 2.2f, 2.1f, 2.1f, 2f, 2f };

        //RANDOMISE THE VALUES BASED ON THE WAVE AND ARRAY NUMBER i.e. Wave 0 uses the values associated with array number 0.
        followTime = Random.Range(minFollowTimes[gC.waveNumber], maxFollowTimes[gC.waveNumber]);
        shotDelayTime = Random.Range(minShotDelayTimes[gC.waveNumber], maxShotDelayTimes[gC.waveNumber]);
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
        Debug.Log("ENEMY SHOOTS CALLED");

        // CREATE A RAY FROM THE TURRET'S SPAWN POINT
        Ray ray = new Ray(spawnPoint.position, spawnPoint.forward);

        // DEFAULT END POINT IF THE RAY DOES NOT HIT ANYTHING
        Vector3 targetPoint = ray.GetPoint(1000f);

        // CHECK WHETHER THE RAY HITS SOMETHING
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log("ENEMY RAY HIT: " + hit.collider.name);

            // USE THE ACTUAL HIT POINT FOR THE BULLET TRAIL
            targetPoint = hit.point;

            // CHECK WHETHER THE RAY HIT THE PLAYER OR ONE OF ITS CHILD OBJECTS
            if (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))
            {
                // GET THE EXACT POINT WHERE THE RAY HIT THE PLAYER
                Vector3 hitPoint = hit.point;

                // ROTATE THE IMPACT EFFECT TO FACE AWAY FROM THE SURFACE
                Quaternion oppositeRotation = Quaternion.LookRotation(hit.normal);

                // SPAWN AND PLAY THE EFFECT
                ParticleSystem effect = Instantiate(collisionEffect, hitPoint, oppositeRotation);
                effect.Play();

                // REDUCE PLAYER HEALTH
                gC.StartCoroutine(gC.ReduceHealth(damageToPlayer));

                // CAMERA MOVEMENT TO SHOW THAT IT HIT THE PLAYER
                cIS.ScreenShake(Vector3.right, 0.5f, 0.25f, CinemachineImpulseDefinition.ImpulseShapes.Bump);

                // CONTROLLER RUMBLE (VIBRATION) WHEN PLAYER IS HIT
                cR.Rumble(0.5f, 0.6f, 0.2f);
            }

            Debug.DrawRay(spawnPoint.position, spawnPoint.forward * 1000f, Color.red, 3f);
        }

        // CREATE THE ENEMY'S BULLET TRAIL
        LineRenderer lr = Instantiate(trailPrefab).GetComponent<LineRenderer>();

        // START THE TRAIL AT THE ENEMY'S MUZZLE
        lr.SetPosition(0, spawnPoint.position);

        // END THE TRAIL AT THE RAYCAST HIT POINT, OR MAXIMUM DISTANCE
        lr.SetPosition(1, targetPoint);

        // REMOVE THE VISUAL TRAIL AFTER A SHORT DURATION
        Destroy(lr.gameObject, bulletTrailDuration);
    }


    void DamageToPlayer()
    {
        // SET THE DAMAGE TO THE PLAYER ACCORDING TO THE WAVE
        if (gC.waveNumber == 0 || gC.waveNumber == 1 || gC.waveNumber == 2 || gC.waveNumber == 3 || gC.waveNumber == 4)
            damageToPlayer = 100/11;

        else if (gC.waveNumber == 5 || gC.waveNumber == 6)
            damageToPlayer = 100/10;

        else if (gC.waveNumber == 7 || gC.waveNumber == 8)
            damageToPlayer = 100/9;

        else if (gC.waveNumber == 9)
            damageToPlayer = 100/8;

        else if (gC.waveNumber == 10)
            damageToPlayer = 100/6;
    }
}
