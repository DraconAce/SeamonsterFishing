using UnityEngine;

public class MainMenu : AbstractMenu
{
    private SceneController sceneController;

    protected override bool UseInputActions => false;

    protected override void Start()
    {
        base.Start();
        sceneController = SceneController.instance;
    }
    
    public void LoadFishingScene() => SwitchToScene(Level.Fishing);
    
    public void LoadBaitingScene() => SwitchToScene(Level.Baiting);
    
    public void SwitchToScene(Level levelIdentifier) => sceneController.SwitchToScene(levelIdentifier);
}