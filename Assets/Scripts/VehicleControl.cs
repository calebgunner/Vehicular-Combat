using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    #endregion


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        isMoving = (isAccelarating || isReversing) || ((movementInput.magnitude > 0.01f) && combatModeActivated); //write what this means

        StateMachine();
        SpeedControl();
        Steering();

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
        if (context.performed && !combatModeActivated)
        {
            isAccelarating = true;
        }
        else
        {
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
