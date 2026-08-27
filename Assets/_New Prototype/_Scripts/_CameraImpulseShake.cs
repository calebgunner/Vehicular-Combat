using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class _CameraImpulseShake : MonoBehaviour
{
    // THIS SCRIPT CONTROLS THE CAMERA SHAKE WHEN AN ACTION IS PERFORMED

    [SerializeField] CinemachineImpulseSource screenShake;


    public void ScreenShake(Vector3 dir, float power, float duration, CinemachineImpulseDefinition.ImpulseShapes shape)
    {
        screenShake.ImpulseDefinition.ImpulseShape = shape;
        screenShake.ImpulseDefinition.ImpulseDuration = duration;

        screenShake.GenerateImpulseWithVelocity(dir * power);
    }
}
