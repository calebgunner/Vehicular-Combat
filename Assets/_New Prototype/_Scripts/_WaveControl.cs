using UnityEngine;
using System.Collections.Generic;

public class _WaveControl : MonoBehaviour
{
    //SPAWNPOINT POSITIONS
    public Transform[] spawnPoints;
    int amountOfEnemies;
    bool enemiesSpawned = false;
    [Space]
    public GameObject EnemyTurrets; //use a random cube for now
    [Space]
    public GameObject healthBoost;

    //REFERENCES FROM OTHER SCRIPTS
    _GameCanvas gC;

    void Start()
    {
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();

        gC.waveNumber = 0;
    }


    void Update()
    {
        if (!enemiesSpawned) //IF ENEMIES HAVE NOT BEEN SPAWNED...
        {
            WaveSettings();
        }
    }


    void WaveSettings()
    {
        switch (gC.waveNumber)
        {
            //TUTORIAL
            case 0:     
                //SPAWN one particular ENEMY TURRET CLOSE TO THE PLAYER THAT ISN'T RANDOMISED TO EXPLAIN THE WAVE SYSTEM and ENEMY ATTACK PATTERN
                Instantiate(EnemyTurrets, spawnPoints[19].position, spawnPoints[19].rotation);
                break;


            //WAVE 1
            case 1:
                amountOfEnemies = 3; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 2
            case 2:
                amountOfEnemies = 4; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 3
            case 3:
                amountOfEnemies = 5; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                healthBoost.SetActive(true); //ACTIVATE THE HEALTH BOOST ICON
                SpawnEnemies();
                break;


            //WAVE 4
            case 4:
                amountOfEnemies = 6; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 5
            case 5:
                amountOfEnemies = 7; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                healthBoost.SetActive(true); //ACTIVATE THE HEALTH BOOST ICON
                SpawnEnemies();
                break;


            //WAVE 6
            case 6:
                amountOfEnemies = 8; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 7
            case 7:
                amountOfEnemies = 9; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 8
            case 8:
                amountOfEnemies = 10; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                healthBoost.SetActive(true); //ACTIVATE THE HEALTH BOOST ICON
                SpawnEnemies();
                break;


            //WAVE 9
            case 9:
                amountOfEnemies = 11; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;


            //WAVE 10
            case 10:
                amountOfEnemies = 12; //SET THE AMOUNT OF ENEMIES SPAWNED FOR THE WAVE
                SpawnEnemies();
                break;

        }

        enemiesSpawned = true;
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
