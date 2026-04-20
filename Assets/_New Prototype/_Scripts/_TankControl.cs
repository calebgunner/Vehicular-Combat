using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum TankMovement //USE IT OUTSIDE THE CLASSES TO MAKE THINGS A LOT EASIER
{
    // ==== MOVEMENT ====
    idle,
    movement,

    // ==== DODGE ====
    dodging
}


public enum ReticleControl
{
    onTarget,
    onTargetAndShoot,
    offTarget,
}

public class _TankControl : MonoBehaviour
{
    /// <summary>
    ///  -  ADD SOME LITTLE AIM ASSIST
    ///  -  ADD FEEDBACK 
    ///         I.E. PARTICLES WHEN BULLET HITS, 
    ///         ENEMY EXPLOSION WHEN DEAD, 
    ///         PARTICLE EFECT FROM MUZZLE, 
    ///         DODGING EFFECT, 
    ///         SMALL CHANGE IN COLOUR WHEN ENEMY HIT
    ///     
    /// </summary>

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 movementInput;
    
    [Header("normal movement")]
    public bool isMoving;
    public float speed;
    public TankMovement theTankMovement;

    [Header("dodging")]
    public bool isDodging;
    public bool canDodge = true;
    public float dodgeForce;
    public float dodgeDuration;
    public float canDodgeDelay;

    [Header("tire control/rotation")]
    public Transform[] tirePivot;

    [Header("shooting mechanics")]
    public Transform spawnPoint;
    public GameObject trailPrefab;
    public bool isOnTarget;
    public bool isHoldingShoot;
    public float bulletTrailDuration = 0.05f;

    [Space]
    public bool isShooting1;
    public float shootCooldown1; // Cooldown time between attacks
    bool canShoot1 = true;

    [Space]
    public LayerMask enemyLayer;
    public LayerMask otherLayer;
    RaycastHit hit;

    [Space]
    public ReticleControl theReticleControl;


