using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class AbstractStation : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera stationCamera;
    public CinemachineVirtualCamera StationCamera => stationCamera;

    [SerializeField] private GameState stationGameState = GameState.FightOverview;
    public GameState StationGameState => stationGameState;
    
    [SerializeField] private UnityEvent onEnterStation;

    [SerializeField] private UnityEvent onLeaveStation;

    public bool IsInStation { get; private set; }
    public GameStateManager GameStateManager { get; private set; }
    public UpdateManager UpdateManager { get; private set; }
    private PlayerInputs customPlayerInputs;
    
    public StationManager StationManager { get; private set; }

    public event Action StationGameStateMatchesEvent;
    public event Action StationGameStateDoesNotMatchEvent;

    protected virtual void Start()
    {
        GameStateManager = GameStateManager.instance;
        GameStateManager.GameStateChangedEvent += OnGameStateChanged;

        UpdateManager = UpdateManager.instance;

        StationManager = StationManager.instance;
        StationManager.RegisterStation(this);
        
        customPlayerInputs ??= new();
        
        SetupSegments();
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        if(newGameState is GameState.Pause 
           || GameStateManager.PreviousGameState is GameState.Pause && GameStateManager.CurrentGameState == StationGameState) return;
        
        if (newGameState != StationGameState)
        {
            GameStateDoesNotMatch();
            return;
        }
        
        GameStateMatches();
    }
    
    private void SetupSegments()
    {
        var stationSegments = GetComponentsInChildren<AbstractStationSegment>();
        
        foreach(var segment in stationSegments)
            segment.SetupController(this);
    }

    protected virtual void GameStateDoesNotMatch()
    {
        SetStationCameraActiveState(false);
        
        StationGameStateDoesNotMatchEvent?.Invoke();

        if (!IsInStation) return;
        
        onLeaveStation?.Invoke();
        IsInStation = false;
    }

    protected virtual void GameStateMatches()
    {
        SetStationCameraActiveState(true);
        
        StationGameStateMatchesEvent?.Invoke();

        if (IsInStation) return;
        
        onEnterStation?.Invoke();
        IsInStation = true;
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
    
    public PlayerInputs GetPlayerInputs()
    {
        return customPlayerInputs ??= new();
    }

    protected virtual void OnDestroy()
    {
        if (GameStateManager == null) return;
        
        GameStateManager.GameStateChangedEvent -= OnGameStateChanged;
    }
}