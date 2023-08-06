using System;
using System.Collections.Generic;
using System.Xml;
using DG.Tweening;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    [SerializeField] private GameState currentGameState = GameState.FightOverview;
    
    private SceneController sceneController;
    
    private readonly List<GameState> blockChangeExceptionList = new(){GameState.Pause, GameState.Dead, GameState.Won, GameState.MainMenu};
    
    public bool BlockGameStateChange { get; set; }
    public bool GameIsPaused => CurrentGameState == GameState.Pause;
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
    }

    public event Action<GameState> GameStateChangedEvent;

    public void ChangeGameState(GameState newGameState)
    {
        if (newGameState == CurrentGameState || BlockGameStateChange && !IsNewStateInExceptionList(newGameState)) return;

        PreviousGameState = CurrentGameState;
        CurrentGameState = newGameState;
        
        GameStateChangedEvent?.Invoke(newGameState);
    }
    
    private bool IsNewStateInExceptionList(GameState newGameState) => blockChangeExceptionList.Contains(newGameState);

    public void ChangeToPreviousGameState() => ChangeGameState(PreviousGameState);
}