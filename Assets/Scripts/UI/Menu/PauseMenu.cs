using UnityEngine;

public class PauseMenu : AbstractMenu
{
    private GameStateManager gameStateManager;

    private bool gameIsPaused;
    public bool GameIsPaused
    {
        get => gameIsPaused;
        private set
        {
            gameIsPaused = value;

            Time.timeScale = gameIsPaused ? 0 : 1;
        }
    }

    protected override void Start()
    {
        gameStateManager = GameStateManager.instance;
        
        base.Start();
    }

    protected override void InputStartedImpl() => ToggleGamePause();

    private void ToggleGamePause()
    {
        if (gameStateManager.CurrentGameState is GameState.Dead or GameState.Won) return;
        
        GameIsPaused = !GameIsPaused;
        
        gameStateManager.BlockGameStateChange = GameIsPaused;
        
        SceneController.ToggleCursorForLevel(GameIsPaused);

        if (GameIsPaused)
        {
            OpenMenu();
            return;
        }

        CloseMenu();
    }

    protected override void OpenMenuImpl() => gameStateManager.ChangeGameState(GameState.PauseMenu);

    protected override void CloseMenuImpl()
    {
        
        gameStateManager.ChangeToPreviousGameState();
    }

    protected override void OnDestroy()
    {
        gameStateManager.BlockGameStateChange = false;
        GameIsPaused = false;
        base.OnDestroy();
    }
}