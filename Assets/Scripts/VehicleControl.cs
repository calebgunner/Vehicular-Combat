using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VehicleControl : MonoBehaviour
{
    [Header("activated mode")]
    public bool combatModeActivated;
    public bool isMoving;
    public bool isAccelarating;
    public bool isReversing;

    [Space]
    public float speed;
    public float stationarySpeed;
    public float reversingSpeed;
    public float accelaratingSpeed;
    public float combatSpeed;

    [Space]
    bool hasDriveInput;
    Vector3 moveVelocity;

    [Space]
    bool rotateLeft;
    bool rotateRight;

    //ROTATE THE PLAYERTRANSFORM LEFT OR ROGHT WHEN DRIVING TO STEER.... ONE CLICK OF THE STICK MOVES IT 45d ON IT'S RESPECTIVE SIDE

    [Space]
    public VehicleMovement theVehicleMovement;

    public enum VehicleMovement
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

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 movementInput;


    // ADD ENUMS FOR STATE MACHINE.... THEN ADD THE SPEED FOR EACH STATE: ACC< REV< COMBAT< STATIONARY



    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        hasDriveInput = isAccelarating || isReversing;

        // check if the vehicle is moving BY LOOKING AT THE VELOCITY CHANGE
        //isMoving = (rb.linearVelocity.magnitude > 0.01f);

        StateMachine();
        SpeedControl();
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

    }

    #endregion


    #region STATE MACHINE:

    void StateMachine()
    {
        // COMBAT MODE
        if (combatModeActivated)
        {
            if (isAccelarating || isReversing)
                theVehicleMovement = VehicleMovement.combatMovement;
            else
                theVehicleMovement = VehicleMovement.combatIdle;

            return;
        }

        // NORMAL MODE
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
            //movementInput = context.ReadValue<Vector2>(); //Get the values of the left stick
            //isMoving = true;

            if (combatModeActivated)
            {

            }

            else //STEERING during NORMAL MOVEMENT 
            {
                movementInput = context.ReadValue<Vector2>();
                float horizontalInputValue = movementInput.x;

                if (horizontalInputValue < -0.1f)
                {
                    rotateLeft = horizontalInputValue < -0.1f; // STICK MOVING LEFT
                }
                else if (horizontalInputValue > 0.1f)
                {
                    rotateRight = horizontalInputValue > 0.1f; // STICK MOVING RIGHT
                }
            }
        }
        else if (context.canceled) //Reset when stick is released
        {
            movementInput = Vector2.zero; // Reset input to avoid lingering direction
            //isMoving = false;
        }
    }

    #endregion
}
