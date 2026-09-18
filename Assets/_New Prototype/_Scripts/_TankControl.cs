using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;


public enum TankMovement //USE IT OUTSIDE THE CLASSES TO MAKE THINGS A LOT EASIER
{
    // ==== MOVEMENT ====
    idle,
    movement,

    // ==== DODGE ====
    dodging
}

public enum TankShot
{
    // ==== SHOOTING ====
    shooting,
    notShooting
}

public class _TankControl : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 movementInput;

    [Header("tank enums")]
    public TankMovement theTankMovement;
    public TankShot theTankSot;
    
    [Header("normal movement")]
    public bool isMoving;
    public float speed;

    [Header("dodging")]
    public bool isDodging;
    public bool canDodge = true;
    public float dodgeForce;
    public float dodgeDuration;
    public float canDodgeDelay;
    public GameObject speedlines;

    [Header("tire control/rotation")]
    public Transform[] tirePivot;

    [Header("aim assist")]
    public float aimAssistRadius = 1f; //How wide the aim assist detection area is
    [Range(0f, 1f)] public float aimAssistStrength = 0.1f; //How strongly the shot is pulled towards the enemy

    [Header("ammo control and ui")]
    public float primaryAmmoAmnt;
    public float primaryAmmoMax;
    public bool activateSecondaryAttack;
    public float secondaryFireAmmo = 10;
    public float damageToEnemy_Primary;
    public float damageToEnemy_Secondary;

    [Header("shooting mechanics")]
    public GameObject hitMarker;
    public Transform spawnPoint;
    public GameObject trailPrefab;
    public bool isOnTarget;
    public bool isHoldingShoot;
    public float bulletTrailDuration = 0.05f;

    [Space]
    public bool isShooting1;
    public float shootCooldown1; // Cooldown time between attacks
    public bool canShoot1 = true;
    public bool reloading1;

    [Space]
    public LayerMask enemyLayer;
    public LayerMask otherLayer;
    RaycastHit hit;

    [Space]
    public ParticleSystem muzzleEffectPlayer;
    public ParticleSystem collisionEffect;

    [Space]
    public GameObject BoostRight;
    public GameObject BoostLeft;
    private float lastHorizontalDirection = 0f; // -1 for Left, 1 for Right, 0 for None


    [Header("references")]
    _TankCamera tC;
    _CameraImpulseShake cIS;
    _ControllerRumble cR;



    void Start()
    {
        activateSecondaryAttack = false; //until wave 5

        primaryAmmoMax = 40;
        primaryAmmoAmnt = primaryAmmoMax;

        rb = GetComponent<Rigidbody>();
        tC = FindAnyObjectByType<_TankCamera>(); ;
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();
    }


    void Update()
    {
        // PLAYER CAN SHOOT
        if (primaryAmmoAmnt <= 0) reloading1 = true;

        if (primaryAmmoAmnt > 0 && !reloading1) canShoot1 = true;
        else canShoot1 = false;

        // DODGE / BOOST EFFECTS
        if (isDodging)
        {
            if (lastHorizontalDirection < 0f) // It's -1 (Left)
            {
                BoostRight.SetActive(true);
                BoostLeft.SetActive(false);
            }
            else if (lastHorizontalDirection > 0f) // It's 1 (Right)
            {
                BoostLeft.SetActive(true);
                BoostRight.SetActive(false);
            }
        }
        else
        {
            // Turn everything off when not dodging, and reset the memory
            BoostLeft.SetActive(false);
            BoostRight.SetActive(false);
            lastHorizontalDirection = 0f;
        }
        
        if (isDodging) return;  //prevents a boolean from overriding this until dodging has completed


        // ANY MOVEMENT WILL TRIGGER THE BOOLEAN
        isMoving = movementInput.magnitude > 0.01f;

        #region OTHER FUNCTIONS:

        StateMachine_Movement();
        StateMachine__Shooting();
        AimingDirections();
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

        speedlines.SetActive(true); //speedlines are ONLY active when player is dodging

        // movement direction
        Vector3 localMovement = (tC.cameraTarget.forward * movementInput.z + tC.cameraTarget.right * movementInput.x).normalized;
        localMovement.y = 0f;

        rb.AddForce(localMovement * dodgeForce, ForceMode.Impulse); // ADD FORCE IN THAT DIRECTION

        theTankMovement = TankMovement.dodging; //change the movement to 'dodging'

        yield return new WaitForSeconds(dodgeDuration);

        speedlines.SetActive(false); //speedlines are ONLY active when player is dodging

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

    Ray AimAssist(Ray ray)
    {
        //IF THE PLAYER IS ALREADY AIMING DIRECTLY AT AN ENEMY, DON'T CHANGE THEIR AIM
        if (Physics.Raycast(ray, out RaycastHit directHit, 2000f, enemyLayer))
        {
            return ray;
        }

        //LOOK FOR AN ENEMY SLIGHTLY AROUND THE PLAYER'S NORMAL AIMING RAY
        if (Physics.SphereCast(ray, aimAssistRadius, out RaycastHit assistHit, 2000f, enemyLayer))
        {
            //GET THE DIRECTION TOWARDS THE POINT ON THE ENEMY CLOSEST TO WHERE THE PLAYER WAS AIMING
            Vector3 enemyDirection = (assistHit.point - ray.origin).normalized;

            //SLIGHTLY SWAY THE PLAYER'S NORMAL AIM TOWARDS THAT POINT
            Vector3 assistedDirection = Vector3.Slerp(ray.direction, enemyDirection, aimAssistStrength);

            //RETURN THE ASSISTED RAY
            return new Ray(ray.origin, assistedDirection);
        }

        //NO ENEMY FOUND, USE THE PLAYER'S NORMAL AIM
        return ray;
    }

    #region primary shooting:
    void ShootingMechanics1() // Hitscan
    {
        #region AIMING / DIRECTION:

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); //creates ray from the center of our screen

        ray = AimAssist(ray); //slightly correct the shot if an enemy is close to the player's aim

        Vector3 targetPoint; //where the bullet should go

        #endregion

        #region HITSCAN LOGIC:

        if (Physics.Raycast(ray, out hit, 2000f)) //shoots invisible line forward
        {
            targetPoint = hit.point;

            if (hit.transform.CompareTag("Enemy"))
            {
                // Apply damage here
                hit.transform.GetComponent<_EnemyHealth>().EnemyTakesDamage(damageToEnemy_Primary);

                // Show Hit indicator
                StartCoroutine(HideHitMarker());

                // Instantiate the particle system at HITPOINT (rotate it to face the player at 180d)
                ParticleSystem spawnedEffect = Instantiate(collisionEffect, hit.point, tC.gunHeadHorizontal.transform.rotation * Quaternion.Euler(0, 180, 0));
                // Play the effect
                spawnedEffect.Play();
            }
            else if (hit.transform.CompareTag("World"))
            {
                // Instantiate the particle system at HITPOINT (rotate it to face the player at 180d)
                ParticleSystem spawnedEffect = Instantiate(collisionEffect, hit.point, tC.gunHeadHorizontal.transform.rotation * Quaternion.Euler(0, 180, 0));
                // Play the effect
                spawnedEffect.Play();
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 2000f;

            Debug.DrawLine(ray.origin, targetPoint, Color.red, 0.2f);
        }

        //RECOIL THAT KNOWS THE DIRECTION WHERE THE GUN WAS SHOT and the strngth of the camera shake
        cIS.ScreenShake(ray.direction, 0.3f, 0.2f, CinemachineImpulseDefinition.ImpulseShapes.Recoil);

        // CONTROLLER RUMBLE (VIBRATION) WHEN SHOOTING
        cR.Rumble(0.1f, 0.2f, 0.1f);

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

    IEnumerator ShootingControl1()
    {
        while (isHoldingShoot && canShoot1)
        {
            isShooting1 = true;

            ShootingMechanics1(); // Apply the shooting mechanics

            muzzleEffectPlayer.Play(); //Play the muzzle effect

            // SUbtract Bullet
            primaryAmmoAmnt--;

            // Wait for shoot animation or duration
            yield return new WaitForSeconds(0.15f);

            isShooting1 = false;
            muzzleEffectPlayer.Stop();

            // Wait for cooldown
            yield return new WaitForSeconds(shootCooldown1); //ZERO SINCE THE "SHOOTING ANIMATION" IS A LONG ENOUGH WAIT
        }
    }
    #endregion


    #region secondary shooting:
    void ShootingMechanics2() // Hitscan
    {
        #region AIMING / DIRECTION:

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); //creates ray from the center of our screen

        ray = AimAssist(ray); //slightly correct the shot if an enemy is close to the player's aim

        Vector3 targetPoint; //where the bullet should go

        #endregion

        #region HITSCAN LOGIC:

        if (Physics.Raycast(ray, out hit, 2000f)) //shoots invisible line forward
        {
            targetPoint = hit.point;

            if (hit.transform.CompareTag("Enemy"))
            {
                // Apply damage here
                hit.transform.GetComponent<_EnemyHealth>().EnemyTakesDamage(damageToEnemy_Secondary);

                // Show Hit indicator
                StartCoroutine(HideHitMarker());

                // Instantiate the particle system at HITPOINT (rotate it to face the player at 180d)
                ParticleSystem spawnedEffect = Instantiate(collisionEffect, hit.point, tC.gunHeadHorizontal.transform.rotation * Quaternion.Euler(0, 180, 0));
                // Play the effect
                spawnedEffect.Play();
            }
            else if (hit.transform.CompareTag("World"))
            {
                // Instantiate the particle system at HITPOINT (rotate it to face the player at 180d)
                ParticleSystem spawnedEffect = Instantiate(collisionEffect, hit.point, tC.gunHeadHorizontal.transform.rotation * Quaternion.Euler(0, 180, 0));
                // Play the effect
                spawnedEffect.Play();
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 2000f;

            Debug.DrawLine(ray.origin, targetPoint, Color.red, 0.2f);
        }

        //STRONGER RECOIL FOR THE POWER SHOT
        cIS.ScreenShake(ray.direction, 1f, 0.6f, CinemachineImpulseDefinition.ImpulseShapes.Recoil);

        //STRONGER CONTROLLER RUMBLE FOR THE POWER SHOT
        cR.Rumble(0.6f, 0.8f, 0.3f);

        #endregion

        #region SHOOTING TRAIL CODE:

        //FIRST TRAIL
        LineRenderer lr1 = Instantiate(trailPrefab).GetComponent<LineRenderer>();

        lr1.SetPosition(0, spawnPoint.position + Camera.main.transform.right * 0.5f);
        lr1.SetPosition(1, targetPoint + Camera.main.transform.right * 3f);

        Destroy(lr1.gameObject, bulletTrailDuration);


        //SECOND TRAIL
        LineRenderer lr2 = Instantiate(trailPrefab).GetComponent<LineRenderer>();

        lr2.SetPosition(0, spawnPoint.position - Camera.main.transform.right * 0.5f);
        lr2.SetPosition(1, targetPoint - Camera.main.transform.right * 3f);

        Destroy(lr2.gameObject, bulletTrailDuration);


        /*GameObject trail = Instantiate(trailPrefab, spawnPoint.position, Quaternion.identity); //spawns the trail
        StartCoroutine(MoveTrail(trail, targetPoint));*/

        #endregion
    }

    IEnumerator ShootingControl2()
    {
        while (isHoldingShoot && canShoot1)
        {
            isShooting1 = true;

            ShootingMechanics2(); // Apply the shooting mechanics

            muzzleEffectPlayer.Play(); //Play the muzzle effect

            // SUbtract Bullet
            primaryAmmoAmnt -= secondaryFireAmmo;

            // Wait for shoot animation or duration
            yield return new WaitForSeconds(0.15f);

            isShooting1 = false;
            muzzleEffectPlayer.Stop();

            // Wait for cooldown
            yield return new WaitForSeconds(0.75f); //ZERO SINCE THE "SHOOTING ANIMATION" IS A LONG ENOUGH WAIT
        }
    }
    #endregion


    IEnumerator HideHitMarker()
    {
        hitMarker.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        hitMarker.SetActive(false);
    }

    #endregion


    #region STATE MACHINES:

    void StateMachine_Movement() //CONTROLLING THE MOVEMENT OF THE TANK
    {
        // ==== MOVEMENT ====
        if (isMoving)
            theTankMovement = TankMovement.movement;
        else
            theTankMovement = TankMovement.idle;
    }

    void StateMachine__Shooting()
    {
        // ==== SHOOTING ====
        if (isShooting1)
            theTankSot = TankShot.shooting;
        else
            theTankSot= TankShot.notShooting;
    }

    #endregion


    #region PLAYER INPUT:

    // MOVEMENT INPUT
    public void OnMove(InputAction.CallbackContext context) //THIS WILL BE FOR STEERING
    {
        if (context.started || context.performed)
        {
            Vector2 stickInput = context.ReadValue<Vector2>();
            movementInput = new Vector3(stickInput.x, 0f, stickInput.y);

            // Capture and REMEMBER the last direction pushed
            if (stickInput.x > 0.1f) lastHorizontalDirection = 1f;  // Right
            if (stickInput.x < -0.1f) lastHorizontalDirection = -1f; // Left
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
            StartCoroutine(ShootingControl1());
        }

        else if (context.canceled)
        {
            isHoldingShoot = false;
        }
    }


    // SECONDARY FIRE
    public void SecondaryFire(InputAction.CallbackContext context)
    {
        if (context.performed && canShoot1 && primaryAmmoAmnt >= secondaryFireAmmo && activateSecondaryAttack) //When the button is pressed
        {
            isHoldingShoot = true;
            StartCoroutine(ShootingControl2());
        }

        else if (context.canceled)
        {
            isHoldingShoot = false;
        }
    }

    #endregion
}
