public class PauseMenu : AbstractMenu
{
    private PauseManager pauseManager;

    protected override void Start()
    {
        base.Start();
        
        pauseManager = PauseManager.instance;
    }

    protected override void InputStartedImpl() => ToggleGamePause();

    private void ToggleGamePause()
    {
        pauseManager.ToggleGamePause();

        if (pauseManager.GameIsPaused)
        {
            OpenMenu();
            return;
        }

        CloseMenu();
    }
}