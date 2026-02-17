using Unity.VisualScripting;
using UnityEngine;

public class UiControl : MonoBehaviour
{
    GameObject ReticleImage;

    public CameraControl cc;

    void Start()
    {
        ReticleImage = GameObject.Find("Reticle");
    }


    void Update()
    {
        UiSpriteControl();
    }


    // UI SPRITE CONTROL
    void UiSpriteControl()
    {
        //The Reticle Appears when the combat mode is activated
        ReticleImage.SetActive(cc.theOperatingCamera == OperatingCamera.combatCamera);
    }
}
