using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;
using UnityEngine.Rendering;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.ScrollRect;

public class CameraControl : MonoBehaviour
{
    // FIRST, FIX AND ORGANISE BOTH SCRIPTSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS



    // ADDDDDDDDDDDDD CAMERA MOVE TO DEFAULT POSITION WHEN TIME HAS PASSED WITH NO INPUT
    // ADD THIS
    // MAKE CAM BE LINKED TO PLAYER WHEN REVVING SO THA IT MOVES WITH PLAYER

    private VehicleControl vC;

    [Header("CANERA CONTROL")]
    public OperatingCamera theOperatingCamera;

    [Header("CAMERA SETTINGS")]
    Transform mainCamera;

    float cameraDistance;
    public float cameraDistance_Stationary;
    public float cameraDistance_Driving;
    public float cameraDistance_Combat;

    [Space]
    public float sensitivity = 120f;  // Look sensitivity
    public float minY = -40f;          // Min vertical angle
    public float maxY = 70f;           // Max vertical angle
    float easingSpeed = 3f;

    Vector2 lookInput;                           // Stored look input
    float xRotation;                             // Vertical rotation
    float yRotation;                             // Horizontal rotation

    Rigidbody playerRb;

    [Space]
    public Transform gunTransform;
    public Transform theOffset;

    [Space]
    public Vector3 defaultPosition;
    public Vector3 defaultRotation;
    public Vector3 defaultCameraPosition;
    public Transform deafaltCamPosition_Transform;

    [Space] //time with NO INPUT
    public float timePassed;
    public float interval = 3.5f;
    public bool rotationInput;


    public enum OperatingCamera
    {
        stationaryCamera,
        drivingCamera,
        combatCamera
    }


    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        vC = FindFirstObjectByType<VehicleControl>();
        playerRb = vC.GetComponent<Rigidbody>();
        mainCamera = Camera.main.transform;

        cameraDistance = cameraDistance_Stationary;

        
    }


    void Update()
    {
        CameraInUse();
    }


    private void LateUpdate()
    {
        CameraSetting();

        // TIME PASSED WITH NO INPUT
        if (!rotationInput && vC.isMoving)
        {
            timePassed += Time.deltaTime; // accumalate time since last frame

            if (timePassed >= interval)
            {
                // The code here will execute every 2 seconds
                Debug.Log("RESET THE CAMERA");

                // Use standard = 0 if you want a hard reset after the action, 
                // or -= interval to keep it looping every 2 seconds.
                timePassed = 0;

                // Add your camera reset code here
                //ResetCamera();
            }
        }
        else
        {
            // If there IS rotation input, keep the timer at zero
            timePassed = 0;
        }

        // ========== THE DEFAULT CAMERA POSITION ==========
        defaultPosition = new Vector3(0, 3.078181f, -8.457233f);
        defaultRotation = new Vector3(10.57f, 0, 0);

        // Cam position relative to car
        // This converts 'defaultPosition' from a local offset into a world position based on where the car is and which way it's facing.
        deafaltCamPosition_Transform.position = vC.transform.TransformPoint(defaultPosition);

        Quaternion targetation = Quaternion.Euler(defaultRotation); //rotation relative to car
        deafaltCamPosition_Transform.rotation = vC.transform.rotation * targetation;
    }

    // ====== CAMERA CONTROL ======
    void CameraInUse()
    {
        if (vC.isMoving)
        {
            if (vC.combatModeActivated)
                theOperatingCamera = OperatingCamera.combatCamera;

            else if (vC.isAccelarating && vC.isReversing) //revving the engine
                theOperatingCamera = OperatingCamera.stationaryCamera;

            else
                theOperatingCamera = OperatingCamera.drivingCamera;
        }
        else
        {
            theOperatingCamera = OperatingCamera.stationaryCamera;
        }
    }


    void CameraSetting()
    {
        //mainCamera.position = new Vector3(mainCamera.x, mainCamera.y, cameraDistance);

        if (theOperatingCamera == OperatingCamera.combatCamera) //combat camera settings
        {

        }

        else //driving camera and stationary camera settings
        {
            Non_CombatCamera();

            // Set the camera distance
            if (theOperatingCamera == OperatingCamera.stationaryCamera)
                cameraDistance = Mathf.Lerp(cameraDistance, cameraDistance_Stationary, easingSpeed * Time.deltaTime);
            else if (theOperatingCamera == OperatingCamera.drivingCamera)
                cameraDistance = Mathf.Lerp(cameraDistance, cameraDistance_Driving, easingSpeed * Time.deltaTime);

        }
    }

    

    void Non_CombatCamera()
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
}
