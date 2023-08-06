using System;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    public bool GameIsPaused { get; private set; }

    private GameStateManager gameStateManager;
    private SceneController sceneController;

    public override bool AddToDontDestroy => false;

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        sceneController = SceneController.instance;
    }

    public event Action<bool> GamePausedStateChangedEvent;

    public void ToggleGamePause()
    {
        if (gameStateManager.CurrentGameState is GameState.Dead or GameState.Won) return;
        
        SetGamePausedAndTimeScale(!GameIsPaused);

        CheckCursorVisibleState();

        if (GameIsPaused)
        {
            GamePausedStateChangedEvent?.Invoke(true);
            gameStateManager.ChangeGameState(GameState.Pause);
            gameStateManager.BlockGameStateChange = GameIsPaused;
        }
        else
        {
            GamePausedStateChangedEvent?.Invoke(false);
            gameStateManager.BlockGameStateChange = GameIsPaused;
            gameStateManager.ChangeToPreviousGameState();
        }
    }

    private void CheckCursorVisibleState()
    {
        var showCursor = GameIsPaused || !sceneController.CurrentLevelRepresentation.HideCursorOnLoad;
        sceneController.ToggleCursorForLevel(showCursor);
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