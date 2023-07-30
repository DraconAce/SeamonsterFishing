using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject tutorialScreen;
    
    private readonly InputAction anyKey = new ("AnyKey", binding: "/*/<button>", type: InputActionType.Button); 
    
    private PauseManager pauseManager;
    private InputManager inputManager;
    private PlayerSingleton playerSingleton;

    private IEnumerator Start()
    {
        pauseManager = PauseManager.instance;
        inputManager = InputManager.instance;
        playerSingleton = PlayerSingleton.instance;
        
        playerSingleton.DisableMovementControls = true;
        
        yield return new WaitForEndOfFrame();
        ShowTutorialAndPauseGame();
    }

    private void ShowTutorialAndPauseGame()
    {
        anyKey.Enable();
        anyKey.performed += OnContinue;

        pauseManager.ToggleGamePause();

        inputManager.playerInput.currentActionMap.Disable();

        tutorialScreen.SetActive(true);
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        anyKey.performed -= OnContinue;
        anyKey.Disable();
        
        pauseManager.ToggleGamePause();

        inputManager.playerInput.currentActionMap.Enable();
        
        tutorialScreen.SetActive(false);
        
        playerSingleton.DisableMovementControls = false;
    }
}