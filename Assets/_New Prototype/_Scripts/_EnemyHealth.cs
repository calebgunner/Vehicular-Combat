using UnityEngine;
using UnityEngine.UI;

public class _EnemyHealth : MonoBehaviour
{
    [Header("enemy health bar settings")]
    public Slider enemyHealthBar;
    public float startingEnemyHealth = 100;
    public float playerDamage;
    public GameObject parentObject; // This script is on EACH child object of the Enemy


    private void Awake()
    {
        enemyHealthBar.value = startingEnemyHealth;
    }


    #region ENEMY TAKES DAMAGE:

    //This is called in the "TankControl" script when the ENEMY TAKES DAMAGE
    public void EnemyTakesDamage()
    {
        enemyHealthBar.value -= playerDamage;

        if (enemyHealthBar.value <= 0)
        {
            parentObject.SetActive(false);
        }
    }

    #endregion
}
