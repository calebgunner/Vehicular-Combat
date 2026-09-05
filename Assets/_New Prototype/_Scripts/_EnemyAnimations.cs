using UnityEngine;

public class _EnemyAnimations : MonoBehaviour
{
    [Header("animator controls")]
    Animator EnemyTurretAnimator;
    string currentHeadState;
    string currentBodyState;

    [Header("other scripts")]
    _EnemyAttack eA;

    // Cache your animation state names to avoid typos
    #region crosshair animations:
    const string ET_HEAD_IDLE = "ET_Head_Idle";
    const string ET_HEAD_SHOOTING = "ET_Head_Shooting";
    const string ET_BODY_IDLE = "ET_Body_Idle";
    #endregion

    private void Start()
    {
        eA = gameObject.GetComponent <_EnemyAttack>();
        EnemyTurretAnimator = gameObject.GetComponent<Animator>();

        // Start the body's idle once on its layer; it will loop forever on its own
        ChangeAnimationState(ET_BODY_IDLE, 1);
    }

    void Update()
    {
        if (eA.theEnemyAttackSequence == EnemyAttackSequence.FollowPlayer || eA.theEnemyAttackSequence == EnemyAttackSequence.StopAndReadyToShoot)
        {
            ChangeAnimationState(ET_HEAD_IDLE, 0);
        }

        else if (eA.theEnemyAttackSequence == EnemyAttackSequence.Shoot)
        {
            ChangeAnimationState(ET_HEAD_SHOOTING, 0);
        }
    }

    /// <summary>
    /// Safely changes the animation state without restarting the clip if it's already playing.
    /// </summary>
    void ChangeAnimationState(string newState, int layerIndex)
    {
        // Check the correct tracking variable based on the layer index
        if (layerIndex == 0 && currentHeadState == newState) return;
        if (layerIndex == 1 && currentBodyState == newState) return;

        // Play the animation on the SPECIFIC layer index
        EnemyTurretAnimator.Play(newState, layerIndex);

        // Update the correct tracking variable
        if (layerIndex == 0) currentHeadState = newState;
        if (layerIndex == 1) currentBodyState = newState;
    }
}
