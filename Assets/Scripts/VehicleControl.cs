using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.UI.ScrollRect;

 public enum VehicleMovement //USE IT OUTSIDE THE CLASSES TO MAKE THINGS A LOT EASIER
{
    // ==== NORMAL MOVEMENT ====
    idle,
    accelerate,
    reverse,
    revving,

    // ==== COMBAT MOVEMENT ====
    combatIdle,
    combatMovement
}

public class VehicleControl : MonoBehaviour
{
    #region INSPECTOR VALUES:

    [Header("activated mode")]
    public bool combatModeActivated;
    public bool isMoving;
    public bool isAccelarating;
    public bool isReversing;

    [Header("movement speed")]
    public float stationarySpeed;
    public float reversingSpeed;
    public float accelaratingSpeed;
    public float combatSpeed;
    float speed;

    [Space]
    bool hasDriveInput;
    Vector3 moveVelocity;

    [Header("vehicle rotation")]
    public bool rotateLeft;
    public bool rotateRight;
    public float rotateSpeed;
    float horizontalInputValue;

    Vector3 moveDirection;

    [Space]
    public GameObject GunObject;

    [Space]
    public VehicleMovement theVehicleMovement;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 movementInput;

    [Header("shooting mechanics")]
    public Transform spawnPoint;
    public Rigidbody projectile;
    public float bulletSpeed;
    public bool isOnTarget;
    public bool isHoldingShoot;

    [Space]
    public bool isShooting1;
    float shootCooldown1; // Cooldown time between attacks
    bool canShoot1 = true;

    [Space]
    public LayerMask enemyLayer;
    public LayerMask otherLayer;

    #endregion


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        //
        //if (isOnTarget) ShootingMechanics();

        isMoving = (isAccelarating || isReversing) || ((movementInput.magnitude > 0.01f) && combatModeActivated); //write what this means

        StateMachine();
        SpeedControl();
        Steering();
        CheckForTarget();

