using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class _ObjectPool : MonoBehaviour
{
    [Header("the prefabs in the pool")]
    public GameObject enemyTurretPrefab;


    //EACH OBJECT HAS ITS OWN POOL
    List<GameObject> enemyTurretPool = new List<GameObject>();


    //GETS THE OBJECT FROM THE POOL
    public void Start()
    {
        // CREATE 12 ENEMIES SINCE THAT IS THE MOST THAT WILL BE SPAWNED AT ONCE
        for (int i = 0; i < 12; i++)
        {
            GameObject enemyTurret = Instantiate(enemyTurretPrefab);
            enemyTurret.SetActive(false);
            enemyTurretPool.Add(enemyTurret);
        }
    }


    //GET AN AVAILABLE ENEMY TURRET PREFAB OBJECT
    public GameObject GetEnemyTurret()
    {
        foreach (GameObject enemyTurret in enemyTurretPool)
        {
            if (!enemyTurret.activeSelf)
            {
                enemyTurret.SetActive(true);
                return enemyTurret;
            }
        }

        return null;
    }
}
