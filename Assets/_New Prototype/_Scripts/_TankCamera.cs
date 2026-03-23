using UnityEngine;
using UnityEngine.InputSystem;

public class _TankCamera : MonoBehaviour
{
    /// <summary>
    /// /////////////////////// JUST FOCUS ON FIXING/REVAMPING CAMERA SYSTEM TODAY !!!
    /// </summary>
    
    #region inspector values:

    _TankControl tC;
    Transform mainCamera;

    [Header("camera settings")]
    public float cameraDistance;

    [Space]
    public float sensitivity = 120f;    // Look sensitivity
    public float minY = -40f;           // Min vertical angle
    public float maxY = 70f;            // Max vertical angle
    float combatEasingSpeed = 10f;

    Vector2 lookInput;                  // Stored look input
    float xRotation;                    // Vertical rotation
    float yRotation;                    // Horizontal rotation

    Rigidbody playerRb;

    [Header("positions and offsets")]
    public Transform gunTransform;
    public Transform theOffset;

    [Space]
    public Vector3 defaultPosition;
    public Vector3 defaultRotation;
    public Vector3 defaultCameraPosition;
    public Transform deafaltCamPosition_Transform;

    [Header("rotation insults")]
    public float timePassed;
    public float interval = 3.5f;
    public bool rotationInput;

    #endregion


    void Awake()
    {
        // ====== CURSOR SETTINGS =======
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // ====== START VALUES =======
        tC = FindFirstObjectByType<_TankControl>();
        playerRb = tC.GetComponent<Rigidbody>();
        mainCamera = Camera.main.transform;


    }


    void Update()
    {
        CombatCamera();
    }


    private void LateUpdate()
    {
        GunFollowCameraDirection();
    }


    void GunFollowCameraDirection()
    {
        // Change the Y-DIRECTION so that it will only follow the Camera's Y-DIRECTION (Rotating from left-to-right)

        // Get the CAMERA's Y-angle
        float cameraYAngle = Camera.main.transform.eulerAngles.y;

        // Create a new rotation with the target's Y angle and keep the current X and Z angles
        Vector3 newYRotation = new Vector3(0, cameraYAngle, 0);

        // Apply the new rotation using Quaternion.Euler
        gunTransform.transform.rotation = Quaternion.Euler(newYRotation);
    }


    void CombatCamera()
    {
        // Read look input and update rotation values
        yRotation += lookInput.x * sensitivity * Time.deltaTime;
        xRotation -= lookInput.y * sensitivity * Time.deltaTime;

        // Clamp vertical rotation so the camera cannot flip
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        // Build rotation from current x and y values
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Calculate offset behind the player
        Vector3 offset = rotation * Vector3.back * cameraDistance;

        // Follow the player smoothly
        transform.position = playerRb.transform.position + offset;
        transform.LookAt(theOffset.position);
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
