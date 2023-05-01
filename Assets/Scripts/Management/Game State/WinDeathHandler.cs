using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class WinDeathHandler : MonoBehaviour
{
    [SerializeField] private string sceneToReloadOnRestart;
    [SerializeField] private GameState stateAfterReload = GameState.FightOverview;
    
    [Header("Events")]
    [SerializeField] private UnityEvent playerIsDeadEvent;
    [SerializeField] private UnityEvent playerWonEvent;
    
    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        
        SetInitialGameState();

        gameStateManager.GameStateChangedEvent += OnGameStateChanged;
    }

    private void SetInitialGameState()
    {
        Time.timeScale = 1;
        gameStateManager.ChangeGameState(stateAfterReload);
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState == GameState.Won)
            OnRunEnded(playerWonEvent);
        
        if (newGameState != GameState.Dead) return;

        OnRunEnded(playerIsDeadEvent);
    }

    private void OnRunEnded(UnityEvent eventToTrigger)
    {
        Time.timeScale = 0;
        eventToTrigger?.Invoke();
    }

    public void RestartRun() => SceneManager.LoadScene(sceneToReloadOnRestart);
}