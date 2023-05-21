using UnityEngine;

public class BaseMenuNavigation : MonoBehaviour
{
    [SerializeField] private LevelRepresentation reloadScene;
    
    public void ReturnToMenu()
    {
        GameStateManager.instance.BlockGameStateChange = false;
        
        SceneController.instance.SwitchToScene(Level.MainMenu);
    }
    
    public void RestartRun()
    {
        GameStateManager.instance.BlockGameStateChange = false;

        SceneController.instance.SwitchToScene(reloadScene);
    }

    public void QuitGame() => Application.Quit();
}