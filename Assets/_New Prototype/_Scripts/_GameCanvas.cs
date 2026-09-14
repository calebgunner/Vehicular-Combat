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

    [Header("reticle control")]
    public Image reticleImage;

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
    [Space]
    public GameObject WaveBar;
    Slider waveBar_Slider;

    [Header("ui menu management")]
    public GameObject PauseMenu;
    public Button firstSelectedButton;
    bool gameIsPaused;
    [Space]
    public GameObject DeathMenu;
    public Button restartSelectedButton;
    bool playerIsDead;
    bool deathTriggered;

    [Space]
    _TankControl tControl;
    _CameraImpulseShake cIS;
    _ControllerRumble cR;
    public PlayerInput playerInput;

    #endregion


    private void Awake()
    {
        //UI MANAGEMENT
        gameIsPaused = false;
        playerIsDead = false;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);        // Set the active button
        deathTriggered = false;
        waveText.text = "wave 0" + waveNumber;
        healthBarFill.color = redColour;
        tankMaterial.DisableKeyword("_EMISSION");

        //SET THE FPS
        Application.targetFrameRate = 60;

        //REFERENCE OBJECTS
        tControl = GameObject.FindWithTag("Player").GetComponent<_TankControl>();
        cIS = GameObject.FindWithTag("Player").GetComponent<_CameraImpulseShake>();
        cR = GameObject.FindWithTag("Player").GetComponent<_ControllerRumble>();

        //SET SLIDER VALUE
        playerHealthPoints = damageBarPoints = 100f;
        playerHealthBar.value = damageBar.value = playerHealthPoints;
    }


    void Update()
    {
        //WAVE TEXT
        waveText.text = "wave 0" + waveNumber;

        //CONTROL PLAYER HEALTH
        playerHealthBar.value = playerHealthPoints;
        damageBar.value = damageBarPoints;

        //BOOST EFFECT (VISUAL INDICATOR OF HEALTH BOOST
        if (boostEffectIsActive) StartCoroutine(HealthBarColourChange());

        //CONTROL DODGE INDICATOR
        dodgeIndicator.SetActive(tControl.canDodge);
            
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

            playerInput.SwitchCurrentActionMap("UI");
            Time.timeScale = 0f; //FREEZE THE GAME WHEN PAUSED
        }
    }

    public void ResumeButton()
    {
        PauseMenu.SetActive(false);
        gameIsPaused = false;

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
}
