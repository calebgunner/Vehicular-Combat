using UnityEngine;
using System.Collections.Generic;

public class _WaveControl : MonoBehaviour
{
    //1. SPAWN A CERTAIN NUMBER AT EACH WAVE
    //2. TWEAK HOW QUICLKLY A FEW OF THEM SHOOT
    //3. TWEAK THE HEALTH OF OTHERS
    //4. TWEAK THE DAMAGE OF OTHERS
    //GIVE LIFE HALFWAY IN WAVE AMOUNT
    // MAKE MINI MAP (CIRCLES = ENEMIES.... TRAINGLE = PLAYER)

    //SPAWNPOINT POSITIONS
    public Transform[] spawnPoints;
    public int amountOfEnemies;
    bool enemiesSpawned = false;
    [Space]
    public GameObject EnemyTurrets; //use a random cube for now

    //REFERENCES FROM OTHER SCRIPTS
    _GameCanvas gC;

    void Start()
    {
        gC = GameObject.FindWithTag("Player").GetComponent<_GameCanvas>();

        gC.waveNumber = 1;
    }


    void Update()
    {
        if (!enemiesSpawned) //IF ENEMIES HAVE NOT BEEN SPAWNED...
        {
            SpawnEnemies(); //remove when implementing waves

        /*if(gC.waveNumber == 1)
        {
            //Activate one particular ENERY CLOSE TO THE PLAYER THAT ISN'T RANDOMISED TO EXPLAIN THE WAVE SYSTEM

        }
        
        if (gC.waveNumber == 2)
        {
            
        }

        if (gC.waveNumber == 3)
        {
            
        }


        */

            enemiesSpawned = true;
        }
    }

    void SpawnEnemies()
    {
        //MAKE A TEMPORARY LIST USING ALL OF OUR SPAWN POINTS
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        //REPEAT THE CODE INSIDE THESE BRACKETS UNTIL WE'VE SPAWNED THE AMOUNT OF ENEMIES WE ASKED FOR
        for (int i = 0; i < amountOfEnemies; i++)
        {
            //CHOOSE A RANDOM SPAWN POINT FROM THE AVAILABLE SPAWN POINTS
            int randomSpawnPoint = Random.Range(0, availableSpawnPoints.Count);

            //GET THE TRANSFORM OF THE CHOSEN SPAWN POINT
            Transform chosenSpawnPoint = availableSpawnPoints[randomSpawnPoint];

            //SPAWN THE ENEMY AT THE CHOSEN SPAWN POINT
            Instantiate(EnemyTurrets, chosenSpawnPoint.position, chosenSpawnPoint.rotation);

            //REMOVE THIS SPAWN POINT SO IT CANNOT BE CHOSEN AGAIN THIS WAVE
            availableSpawnPoints.RemoveAt(randomSpawnPoint);
        }
    }
}
