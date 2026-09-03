using Unity.Cinemachine;
using UnityEngine;

public class _CanvasFacesCam : MonoBehaviour
{
    Transform target;
    public bool isPlayerHealth;
    [Space]
    public Transform cannon;
    public float horizontalOffset = 4f;
    public float verticalOffset = 1.5f;

    private void Awake()
    {
        target = GameObject.FindWithTag("MainCamera").transform;
    }

    void LateUpdate()
    {
        if (isPlayerHealth)
        {
            // Get camera directions
            Vector3 cameraRight = target.right;
            Vector3 cameraUp = target.up;

            // Position canvas to the LEFT of the cannon
            transform.position = cannon.position - cameraRight * horizontalOffset + cameraUp * verticalOffset;

            // Match camera rotation
            float cameraYAngle = target.eulerAngles.y;
            float cameraXAngle = target.eulerAngles.x;

            transform.rotation = Quaternion.Euler(cameraXAngle, cameraYAngle, 0f);
        }
        else
        {
            // Get ONLY the Y rotation from the camera target
            float cameraYAngle = target.eulerAngles.y;

            // Apply rotation to the canvas
            // Keeps camera aligned with camera direction (left/right only)
            transform.rotation = Quaternion.Euler(0f, cameraYAngle, 0f);
        }

        
    }
}
