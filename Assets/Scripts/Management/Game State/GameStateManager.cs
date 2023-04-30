using System;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    [SerializeField] private GameState currentGameState = GameState.FightOverview;
    
    public bool BlockGameStateChange { get; set; }

    public GameState CurrentGameState
    {
        get => currentGameState;
        private set => currentGameState = value;
    }

    public override void OnCreated()
    {
        base.OnCreated();
        GameStateChanger.instance.Activation();
    }

    public event Action<GameState> GameStateChangedEvent;

    public void ChangeGameState(GameState newGameState)
    {
        if (newGameState == CurrentGameState || BlockGameStateChange) return;

        CurrentGameState = newGameState;
        GameStateChangedEvent?.Invoke(newGameState);
    }
}