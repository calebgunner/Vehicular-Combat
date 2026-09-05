using UnityEngine;

public class _TankAnimations : MonoBehaviour
{
    [Header("animator controls")]
    public Animator CrossAnimator;
    string currentState;

    [Space]
    public Animator TankAnimator;
    string currentHeadState;
    string currentWheelsState;


    [Header("other scripts")]
    public _TankControl tC;

    // Cache your animation state names to avoid typos
    #region crosshair animation names:
    const string IDLE_CROSSHAIR = "IDLE_CROSSHAIR";
    const string SHOOT_CROSSHAIR = "SHOOT_CROSSHAIR";
    #endregion

    #region tank animation names:
    const string TANK_HEAD_IDLE = "TANK_HEAD_IDLE";
    const string TANK_HEAD_SHOOTING = "TANK_HEAD_SHOOTING";
    
    const string TANK_BODY_IDLE = "TANK_BODY_IDLE";
    const string TANK_BODY_MOVING = "TANK_BODY_MOVING";

    const string TANK_WHEELS_IDLE = "TANK_WHEELS_IDLE";
    const string TANK_WHEELS_SPIN = "TANK_WHEELS_SPIN";
    #endregion

    void Update()
    {
        //CrossHair Animation Control
        if (tC.theTankSot == TankShot.shooting) //GET THE ENUMS FROM THE "TANKCONTROL" SCRIPT
        {
            ChangeAnimationState(SHOOT_CROSSHAIR);
            ChangeMovementAnimationState(TANK_HEAD_SHOOTING, 0); //Shooting Animation
        }

        else if (tC.theTankSot == TankShot.notShooting)
        {
            ChangeAnimationState(IDLE_CROSSHAIR);
            ChangeMovementAnimationState(TANK_HEAD_IDLE, 0);
        }


        //Tank Movement Animation Control
        if (tC.theTankMovement == TankMovement.idle) //Keep the wheels idle
        {
            ChangeMovementAnimationState(TANK_BODY_IDLE, 1);
            ChangeMovementAnimationState(TANK_WHEELS_IDLE, 2);
        }
        else if(tC.theTankMovement == TankMovement.dodging) //Keep the wheels idle
        {
            ChangeMovementAnimationState(TANK_BODY_MOVING, 1);
            ChangeMovementAnimationState(TANK_WHEELS_IDLE, 2);
        }
        else if (tC.theTankMovement == TankMovement.movement)
        {
            ChangeMovementAnimationState(TANK_BODY_MOVING, 1);
            ChangeMovementAnimationState(TANK_WHEELS_SPIN, 2);
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

    void ChangeMovementAnimationState(string newState, int layerIndex)
    {
        // Check the correct tracking variable based on the layer index
        if (layerIndex == 0 && currentHeadState == newState) return;
        if (layerIndex == 1 && currentWheelsState == newState) return;
        if (layerIndex == 2 && currentWheelsState == newState) return;

        // Play the animation on the SPECIFIC layer index
        TankAnimator.Play(newState, layerIndex);

        // Update the correct tracking variable
        if (layerIndex == 0) currentHeadState = newState;
        if (layerIndex == 1) currentWheelsState = newState;
        if (layerIndex == 2) currentWheelsState = newState;
    }
}
