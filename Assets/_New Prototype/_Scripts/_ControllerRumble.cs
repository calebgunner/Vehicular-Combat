using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class _ControllerRumble : MonoBehaviour
{
    // THIS CONTROLS THE VIBRATION WHEN CERTAIN EVENTS OCCUR, i.e. PLAYER BEING HIT, PLAYER DYING etc.

    // lowMotorStrength  = strength of the low frequency motor
    //                     Gives heavier, deeper vibration

    // highMotorStrength = strength of the high frequency motor
    //                     Gives sharper, lighter vibration

    // Starts the controller vibration with the given strength and duration
    public void Rumble(float lowMotorStrength, float highMotorStrength, float duration)
    {
        StartCoroutine(RumbleRoutine(lowMotorStrength, highMotorStrength, duration));
    }


    // A coroutine is used because the vibration needs to happen over time.
    IEnumerator RumbleRoutine(float lowMotorStrength, float highMotorStrength, float duration)
    {
        // Check whether a gamepad is currently connected.
        // Stop here if there is no controller connected
        if (Gamepad.current == null)
            yield break;

        // Start vibration (values range from 0 to 1)
        Gamepad.current.SetMotorSpeeds(lowMotorStrength, highMotorStrength);

        // Keep vibrating for the given duration
        yield return new WaitForSeconds(duration);

        // The motors do not automatically stop after the duration.
        // Stop vibration
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }
}
