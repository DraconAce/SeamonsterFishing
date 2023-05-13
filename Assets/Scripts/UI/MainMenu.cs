using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private SceneController sceneController;

    private void Start() => sceneController = SceneController.instance;
    

    public void LoadFishingScene() => SwitchToScene(Level.Fishing);
    
    public void LoadBaitingScene() => SwitchToScene(Level.Baiting);
    
    public void SwitchToScene(Level levelIdentifier) => sceneController.SwitchToScene(levelIdentifier);
}