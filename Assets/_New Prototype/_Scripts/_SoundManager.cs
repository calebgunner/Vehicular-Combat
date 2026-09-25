using UnityEngine;

public class _SoundManager : MonoBehaviour
{   
    //WHEN ALL OF THE SOUNDS ARE IN, LEARN HOW TO PLAY WITH THE PITCHES

    private _TankControl tC;

    #region TANK (SOUND EFFECTS):
    public AudioSource AudioSourceMoving;
    public AudioClip tankMoving;
    [Range(0f, 1f)] public float tankMoving_Volume;

    [Space]
    public AudioSource AudioSourceDodge;
    public AudioClip tankDodges;
    [Range(0f, 1f)] public float tankDodges_Volume;


    #endregion


    void Start()
    {
        tC = GameObject.FindWithTag("Player").GetComponent<_TankControl>();
    }


    void Update()
    {
        TankIsMoving();
        TankDodges();
    }


    // SOUND EFFECTS FOR THE VARIOUS TANK CONTROLS
    void TankIsMoving()
    {
        // If the player is moving AND not currently dodging
        if (tC.isMoving && !tC.isDodging)
        {
            if (!AudioSourceMoving.isPlaying)
            {
                // Explicitly set the moving clip back in case it got cleared
                AudioSourceMoving.clip = tankMoving;
                AudioSourceMoving.Play();
            }
        }
        else
        {
            if (AudioSourceMoving.isPlaying)
            {
                AudioSourceMoving.Stop();
            }
        }
    }

    void TankDodges()
    {
        if (tC.isDodging)
        {
            // Only fire the one-shot if the dodge source isn't already busy
            if (!AudioSourceDodge.isPlaying)
            {
                AudioSourceDodge.PlayOneShot(tankDodges, tankDodges_Volume);
            }
        }
    }

}
