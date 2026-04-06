using UnityEngine;
using UnityEngine.UI;

public class _Enemy : MonoBehaviour
{
    public Slider enemyHealthBar;
    public float startingEnemyHealth = 100;
    public float playerDamage;

    private void Awake()
    {
        enemyHealthBar.value = startingEnemyHealth;
    }

    public void EnemyTakesDamage()
    {
        enemyHealthBar.value -= playerDamage;

        if (enemyHealthBar.value <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

}