        // Activate the Gun IF COMBAT MODE IS ACTIVATED
        GunObject.SetActive(combatModeActivated);

    }


    private void FixedUpdate()
    {
        Movement();
    }


    #region MOVEMENT AND SPEED:

    void Movement()
    {
        // MOVEMENT AND DIRECTION (accelerate or reverse)
        if (theVehicleMovement == VehicleMovement.accelerate)
        {
            Vector3 forwardVelocity = transform.forward * speed;
            rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);
        }
        else if (theVehicleMovement == VehicleMovement.reverse)
        {
            Vector3 backwardVelocity = -transform.forward * speed;
            rb.linearVelocity = new Vector3(backwardVelocity.x,rb.linearVelocity.y, backwardVelocity.z);

        }


        // COMBAT MOVEMENT
        if (theVehicleMovement == VehicleMovement.combatMovement) //
        {
            Vector3 localMovement = transform.forward * movementInput.z + transform.right * movementInput.x;
            rb.linearVelocity = localMovement * speed + Vector3.up * rb.linearVelocity.y;

        }


        // NO MOVEMENT
        if (theVehicleMovement == VehicleMovement.idle || theVehicleMovement == VehicleMovement.combatIdle || theVehicleMovement == VehicleMovement.revving)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }


    void SpeedControl()
    {
        switch (theVehicleMovement)
        {
            case VehicleMovement.idle:
            case VehicleMovement.combatIdle:
            case VehicleMovement.revving:
                speed = stationarySpeed;
                break;

            case VehicleMovement.combatMovement:
                speed = combatSpeed;
                break;

            case VehicleMovement.reverse:
                speed = reversingSpeed;
                break;

            case VehicleMovement.accelerate:
                speed = accelaratingSpeed;
                break;
        }
    }

    #endregion


    #region STEERING (rotating the car):

    void Steering()
    {
        // ==== REGULAR STERRING ROTATION ====
        if (theVehicleMovement == VehicleMovement.accelerate || theVehicleMovement == VehicleMovement.revving)
        {
            rotateLeft = horizontalInputValue < -0.1f; // STICK MOVING LEFT
            rotateRight = horizontalInputValue > 0.1f; // STICK MOVING RIGHT
        }

        // Invert the rotation when reversing
        else if (theVehicleMovement == VehicleMovement.reverse)
        {
            rotateLeft = horizontalInputValue > 0.1f; // STICK MOVING RIGHT
            rotateRight = horizontalInputValue < -0.1f; // STICK MOVING LEFT
        }


        // ==== ROTATE THE VEHICLE ACCORDING THE THE STICK DIRECTION ====
        if (rotateLeft)
            transform.Rotate(0, -rotateSpeed, 0, Space.World);
        else if (rotateRight) // same for the right
            transform.Rotate(0, rotateSpeed, 0, Space.World);


        // ==== DISABLE ROTATIONS IF VEHICLE IS IDLE ====
        if (theVehicleMovement == VehicleMovement.idle || theVehicleMovement == VehicleMovement.combatIdle)
        {
            rotateLeft = rotateRight = false;
        }
    }

    #endregion


    #region COMBAT MECHANICS:

    void CheckForTarget()
    {
        if (combatModeActivated) // this only works when player is in combat mode
        {
            // Ray from the SpawnPoint to the Center of the screen
            Ray ray = new Ray(spawnPoint.position, spawnPoint.forward);
            RaycastHit hit;

            float rayLength = 1000f;

            if (Physics.Raycast(ray, out hit, rayLength, enemyLayer))
            {
                isOnTarget = true;

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, UnityEngine.Color.green);
            }

            else if (Physics.Raycast(ray, out hit, rayLength, otherLayer))
            {
                isOnTarget = false;

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, UnityEngine.Color.red);
            }

            else
            {
                isOnTarget = false;

                Debug.DrawRay(ray.origin, ray.direction * rayLength, UnityEngine.Color.white);
            }
        }
    }


    void ShootingMechanics()
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
            targetPoint = ray.GetPoint(1000f);

            //MAKE AN INVISIBLE BORDER SO THAT THE RAY TECHNICALLY IS ALWAYS HITTING AN OBJECT THAT THE AIM ALWAYS WORKS
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

    }


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
            yield return new WaitForSeconds(0.1f); //ZERO SINCE THE "SHOOTING ANIMATION" IS A LONG ENOUGH WAIT
            canShoot1 = true;
        }
    }

    #endregion


    #region STATE MACHINE:

    void StateMachine()
    {
        // ==== COMBAT MODE ====
        if (combatModeActivated)
        {
            if (isMoving)
                theVehicleMovement = VehicleMovement.combatMovement;
            else
                theVehicleMovement = VehicleMovement.combatIdle;

            return;
        }

        // ==== NORMAL MODE ====
        if (isAccelarating && isReversing)
            theVehicleMovement = VehicleMovement.revving;
        else if (isAccelarating)
            theVehicleMovement = VehicleMovement.accelerate;
        else if (isReversing)
            theVehicleMovement = VehicleMovement.reverse;
        else
            theVehicleMovement = VehicleMovement.idle;
    }


    #endregion


    #region PLAYER INPUTS (Controller):

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            combatModeActivated = true;
            isAccelarating = false;
            isReversing = false;
        }
        else
        {
            combatModeActivated = false;
        }
    }


    public void OnAccelerate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (combatModeActivated)
            {
                isHoldingShoot = true;
                StartCoroutine(ShootingControl());
            }
            else
            {
                isAccelarating = true;
            }
        }

        if (context.canceled)
        {
            isHoldingShoot = false;
            isAccelarating = false;
        }
    }


    public void OnReverse(InputAction.CallbackContext context)
    {
        if (context.performed && !combatModeActivated)
        {
            isReversing = true;
        }
        else
        {
            isReversing = false;
        }
    }


    public void OnMove(InputAction.CallbackContext context) //THIS WILL BE FOR STEERING
    {
        if (context.started || context.performed) //When the left stick is moved
        {
            if (combatModeActivated)
            {
                Vector2 stickInput = context.ReadValue<Vector2>();
                movementInput = new Vector3(stickInput.x, 0f, stickInput.y);
            }

            else //STEERING during NORMAL MOVEMENT 
            {
                movementInput = context.ReadValue<Vector2>();
                horizontalInputValue = movementInput.x; //equate the value to the VALUE OF THE LEFT STICK'S HORIZONTAL MOVEMENT (LEFT OR RIGHT)
            }
        }
        else if (context.canceled) //Reset values when stick is released
        {
            movementInput = Vector2.zero; 
            horizontalInputValue = 0; 
        }
    }

    #endregion
}
