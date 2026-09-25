using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _GameCanvas : MonoBehaviour
{
    #region INSPECTOR VALUES:

    [Header("ammo control and ui")]
    public GameObject primaryAmmoBar;
    Slider primaryAmmoSlider;
    public Image primaryBarFill;
    public Color bothShotColour;
    public Color primaryShotColour;
    [Space]
    public Color reloadBarColor;

    // 20 divided by 4 seconds
    public float growthRate;

    [Header("reticle control")]
    public Image reticleImage;
    public Color gunReloadingColor;

    [Header("player health")]
    public Slider playerHealthBar;
    [Range(0f, 100f)] public float playerHealthPoints;

    [Header("health boost settings")]
    public bool boostEffectIsActive;
    public Image healthBarFill;
    public Color redColour;
    public Color greenColour;

    [Space]
    public Material tankMaterial;

    [Header("player damage bar")]
    public Slider damageBar;
    [Range(0f, 100f)] public float damageBarPoints;

    [Header("death effect")]
    public Transform explosionPosition;
    public GameObject explosionEffect;

    [Header("dodge indicator")]
    public GameObject dodgeIndicator;

    [Header("wave text ui")]
    public TextMeshProUGUI waveText;
    public int waveNumber;

    [Header("ui menu management")]
    public GameObject PauseMenu;
    public TextMeshProUGUI pauseMenuWaveText;
    public Button firstSelectedButton;
    bool gameIsPaused;
    [Space]
    public GameObject DeathMenu;
    public TextMeshProUGUI deathMenuWaveText;
    public Button restartSelectedButton;
    bool playerIsDead;
    bool deathTriggered;
    [Space]
    public GameObject MissionSuccessMenu;
    public Button playAgainButtonSelected;
    public TextMeshProUGUI CompletionTimeText;
    public float elapsedTime = 0f;
    public bool isTimerRunning = true; // This controls the timer status
    public GameObject MissionSuccessText;
    [Space]
    public GameObject[] gameplayTips;
    public bool gameplayTipsActive;
    [Space]
    public GameObject GameplayNotifications;

    [Header("gameplay UI to turn off")]
    public GameObject[] gameplayUI;
    public Image crosshairImage;
    public Canvas miniMapCanvas;

    [Space]
    _TankControl tC;
    _CameraImpulseShake cIS;
    _ControllerRumble cR;
    public PlayerInput playerInput;

    #endregion


    private void Awake()
    {
        Time.timeScale = 1f;

        //UI MANAGEMENT
        gameIsPaused = false;
        playerIsDead = false;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);        // Set the active button
        deathTriggered = false;

        WaveTextControl();

        healthBarFill.color = redColour;
        tankMaterial.DisableKeyword("_EMISSION");
        ResetTimer();

        //SET THE FPS
        Application.targetFrameRate = 60;

        //REFERENCE OBJECTS
        tC = GameObject.FindWithTag("Player").GetComponent<_TankControl>();
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();

        //SET SLIDER VALUE
        playerHealthPoints = damageBarPoints = 100f;
        playerHealthBar.value = damageBar.value = playerHealthPoints;

        primaryAmmoSlider = primaryAmmoBar.GetComponent<Slider>();
    }


    void Update()
    {
        //CCROSSHAIR COLOUR CHANGE WHEN RELOADING
        if (tC.reloading1)
            reticleImage.color = gunReloadingColor;
        else
            reticleImage.color = Color.white;

        if (gameplayTips[0].activeInHierarchy || gameplayTips[1].activeInHierarchy || gameplayTips[2].activeInHierarchy)
        {
            gameplayTipsActive = true;
            gameIsPaused = false; //prevents player from pausing
        }

        growthRate = 40f / 3.25f;

        //AMMO SLIDER ACTIVATION
        primaryAmmoBar.SetActive(true);

        //AMMO BAR CONTROL
        AmmoSlider();

        //IF THE TIMER IS STOPPED, EXIT THE UPDATE LOOP EARLY
        if (!isTimerRunning) return;

        TimerControl(); //Start and control the timer

        //WAVE TEXT
        WaveTextControl();

        //CONTROL PLAYER HEALTH
        playerHealthBar.value = playerHealthPoints;
        damageBar.value = damageBarPoints;

        //BOOST EFFECT (VISUAL INDICATOR OF HEALTH BOOST
        if (boostEffectIsActive) StartCoroutine(HealthBarColourChange());

        //CONTROL DODGE INDICATOR
        dodgeIndicator.SetActive(tC.canDodge);
            
        //SELECT THE RESTART BUTTON WHEN THE DEATH SCREEN IS ACTIVATED
        if (playerIsDead)
        {
            DeathMenu.SetActive(true);
            PauseMenu.SetActive(false);
        }
        else
        {
            DeathMenu.SetActive(false);
        }
            

        PlayerDies();
    }


    #region PRIMARY AMMO SLIDER

    void AmmoSlider()
    {
        //AMMO SLIDER CONTROL
        primaryAmmoSlider.value = tC.primaryAmmoAmnt;
        primaryAmmoSlider.maxValue = tC.primaryAmmoMax;

        //RELOAD RIMER
        if (tC.reloading1) ReloadTimer_Primary();

        // AMMO SLIDER COLOUR
        if (tC.reloading1) primaryBarFill.color = reloadBarColor;
        else
        {

            if(tC.primaryAmmoAmnt >= tC.secondaryFireAmmo) //AMMO ALLOWS BOTH PRIMARY AND SECONDARY FIRE
                primaryBarFill.color = bothShotColour;
            else
                primaryBarFill.color = primaryShotColour; //AMMO ONLY ENOUGH FOR PRIMARY SHOT

        }
    }

    #endregion


    #region WAVE TEXT CONTROL

    void WaveTextControl()
    {
        // WAVE TEXT IN-GAME
        if (waveNumber == 10)
            waveText.text = "final assault";
        else
            waveText.text = "assault 0" + waveNumber;

        // WAVE TEXT IN PAUSE MENU
        if (waveNumber == 10)
            pauseMenuWaveText.text = "final assault";
        else
            pauseMenuWaveText.text = "assault 0" + waveNumber + "/10";

        // WAVE TEXT IN DEATH SCREEN
        if (waveNumber == 10)
            deathMenuWaveText.text = "assault reached: " + waveNumber + "/10";
        else
            deathMenuWaveText.text = "assault reached: 0" + waveNumber + "/10";

    }

    #endregion


    #region TIMER:
    void TimerControl()
    {
        // Accumulate time
        elapsedTime += Time.deltaTime;

        // Calculate and format mm:ss
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        CompletionTimeText.text = $"clear time: {minutes:D2}:{seconds:D2}";
    }


    // Call this function from another script or a UI Button to STOP the timer
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // Call this function to START or RESUME the timer
    public void StartTimer()
    {
        isTimerRunning = true;
    }

    // Call this function to RESET the timer back to zero
    public void ResetTimer()
    {
        elapsedTime = 0f;
        CompletionTimeText.text = "00:00";
    }

    #endregion


    #region RELOAD TIME:

    void ReloadTimer_Primary()
    {
        if (tC.reloading1)
        {
            // Add 6.666 units multiplied by deltaTime every second
            tC.primaryAmmoAmnt += growthRate * Time.deltaTime;

            // Clamp it so it doesn't overshoot 20 due to framerate math
            tC.primaryAmmoAmnt = Mathf.Min(tC.primaryAmmoAmnt, tC.primaryAmmoMax);


            //CHECK AFTER WE HAVE INCREASED THE AMMO
            if (tC.primaryAmmoAmnt >= tC.primaryAmmoMax)
            {
                tC.reloading1 = false;
            }
        }
    }

    #endregion


    void PlayerDies()
    {
        if (playerHealthPoints <= 0 && !deathTriggered)
        {
            deathTriggered = true;

            // Remove the game object
            GameObject.FindWithTag("Player").SetActive(false);

            // SCREEN SHAKE for theexplosion
            cIS.ScreenShake(Vector3.up, 0.8f, 0.6f, CinemachineImpulseDefinition.ImpulseShapes.Explosion);

            //CONTROLLER VIBRATION WHEN THERE'S AN EXPLOSION
            cR.Rumble(0.8f, 0.4f, 0.5f);

            // Add the explosion effect
            GameObject spawnedInstance = Instantiate(explosionEffect, explosionPosition.position, Quaternion.identity);

            //Activate the death menu
            StartCoroutine(ActivateDeathMenu());
        }
    }


    IEnumerator ActivateDeathMenu()
    {
        yield return new WaitForSeconds(3.5f);

        //ACTIVATE DEATH MENU
        playerIsDead = true;

        EventSystem.current.SetSelectedGameObject(restartSelectedButton.gameObject); // Set the active button
    }


    //FIND A WAY TO MAKE ALL OF THOSE TRANSPARENT INSTEAD OF DISABLYING THEM!!!!!!
    public IEnumerator ActivateMissionSuccess()
    {
        // PLAY MISSION SUCCES SOUND

        //ACTIVATE MISSION SUCCESS TEXT
        MissionSuccessText.SetActive(true);

        // TURN THIS OFFF (ONLY TURN OFF PARTS OF THE GAMEOBJECT AND NOT THE GAME OBJECTS THEMSELVES)
        for (int i = 0; i <= 5; i++)
        {
            gameplayUI[i].SetActive(false);
        }

        waveText.enabled = false;
        crosshairImage.enabled = false;

        // WAIT A SECON AND A HALF
        yield return new WaitForSeconds(2f);

        // ACTIVATE MISSION SUCCESS MENU
        MissionSuccessMenu.SetActive(true);

        // ACTIVATE THE START BUTTON FOR THE MENU
        EventSystem.current.SetSelectedGameObject(playAgainButtonSelected.gameObject); // Set the active button
    }


    // REDUCE PLAYER HEALTH
    public IEnumerator ReduceHealth(float damageToPlayer)
    {
        // Reduce player health
        playerHealthPoints = Mathf.Max(playerHealthPoints - damageToPlayer, 0f);

        yield return new WaitForSeconds(0.35f);

        // Reduce player damage bar
        damageBarPoints = Mathf.Max(damageBarPoints - damageToPlayer, 0f);
    }


    // HEALTH FILL COLOUR CHANGE AND EMISSION COLOUR CHANGE (PLAYER GLOWS WHEN THEY RECEIVE HEALTH)
    IEnumerator HealthBarColourChange()
    {
        //Change health bar colour to green
        healthBarFill.color = greenColour;

        //turn emission on
        tankMaterial.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(0.6f);

        healthBarFill.color = redColour;
        tankMaterial.DisableKeyword("_EMISSION");
        boostEffectIsActive = false;
    }


    #region u.i. buttons:

    public void PauseGame(InputAction.CallbackContext context)
    {
        if (context.performed && !gameIsPaused)
        {
            PauseMenu.SetActive(true);
            gameIsPaused = true;
            StopTimer();

            playerInput.SwitchCurrentActionMap("UI");
            Time.timeScale = 0f; //FREEZE THE GAME WHEN PAUSED
        }
    }

    public void ResumeButton()
    {
        PauseMenu.SetActive(false);
        gameIsPaused = false;
        StartTimer();

        playerInput.SwitchCurrentActionMap("Player");
        Time.timeScale = 1f;
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //RESTART THE CURRENT SCENE
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    #endregion


    public void RemoveGameplayTips(InputAction.CallbackContext context)
    {
        if (context.performed && gameplayTipsActive)
        {
            // CLOSE ALL GAMEPLAY TIPS
            gameplayTips[0].SetActive(false);
            gameplayTips[1].SetActive(false);
            gameplayTips[2].SetActive(false);

            // RESET THE GAMEPLAY TIP BOOLEAN
            gameplayTipsActive = false;

            // UN-FREEZE THE GAME
            Time.timeScale = 1f;
        }
    }
}
