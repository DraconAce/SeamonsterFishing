using System.Collections.Generic;
using UnityEngine;

public class StationManager : Singleton<StationManager>
{
    public Dictionary<GameState, AbstractStation> Stations { get; } = new();

    public void RegisterStation(AbstractStation station)
    {
        var stationGameState = station.StationGameState;
        
        if (Stations.ContainsKey(stationGameState)) return;
        
        Stations.Add(stationGameState, station);
    }

    public AbstractStation GetStationOfGameState(GameState stationGameState) 
        => !Stations.ContainsKey(stationGameState) ? null : Stations[stationGameState];
}