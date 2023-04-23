using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbstractStation : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera stationCamera;
    public CinemachineVirtualCamera StationCamera => stationCamera;

    [SerializeField] private GameState stationGameState = GameState.FightOverview;
    public GameState StationGameState => stationGameState;

    public GameStateManager GameStateManager { get; private set; }
    public UpdateManager UpdateManager { get; private set; }
    public PlayerInputs CustomPlayerInputs { get; private set; }

    public event Action StationGameStateMatchesEvent;
    public event Action StationGameStateDoesNotMatchEvent;

    protected virtual void Start()
    {
        GameStateManager = GameStateManager.instance;
        GameStateManager.GameStateChangedEvent += OnGameStateChanged;

        UpdateManager = UpdateManager.instance;
        
        CustomPlayerInputs = new();
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState != StationGameState)
        {
            GameStateDoesNotMatch();
            return;
        }
        
        GameStateMatches();
    }

    protected virtual void GameStateDoesNotMatch()
    {
        SetStationCameraActiveState(false);
        
        StationGameStateDoesNotMatchEvent?.Invoke();
    }

    protected virtual void GameStateMatches()
    {
        SetStationCameraActiveState(true);
        
        StationGameStateMatchesEvent?.Invoke();
    }

    private void SetStationCameraActiveState(bool gameStateMatches)
    {
        if (StationCamera == null) return;
        
        if (gameStateMatches)
        {
            if (StationCamera.isActiveAndEnabled) return;
            
            StationCamera.gameObject.SetActive(true);
            
            return;
        }
        
        if (!StationCamera.isActiveAndEnabled) return;
        
        StationCamera.gameObject.SetActive(false);
        
    }

    public bool GameStateMatchesStationGameState() => GameStateManager.CurrentGameState == StationGameState;

    protected virtual void OnDestroy()
    {
        if (GameStateManager == null) return;
        
        GameStateManager.GameStateChangedEvent -= OnGameStateChanged;
    }
}