using System;
using System.Xml;
using DG.Tweening;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    [SerializeField] private GameState currentGameState = GameState.FightOverview;
    
    private SceneController sceneController;
    
    public bool BlockGameStateChange { get; set; }
    public bool GameIsPaused => CurrentGameState == GameState.PauseMenu;
    public GameState PreviousGameState { get; private set; }

    public GameState CurrentGameState
    {
        get => currentGameState;
        private set => currentGameState = value;
    }

    public override void OnCreated()
    {
        base.OnCreated();

        PreviousGameState = currentGameState;
        
        InputGameStateChangeRequestor.instance.Activation();
    }

    private void Start()
    {
        sceneController = SceneController.instance;
        
        sceneController.SceneStarted(currentGameState);
        
        //Todo: Remove this
        DOVirtual.DelayedCall(1f, () => GameStateChangedEvent?.Invoke(currentGameState));
    }

    public event Action<GameState> GameStateChangedEvent;

    public void ChangeGameState(GameState newGameState)
    {
        if (newGameState == CurrentGameState || BlockGameStateChange) return;

        PreviousGameState = CurrentGameState;
        CurrentGameState = newGameState;
        
        GameStateChangedEvent?.Invoke(newGameState);
    }

    public void ChangeToPreviousGameState() => ChangeGameState(PreviousGameState);
}