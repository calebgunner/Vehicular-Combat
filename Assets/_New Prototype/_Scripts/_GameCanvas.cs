using UnityEngine;
using UnityEngine.UI;

public class _GameCanvas : MonoBehaviour
{
    public Image reticleImage;
    public Sprite[] differentReticles;

    [Space]
    _TankControl tControl;


    private void Awake()
    {
        tControl = GameObject.FindWithTag("Player").GetComponent<_TankControl>();
    }


    void Update()
    {

        ReticleImageControl();
    }


    void ReticleImageControl()
    {
        //Control the RETICLE APPEARENCE

        if (tControl.theReticleControl == ReticleControl.onTarget)
            reticleImage.sprite = differentReticles[0];

        else if (tControl.theReticleControl == ReticleControl.onTargetAndShoot)
            reticleImage.sprite = differentReticles[1];

        else
            reticleImage.sprite = differentReticles[2];
    }
}
