using System;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    public bool GameIsPaused { get; private set; }

    private GameStateManager gameStateManager;

    public override bool AddToDontDestroy => false;

    private void Start() => gameStateManager = GameStateManager.instance;

    public void ToggleGamePause()
    {
        if (gameStateManager.CurrentGameState is GameState.Dead or GameState.Won) return;
        
        SetGamePausedAndTimeScale(!GameIsPaused);
        
        SceneController.ToggleCursorForLevel(GameIsPaused);

        if (GameIsPaused)
        {
            gameStateManager.ChangeGameState(GameState.Pause);
            gameStateManager.BlockGameStateChange = GameIsPaused;
        }
        else
        {
            gameStateManager.BlockGameStateChange = GameIsPaused;
            gameStateManager.ChangeToPreviousGameState();
        }
    }

    private void SetGamePausedAndTimeScale(bool isPaused)
    {
        GameIsPaused = isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        gameStateManager.BlockGameStateChange = false;
        GameIsPaused = false;
    }
}