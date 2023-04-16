using System;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    public GameState CurrentGameState { get; private set; }
    public event Action GameStateChangedEvent;
    
    public void ChangeGameState(GameState newGameState){}
}