using UnityEngine;

public class TutorialMenu : AbstractMenu
{
    [SerializeField] private GameObject pauseMenuButton;
    
    protected override bool UseInputActions => false;
    
    public void OpenTutorialMenu() => OpenMenu();

    public void CloseTutorialMenu() => CloseMenu();

    public void HighlightPauseMenuButton()
    {
        inputManager.EventSystem.SetSelectedGameObject(null);
        inputManager.EventSystem.SetSelectedGameObject(pauseMenuButton);
    }
}