    [Header("references")]
    _TankCamera tC;




    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tC = FindAnyObjectByType<_TankCamera>();
    }


    void Update()
    {
        if (isDodging) return;  //prevents a boolean from overriding this until dodging has completed

        // ANY MOVEMENT WILL TRIGGER THE BOOLEAN
        isMoving = movementInput.magnitude > 0.01f;


        #region OTHER FUNCTIONS:

        StateMachine();
        AimingDirections();
        ReticleControlFunction();
        TyrePivotRotation();

        #endregion
    }


    private void FixedUpdate()
    {
        Movement();
    }


    #region MOVEMENT TYPES:

    void Movement()
    {
        // MOVEMENT
        if (theTankMovement == TankMovement.movement)
        {
            Vector3 localMovement = (tC.cameraTarget.forward * movementInput.z + tC.cameraTarget.right * movementInput.x).normalized; //movement is relavent to the cameraTarget which controls movement direction
            localMovement.y = 0f; //cancel out any irregular moveemnt in he Y-Drection

            rb.linearVelocity = localMovement * speed + Vector3.up * rb.linearVelocity.y; // simple, instant
        }

        // NO MOVEMENT
        if (theTankMovement == TankMovement.idle)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }


    IEnumerator Dodge()
    {
        // ==== DODGING MECHANICS ====
        canDodge = false; 
        isDodging = true;

        // movement direction
        Vector3 localMovement = (tC.cameraTarget.forward * movementInput.z + tC.cameraTarget.right * movementInput.x).normalized;
        localMovement.y = 0f;

        rb.AddForce(localMovement * dodgeForce, ForceMode.Impulse); // ADD FORCE IN THAT DIRECTION

        theTankMovement = TankMovement.dodging; //change the movement to 'dodging'

        yield return new WaitForSeconds(dodgeDuration);

        canDodge = false; //prevents the player from dodging consecutively 
        isDodging = false;
        theTankMovement = TankMovement.idle; //return to default

        yield return new WaitForSeconds(canDodgeDelay);

        canDodge = true; //alows the player to dodge after the "canDodgeDelay" has passed
    }

    #endregion


    #region TYRE-PIVOT ROTATION:

    void TyrePivotRotation()
    {
        // MOVEMENT DIRECTION
        Vector3 localMovement = (transform.forward * movementInput.z + transform.right * movementInput.x).normalized;

        // Only rotate tyres if there is movement input
        if (localMovement != Vector3.zero)
        {
            // Get the Y angle from the movement direction
            float yAngle = Quaternion.LookRotation(localMovement).eulerAngles.y;

            // Create the target rotation
            Quaternion targetRotation = Quaternion.Euler(0f, yAngle, 0f);

            float tireRotationSpeed = 8;

            // Smoothly rotate all tyre pivots
            tirePivot[0].rotation = Quaternion.Slerp(tirePivot[0].rotation, targetRotation, tireRotationSpeed * Time.deltaTime);
            tirePivot[1].rotation = Quaternion.Slerp(tirePivot[1].rotation, targetRotation, tireRotationSpeed * Time.deltaTime);
            tirePivot[2].rotation = Quaternion.Slerp(tirePivot[2].rotation, targetRotation, tireRotationSpeed * Time.deltaTime);
            tirePivot[3].rotation = Quaternion.Slerp(tirePivot[3].rotation, targetRotation, tireRotationSpeed * Time.deltaTime);

            // SLERP: ROTATION = Quaternion.Slerp(currentRotation, targetRotation, speed);
        }
    }

    #endregion


    #region SHOOTING MECHANICS:


    #region // NOTE FOR SHOOTING MECHANICS //

    // 1. use "HITSCAN" for guns like i.e. machine guns, shot guns etc. since they are very fast and aren't able to be seen
    // Shoot FROM camera directly
    // a. these are quick and hit the target quickly without feeling "off" no matter the range

    // (EXAMPLE CODE USED IN THIS SCRIPT FOR PLAYER


    // 2. use the "INSTANTIATE PREFABS" for visible, slower attacks like i.e. rockets, slower energy blast
    // Shoot FROM gun TO where camera is aiming
    // a. these have an unwated offset at close range.

    //2.1 The code you have made

    /* void ShootingMechanics()
    {
        #region BULLET RELEASED:

        // Instantiate the projectile at the position and rotation of this transform
        Rigidbody clone;
        clone = Instantiate(projectile, spawnPoint.position, spawnPoint.rotation);

        #endregion

        #region AIMING / DIRECTION:

        // Calculate the direction from spawnPoint to the center of the screen
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // This block runs if the ray hits something
            targetPoint = hit.point;

        }
        else
        {
            // This block runs if the ray does NOT hit anything
            targetPoint = ray.GetPoint(2000);

            // ** MAKE AN INVISIBLE BORDER SO THAT THE RAY TECHNICALLY IS ALWAYS HITTING AN OBJECT THAT THE AIM ALWAYS WORKS **
        }

        // Calculate direction
        Vector3 direction = (targetPoint - spawnPoint.position).normalized;

        #endregion

        #region BULLET VELOCITY / COLLISION:

        // Apply velocity to the projectile
        clone.linearVelocity = direction * bulletSpeed;

        //Ignore Collision Between Player and Projectile [TO PREVENT THE UNCONTROLLED RECOIL]
        Physics.IgnoreCollision(clone.GetComponent<Collider>(), rb.GetComponent<Collider>());

        #endregion

    } */


    #endregion


    //THIS HELPS TO SEE WHETHER THE PLAYER IS ON TARGET
    void AimingDirections() 
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        float maxDistance = Mathf.Infinity;

        if (Physics.Raycast(ray, out hit, maxDistance, enemyLayer))
        {
            isOnTarget = true; //the ray hit something

        }
        else
        {
            isOnTarget = false; //the ray did not hit anything

            // ** MAKE AN INVISIBLE BORDER SO THAT THE RAY TECHNICALLY IS ALWAYS HITTING AN OBJECT THAT THE AIM ALWAYS WORKS **
        }
    }


    void ShootingMechanics() // Hitscan
    {
        #region AIMING / DIRECTION:

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); //creates ray from the center of our screen

        Vector3 targetPoint; //where the bullet should go

        #endregion

        #region HITSCAN LOGIC:

        if (Physics.Raycast(ray, out hit, 2000f)) //shoots invisible line forward
        {
            targetPoint = hit.point;

            if (hit.transform.CompareTag("Enemy"))
            {
                // Apply damage here
                hit.transform.GetComponent<_EnemyHealth>().EnemyTakesDamage();
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 2000f;

            Debug.DrawLine(ray.origin, targetPoint, Color.red, 0.2f);
        }

        #endregion

        #region SHOOTING TRAIL CODE:

        LineRenderer lr = Instantiate(trailPrefab).GetComponent<LineRenderer>(); //Gain access to the Line Render on the trailPrefab

        lr.SetPosition(0, spawnPoint.position); //start point
        lr.SetPosition(1, targetPoint); //end point

        Destroy(lr.gameObject, bulletTrailDuration); //how long the trail takes


        /*GameObject trail = Instantiate(trailPrefab, spawnPoint.position, Quaternion.identity); //spawns the trail
        StartCoroutine(MoveTrail(trail, targetPoint));*/

        #endregion
    }


    /* IEnumerator MoveTrail(GameObject trail, Vector3 targetPos) //moves the trail
    {
        Vector3 startPos = trail.transform.position;

        float time = 0f;

        while (time < bulletTrailDuration)
        {
            trail.transform.position = Vector3.Lerp(startPos, targetPos, time / bulletTrailDuration); //smoothly goes between pos 1 to pos 2
            time += Time.deltaTime;
            yield return null;
        }

        trail.transform.position = targetPos;

        Destroy(trail, 0.1f);
    }*/


    IEnumerator ShootingControl()
    {
        while (isHoldingShoot && canShoot1)
        {
            isShooting1 = true;
            canShoot1 = false;

            ShootingMechanics(); // Apply the shooting mechanics

            // Return to shoot-idle or appropriate state
            //theMovementType = MovementType.Normal_Shot;
            //movementText.text = "Movement : normal-shot";

            //Turn off effects
            //NormalShot_Particle.SetActive(true);

            // Wait for shoot animation or duration
            yield return new WaitForSeconds(0.17f);

            isShooting1 = false;

            //NormalShot_Particle.SetActive(false);

            // Wait for cooldown
            yield return new WaitForSeconds(shootCooldown1); //ZERO SINCE THE "SHOOTING ANIMATION" IS A LONG ENOUGH WAIT
            canShoot1 = true;
        }
    }

    #endregion


    #region RETICLE CONTROL:

    void ReticleControlFunction()
    {
        //Basic Control of the Game's Reticle/Crosshair
        if (isOnTarget) 
        {
            if (isShooting1 || isHoldingShoot) 
                theReticleControl = ReticleControl.onTargetAndShoot;
            else
                theReticleControl = ReticleControl.onTarget;
        }

        else
            theReticleControl = ReticleControl.offTarget;
    }

    #endregion


    #region STATE MACHINE:

    void StateMachine() //CONTROLLING THE STATE OF THE TANK
    {
        // ==== MOVEMENT ====
        if (isMoving)
            theTankMovement = TankMovement.movement;
        else
            theTankMovement = TankMovement.idle;
    }

    #endregion


    #region PLAYER INPUT:

    // MOVEMENT INPUT
    public void OnMove(InputAction.CallbackContext context) //THIS WILL BE FOR STEERING
    {
        if (context.started || context.performed) //When the left stick is moved
        {
            Vector2 stickInput = context.ReadValue<Vector2>();
            movementInput = new Vector3(stickInput.x, 0f, stickInput.y); //the MovementInput is equal to the MovementInput (ignoring Y-Value)
        }

        //stick is released
        else if (context.canceled) 
        {
            movementInput = Vector2.zero; //Reset values
        }
    }


    // DODGE INPUT
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && canDodge) //When the button is pressed
        {
            StartCoroutine(Dodge());
        }
    }


    // SHOOT
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) //When the button is pressed
        {
            isHoldingShoot = true;
            StartCoroutine(ShootingControl());
        }

        else if (context.canceled)
        {
            isHoldingShoot = false;
        }
    }



    #endregion
}
