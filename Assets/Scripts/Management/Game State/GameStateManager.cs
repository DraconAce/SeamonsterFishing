using System;
using System.Xml;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    [SerializeField] private GameState currentGameState = GameState.FightOverview;
    
    public bool BlockGameStateChange { get; set; }
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