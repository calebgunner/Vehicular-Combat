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
    float speed;
    public float stationarySpeed;
    public float reversingSpeed;
    public float accelaratingSpeed;

    [Space]
    Vector3 moveVelocity;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Vector3 movementInput;


    // ADD ENUMS FOR STATE MACHINE.... THEN ADD THE SPEED FOR EACH STATE: ACC< REV< COMBAT< STATIONARY



    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        
    }


    private void FixedUpdate()
    {
        Movement();
    }


    #region MOVEMENT:

    void Movement() //START A PROPER STATE MACHINE IN TH ENEXT BLOCK
    {
        // CONTROL DIRECTION OF THE MOVEMENT (accelerate or reverse)
        if (isAccelarating)
        {
            moveVelocity = transform.forward * speed;
        }
        else if (isReversing)
        {
            moveVelocity = -transform.forward * speed;
        }

        Vector3 finalVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        rb.linearVelocity = finalVelocity;

        // DOING BOTH REVS ENGINE AND STAGNATES MOVEMENT
        if (!combatModeActivated && isAccelarating &&  isReversing)
        {
            speed = stationarySpeed;
        }
        else
        {
            speed = accelaratingSpeed;
        }
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
            movementInput = context.ReadValue<Vector2>(); //Get the values of the left stick
            isMoving = true;
        }
        else if (context.canceled) //Reset when stick is released
        {
            movementInput = Vector2.zero; // Reset input to avoid lingering direction
            isMoving = false;
        }
    }

    #endregion
}
