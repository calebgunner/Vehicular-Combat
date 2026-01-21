using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.ScrollRect;

public class CameraControl : MonoBehaviour
{
    private VehicleControl vC;

    [Header("CANERA CONTROL")]
    public OperatingCamera theOperatingCamera;

    [Header("CAMERA SETTINGS")]
    Transform mainCamera;

    float cameraDistance;
    public float cameaDistance_Stationary = 10;
    public float cameaDistance_Driving = 13;
    public float cameaDistance_Combat = 7;

    [Space]
    public float sensitivity = 120f;  // Look sensitivity
    public float minY = -40f;          // Min vertical angle
    public float maxY = 70f;           // Max vertical angle
    float easingSpeed = 3f;

    Vector2 lookInput;                           // Stored look input
    float xRotation;                             // Vertical rotation
    float yRotation;                             // Horizontal rotation

    Rigidbody playerRb;


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

        cameraDistance = cameaDistance_Stationary;
    }


    void Update()
    {
        CameraInUse();
    }


    private void LateUpdate()
    {
        CameraSetting();
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
                cameraDistance = Mathf.Lerp(cameraDistance, cameaDistance_Stationary, easingSpeed * Time.deltaTime);
            else if (theOperatingCamera == OperatingCamera.drivingCamera)
                cameraDistance = Mathf.Lerp(cameraDistance, cameaDistance_Driving, easingSpeed * Time.deltaTime);

        }
    }

    // ADD REVVING CONTROL
    // PLAY ARKHAM BATMAN AND DOCUMENT CONTROL SCHEME
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
        transform.LookAt(playerRb.transform.position);
    }




    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
