using Unity.Cinemachine;
using UnityEngine;

public class _CanvasFacesCam : MonoBehaviour
{
    Transform target;

    private void Awake()
    {
        target = GameObject.FindWithTag("MainCamera").transform;
    }

    void Update()
    {
        // Get ONLY the Y rotation from the camera target
        float cameraYAngle = target.eulerAngles.y;

        // Apply rotation to the canvas
        // Keeps camera aligned with camera direction (left/right only)
        transform.rotation = Quaternion.Euler(0f, cameraYAngle, 0f);
    }
}
