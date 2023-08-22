using System;
using UnityEngine;

public class BaseMenuNavigation : MonoBehaviour
{
    [SerializeField] private LevelRepresentation reloadScene;

    private void Start() => SceneController.instance.Activation();

    public void ReturnToMenu()
    {
        GameStateManager.instance.BlockGameStateChangeWithExceptions = false;
        GameStateManager.instance.BlockGameStateChangeWithoutExceptions = false;
        
        SceneController.instance.SwitchToScene(Level.MainMenu);
    }
    
    public void RestartRun()
    {
        GameStateManager.instance.BlockGameStateChangeWithExceptions = false;
        GameStateManager.instance.BlockGameStateChangeWithoutExceptions = false;

        SceneController.instance.SwitchToScene(reloadScene);
    }
    
    public void ShowControls()
    {
        Debug.Log("Show Controls");
    }

    public void QuitGame() => Application.Quit();
}