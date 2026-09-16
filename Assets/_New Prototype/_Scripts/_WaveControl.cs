using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class _WaveControl : MonoBehaviour
{
    // wave > wave complete > transition perdiod > new wave

    [Header("spawnpoint positions")]
    public Transform[] spawnPoints;
    int amountOfEnemies;
    bool enemiesSpawned = false;
    [Space]
    public int enemyCount;
    public bool updateEnemyCount = false;
    public GameObject EnemyTurrets; //use a random cube for now
    [Space]
    public GameObject healthBoost;

    [Header("wave control")]
    public bool transitionPhase;
    public float transitionTime = 0f;
    public float targetTransitionTime = 5f;
    [Space]
    public bool startTransitionTimer;
    public bool healthBoostAvailable;

    GameObject WaveBar;
    Slider waveBar_Slider;

    //REFERENCES FROM OTHER SCRIPTS
    _GameCanvas gC;

    void Start()
    {
        //SET THE REFERENCES
        gC = GameObject.FindWithTag("PlayerCanvas").GetComponent<_GameCanvas>();
        WaveBar = GameObject.FindWithTag("WaveBarUI");
        waveBar_Slider = WaveBar.GetComponent<Slider>();

        //SET START VALUES
        WaveBar.SetActive(false);
        gC.waveNumber = 0;
        healthBoostAvailable = true;

        //TUTORIAL STARTS WITH ONE PREDETERMINED ENEMY
        amountOfEnemies = 1;
        updateEnemyCount = true; //just to start the updatTheEnemyCount function
        UpdateTheEnemyCount();
    }


    void Update()
    {
        //LINK THE WAVE BAR TO THE TRANSITION PHASE
        waveBar_Slider.maxValue = targetTransitionTime;
        waveBar_Slider.value = transitionTime;


        //BEGIN THE TRANSITION PHASE ONCE THE ENEMY-COUNT IS ZERO
        TheTransitionPhse();


        if (!enemiesSpawned) //IF ENEMIES HAVE NOT BEEN SPAWNED...
        {
            WaveSettings();

            UpdateTheEnemyCount();
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


            default:
                //GAME COMPLETE
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

        //////////////////////////////////////

        //SET THE ENEMYCOUNT BOOLEAN TO TRUE TO ENABLE THE ENEMY COUNT CHANGE
        updateEnemyCount = true;
    }


    void UpdateTheEnemyCount() //Update the enemy count EVERY TIME THE WAVE CHANGES
    {
        if (updateEnemyCount)
        {
            //Set the enemy count to the amount of enemies spawn in a wave
            enemyCount = amountOfEnemies;

            //Set the boolean flase to stop update from continuously updating
            updateEnemyCount = false;
        }
    }

    void TheTransitionPhse()
    {
        //BEGIN THE TRANSITION PHASE ONCE THE ENEMY-COUNT IS ZERO
        if (enemyCount == 0 && enemiesSpawned)
        {
            transitionPhase = true;

            WaveBar.SetActive(true); //Activate Wave Bar to should progress
        }


        if (transitionPhase)
        {
            //AFTER WAVES 2, 4 AND 8, ACTIVATE THE HEALTH BOOST
            if ((gC.waveNumber == 2 || gC.waveNumber == 4 || gC.waveNumber == 8) && healthBoostAvailable)
            {
                healthBoost.SetActive(true); //ACTIVATE THE HEALTH BOOST ICON before these levels start
            }


            //AFTER WAVES 2, 4 AND 8, WAIT UNTIL THE HEALTH BOOST HAS BEEN TAKEN BEFORE THE TIMER STARTS
            if ((gC.waveNumber != 2 && gC.waveNumber != 4 && gC.waveNumber != 8) || startTransitionTimer) //boost effect shows when the healthboost has been taken so it works for this
            {
                transitionTime += Time.deltaTime; //add to the timer

                transitionTime = Mathf.Min(transitionTime, targetTransitionTime); //Clamp it so it doesn't overshoot YOUR TARGET TIME
            }


            //ONCE THE TRANSITION PERIOD IS FINISHED...
            if (transitionTime >= targetTransitionTime)
            {
                transitionPhase = false; //END THE TRANSITION PHASE
                transitionTime = 0f; //RESET THE TIMER FOR THE NEXT TRANSITION

                WaveBar.SetActive(false);

                gC.waveNumber++; //MOVE TO THE NEXT WAVE
                enemiesSpawned = false; //ALLOW THE NEXT WAVE TO SPAWN

                startTransitionTimer = false; //RESET FOR THE NEXT SPECIAL WAVE
                healthBoostAvailable = true;
            }
        }
    }
}
