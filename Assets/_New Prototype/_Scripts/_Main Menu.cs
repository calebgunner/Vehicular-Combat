using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _MainMenu : MonoBehaviour
{
    [Header("ui menu management")]
    public Button firstSelectedButton;
    PlayerInput playerInput;


    private void Awake()
    {
        //UI MANAGEMENT
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);        // Set the active button
        playerInput = GetComponent<PlayerInput>();

        // Disables gameplay controls, enables UI navigation
        playerInput.SwitchCurrentActionMap("UI");
    }


    #region u.i. buttons:

    public void StartButton()
    {
        SceneManager.LoadScene("Prototype"); //RESTART THE CURRENT SCENE
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT THE GAME");
    }

    #endregion
}
