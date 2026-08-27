using UnityEngine;

public class _TankAnimations : MonoBehaviour
{
    [Header("animator controls")]
    public Animator CrossAnimator;
    private string currentState;

    [Header("other scripts")]
    public _TankControl tC;

    // Cache your animation state names to avoid typos
    #region crosshair animations:
    private const string IDLE_CROSSHAIR = "IDLE_CROSSHAIR";
    private const string SHOOT_CROSSHAIR = "SHOOT_CROSSHAIR";
    #endregion

    void Update()
    {
        if (tC.theTankSot == TankShot.shooting) //GET THE ENUMS FROM THE "TANKCONTROL" SCRIPT
        {
            ChangeAnimationState(SHOOT_CROSSHAIR);
        }

        else if (tC.theTankSot == TankShot.notShooting)
        {
            ChangeAnimationState(IDLE_CROSSHAIR);
        }
    }

    /// <summary>
    /// Safely changes the animation state without restarting the clip if it's already playing.
    /// </summary>
    void ChangeAnimationState(string newState)
    {
        // Stop the animation from interrupting itself
        if (currentState == newState) return;

        // Play the animation state directly
        CrossAnimator.Play(newState);

        // Update the current tracking state
        currentState = newState;
    }
}
