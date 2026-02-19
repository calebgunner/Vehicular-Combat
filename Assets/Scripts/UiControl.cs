using Unity.VisualScripting;
using UnityEngine;

public class UiControl : MonoBehaviour
{
    // for now the ammo does not decrease.. try this out before adding ammo as players will be continously dodging enemy attacks

    [Header("ui images")]
    GameObject Reticle_OnTarget;
    GameObject Reticle_OffTarget;

    [Header("other scripts")]
    public VehicleControl vC;

    void Start()
    {
        Reticle_OnTarget = GameObject.Find("Reticle (On Target)");
        Reticle_OffTarget = GameObject.Find("Reticle (Off Target)");
    }


    void Update()
    {
        UiSpriteControl();
    }


    // UI SPRITE CONTROL
    void UiSpriteControl()
    {
        //The Reticle (On Target) Appears when the combat mode is activated and aim is On Target
        Reticle_OnTarget.SetActive(vC.combatModeActivated && vC.isOnTarget);

        Reticle_OffTarget.SetActive(vC.combatModeActivated && !vC.isOnTarget);
    }
}
