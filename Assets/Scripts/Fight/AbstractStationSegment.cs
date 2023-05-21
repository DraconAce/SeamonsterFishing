using System;
using UnityEngine;

public abstract class AbstractStationSegment : MonoBehaviour
{
    public event Action SegmentStateChanged;
    protected AbstractStation ControllerStation { get; private set; }
    

    public void SetupController(AbstractStation station)
    {
        ControllerStation = station;
        
        OnControllerSetup();
    }

    protected virtual void OnControllerSetup()
    {
        ControllerStation.StationGameStateMatchesEvent += OnGameStateMatchesCannonStation;

        ControllerStation.StationGameStateDoesNotMatchEvent += OnGameStateDoesNotMatchCannonStation;
    }
    
    protected virtual void OnGameStateMatchesCannonStation(){}
    protected virtual void OnGameStateDoesNotMatchCannonStation(){}

    protected virtual void OnDestroy()
    {
        if (ControllerStation == null) return;
        
        ControllerStation.StationGameStateMatchesEvent -= OnGameStateMatchesCannonStation;

        ControllerStation.StationGameStateDoesNotMatchEvent -= OnGameStateDoesNotMatchCannonStation;
    }

    protected void InvokeSegmentStateChangedEvent() { SegmentStateChanged?.Invoke(); }
}