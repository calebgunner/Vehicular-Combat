using UnityEngine;

public class _GameplayTips : MonoBehaviour
{
    void Start()
    {
        //FREE THE TIMESCALE THE MOMENET THE GAMEPLAY TIP HAS BEEN ACTIVATED
        Time.timeScale = 0;
    }
}
