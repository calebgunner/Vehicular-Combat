using UnityEngine;
using UnityEngine.InputSystem;

public class _TankCamera : MonoBehaviour
{
    /// THE "THIRD PERSON AIM CAMERA" IS LINKED TO THE "CAMERA-TARGET" ROTATION HENCE WHY THAT IS WHAT WE ARE ROTATING IT
    
    
    #region inspector values:

    _TankControl tC;
    Rigidbody playerRb;     // Rigidbody of the player (used if needed for future logic)

    [Header("camera settings")]
    public float sensitivity = 60f;     // Look sensitivity
    public float minY = -40f;           // Min vertical angle
    public float maxY = 70f;            // Max vertical angle

    Vector2 lookInput;                  // Stores input from controller (right stick)
    float xRotation;                    // Vertical rotation  (up/down)
    float yRotation;                    // Horizontal rotation (left/right)

    [Header("references")]
    public Transform gunTransform;
    public Transform cameraTarget;      //(Empty Child Object on Player) Target that Cinemachine follows and rotates around

    [Space]
    public bool rotationInput;          // Used to check if player is actively giving rotation input

    #endregion


    void Awake()
    {
        // ====== CURSOR SETTINGS =======
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // ====== START VALUES =======
        tC = FindAnyObjectByType<_TankControl>();
        playerRb = tC.GetComponent<Rigidbody>();

        // IMPORTANT:
        // Initialise rotation values based on current cameraTarget rotation
        // Prevents snapping when the game starts
        Vector3 angles = cameraTarget.eulerAngles;
        yRotation = angles.y;
        xRotation = angles.x;
    }


    void FixedUpdate()
    {
        HandleCameraRotation(); //This is put in FIXED-UPDATE due to the movement also being in FIXED-UPDATE
        HandleTankRotation();
    }


    private void LateUpdate()
    {
        // Apply gun rotation AFTER camera has updated
        // LateUpdate ensures smoother syncing
        GunFollowCameraDirection();
    }


    // ==== GUN ROTATION ==== 
    void GunFollowCameraDirection() //will eventually make the head follow the reticle in all directions
    {
        // Get ONLY the Y rotation from the camera target
        float cameraYAngle = cameraTarget.eulerAngles.y;

        // Apply rotation to the gun
        // Keeps gun aligned with camera direction (left/right only)
        gunTransform.rotation = Quaternion.Euler(0f, cameraYAngle, 0f);
    }


    // ==== TANK ROTATION ==== 
    void HandleTankRotation()
    {
        if (tC.theTankMovement == TankMovement.movement && rotationInput)
        {
            // Get ONLY the Y rotation from the camera target
            float cameraYAngle = cameraTarget.eulerAngles.y;

            // Target rotation around Y
            Quaternion targetRotation = Quaternion.Euler(0f, cameraYAngle, 0f);

            // Smoothly rotate tank towards camera direction
            tC.transform.rotation = Quaternion.Slerp(
                tC.transform.rotation,      // current rotation
                targetRotation,             // target rotation
                8f * Time.fixedDeltaTime    // smoothing factor, tweak for speed
            );
        }
    }


    // ==== CAMERA ROTATION ==== 
    void HandleCameraRotation()
    {
        // Create a local copy of input (so we can modify it safely)
        Vector2 input = lookInput;

        // DEADZONE:
        // Prevents small stick drift from moving camera
        if (input.magnitude < 0.1f)
        {
            input = Vector2.zero;
        }

        yRotation += input.x * sensitivity * Time.fixedDeltaTime;    // Update horizontal rotation (left/right)
        xRotation -= input.y * sensitivity * Time.fixedDeltaTime;    // Update vertical rotation (up/down)

        // Clamp vertical rotation so camera cannot flip
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        // Apply rotation to camera target
        // Cinemachine will follow this automatically
        cameraTarget.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }


    #region PLAYER INPUTS (CAMERA CONTROLS):

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            lookInput = context.ReadValue<Vector2>();
            rotationInput = true;
        }
        else if (context.canceled)
        {
            rotationInput = false;
            lookInput = Vector2.zero;
        }
    }

    #endregion
}
