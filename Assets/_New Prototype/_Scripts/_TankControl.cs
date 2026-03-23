using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public enum TankMovement //USE IT OUTSIDE THE CLASSES TO MAKE THINGS A LOT EASIER
{
    // ==== MOVEMENT ====
    idle,
    movement,

    // ==== DODGE ====
    dodging
}

public class _TankControl : MonoBehaviour
{
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


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (isDodging) return;  //prevents a boolean from overriding this until dodging has completed

        // ANY MOVEMENT WILL TRIGGER THE BOOLEAN
        isMoving = movementInput.magnitude > 0.01f;

        #region OTHER FUNCTIONS:

        StateMachine();

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
            Vector3 localMovement = transform.forward * movementInput.z + transform.right * movementInput.x; //GETTING THE FWD/BWD AND LEFT/RIGHT DIRECTION
            rb.linearVelocity = localMovement * speed + Vector3.up * rb.linearVelocity.y; //MOVING THE OBJECT IN THAT DIRECTION ACCORDING TO THE SPEED... Y-VALUE remains the same and is not touched

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

        Vector3 localMovement = transform.forward * movementInput.z + transform.right * movementInput.x; //GETTING DODGE DIRECTION
        rb.AddForce(localMovement * dodgeForce, ForceMode.Impulse); //ADD A FORCE IN THE DODGE DIRECTION

        theTankMovement = TankMovement.dodging; //change the movement to 'dodging'

        yield return new WaitForSeconds(dodgeDuration);

        canDodge = false; //prevents the player from dodging consecutively 
        isDodging = false;
        theTankMovement = TankMovement.idle; //return to default

        yield return new WaitForSeconds(canDodgeDelay);

        canDodge = true; //alows the player to dodge after the "canDodgeDelay" has passed
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

    #endregion
}
