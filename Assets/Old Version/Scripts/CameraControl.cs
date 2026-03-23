using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;
using UnityEngine.Rendering;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.ScrollRect;


public enum OperatingCamera //USE IT OUTSIDE THE CLASSES TO MAKE THINGS A LOT EASIER
{
    stationaryCamera,
    drivingCamera,
    combatCamera
}


public class CameraControl : MonoBehaviour
{
    // COMBAT CAM... LET GUN ROTATE WITH CAM ===========================> DONE
    // MAKE THE SHOOTING SYSTEM (SHOOT) MACHINE-GUN ====================> DONE



    ///WHEN IN COMBAT MODE:
    ///IF MOVING, CAR FOLLOWS CAMERA FORWARD
    ///IF STATIONARY, ONLY THE GUN FOLLOWS CAM FORWARD

    // ADDDDDDDDDDDDD CAMERA MOVE TO DEFAULT POSITION WHEN TIME HAS PASSED WITH NO INPUT
    // ADD THIS
    // MAKE CAM BE LINKED TO PLAYER WHEN REVVING SO THA IT MOVES WITH PLAYER
    // AND COMBAT CAMERA

    #region inspector values:

    VehicleControl vC;
    Transform mainCamera;

    [Header("camera control")]
    public OperatingCamera theOperatingCamera;

    [Header("camera settings")]
    public float cameraDistance_Stationary;
    public float cameraDistance_Driving;
    public float cameraDistance_Combat;
    float cameraDistance;

    [Space]
    public float sensitivity = 120f;    // Look sensitivity
    public float minY = -40f;           // Min vertical angle
    public float maxY = 70f;            // Max vertical angle
    float easingSpeed = 3f;
    float combatEasingSpeed = 10f;

    Vector2 lookInput;                  // Stored look input
    float xRotation;                    // Vertical rotation
    float yRotation;                    // Horizontal rotation

    Rigidbody playerRb;

    [Header("positions and offsets")]
    public Transform gunTransform;
    public Transform theOffset;
    [Space]
    float OffsetHeight;
    public float OffsetHeight_Combat = 2.5f;
    public float OffsetHeight_NonCombat = 1.5f;

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
        // ====== DEFAULT OFSSET Y-VALUE PLACEMENT ======
        OffsetHeight = OffsetHeight_NonCombat;
        theOffset.localPosition = new Vector3(theOffset.localPosition.x, OffsetHeight, theOffset.localPosition.z);

        // ====== CURSOR SETTINGS =======
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // ====== START VALUES =======
        vC = FindFirstObjectByType<VehicleControl>();
        playerRb = vC.GetComponent<Rigidbody>();
        mainCamera = Camera.main.transform;

        cameraDistance = cameraDistance_Stationary;

        
    }


    void Update()
    {
        CamStateMachine();
    }


    private void LateUpdate()
    {
        CameraSetting();
        GunFollowCameraDirection();

        // ========== TIME PASSED WITH NO INPUT ==========
        if (!rotationInput && vC.isMoving)
        {
            timePassed += Time.deltaTime; // accumalate time since last frame

            if (timePassed >= interval)
            {
                // The code here will execute every 2 seconds
                Debug.Log("RESET THE CAMERA");

                // Use standard = 0 if you want a hard reset after the action, 
                timePassed = 0;

                // Add your camera reset code here
                ResetCamera();
            }
        }
        else
        {
            // If there IS rotation input, keep the timer at zero
            timePassed = 0;
        }

        // ========== THE DEFAULT CAMERA POSITION ==========

        //Cam position relative to car
        //defaultCameraPosition = vC.transform.position - vC.transform.
        //Vector3 defaultPosition_FromCamera = new Vector3(0, 3.078181f, -8.457233f);
        //deafaltCamPosition_Transform.position = vC.transform.position + defaultPosition_FromCamera;


        //Quaternion targetRotation = Quaternion.Euler(defaultRotation); //rotation relative to car
        //deafaltCamPosition_Transform.rotation = vC.transform.rotation * targetRotation;
    }


    void GunFollowCameraDirection()
    {
        // Change the Y-DIRECTION so that it will only follow the Camera's Y-DIRECTION (Rotating from left-to-right)

        // Get the CAMERA's Y-angle
        float cameraYAngle = Camera.main.transform.eulerAngles.y;

        // Create a new rotation with the target's Y angle and keep the current X and Z angles
        Vector3 newYRotation = new Vector3(0, cameraYAngle, 0);

        // Apply the new rotation using Quaternion.Euler
        vC.GunObject.transform.rotation = Quaternion.Euler(newYRotation);
    }


    void CameraSetting()
    {
        //mainCamera.position = new Vector3(mainCamera.x, mainCamera.y, cameraDistance);

        if (theOperatingCamera == OperatingCamera.combatCamera) //combat camera settings
        {
            CombatCamera();

            if (theOperatingCamera == OperatingCamera.combatCamera)
                cameraDistance = Mathf.Lerp(cameraDistance, cameraDistance_Combat, combatEasingSpeed * Time.deltaTime);
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

        // THE CAMERA OFFSET Y-POSITION: this affects where the camera faces (AFTER CHANGES HAVE ALREADY BEEN MADE)
        theOffset.localPosition = new Vector3(theOffset.localPosition.x, OffsetHeight, theOffset.localPosition.z);
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

        // Ease the Y-OFFSET height change
        OffsetHeight = Mathf.Lerp(OffsetHeight, OffsetHeight_Combat, combatEasingSpeed * Time.deltaTime);
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

        // Ease the Y-OFFSET height change
        OffsetHeight = Mathf.Lerp(OffsetHeight, OffsetHeight_NonCombat, easingSpeed * Time.deltaTime);

    }


    void ResetCamera()
    {

    }


    #region STATE MACHINE (CAMERA):

    void CamStateMachine()
    {
        // ====== combat camera ======
        if (vC.theVehicleMovement == VehicleMovement.combatMovement || vC.theVehicleMovement == VehicleMovement.combatIdle)
            theOperatingCamera = OperatingCamera.combatCamera;

        else if (vC.theVehicleMovement == VehicleMovement.accelerate || vC.theVehicleMovement == VehicleMovement.reverse)
            theOperatingCamera = OperatingCamera.drivingCamera;

        else if (vC.theVehicleMovement == VehicleMovement.idle || vC.theVehicleMovement == VehicleMovement.revving) //controls the same as the driving camera
            theOperatingCamera = OperatingCamera.stationaryCamera;
    }

    #endregion


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
