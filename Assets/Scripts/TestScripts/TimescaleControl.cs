using System;
using UnityEngine;

public class TimescaleControl : MonoBehaviour
{
    [SerializeField] private float timescale = 1f;
    [SerializeField] private bool enableTimeScale = true;
    [SerializeField] private bool triggerDeath;

    private void Update()
    {
        if (triggerDeath)
        {
            GameStateManager.instance.ChangeGameState(GameState.Dead);
            triggerDeath = false;
        }
        
        var currentTimeScale = Time.timeScale;
        
        if(Mathf.Approximately(timescale, currentTimeScale) || !enableTimeScale) return;
        
        Time.timeScale = timescale;
    }
}
