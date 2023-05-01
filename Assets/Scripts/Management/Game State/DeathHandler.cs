using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] private string sceneToReloadOnRestart;
    [SerializeField] private GameState stateAfterReload = GameState.FightOverview;
    [SerializeField] private UnityEvent playerIsDeadEvent;
    
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
        if (newGameState != GameState.Dead) return;

        Time.timeScale = 0;
        playerIsDeadEvent?.Invoke();
    }

    public void RestartRun() => SceneManager.LoadScene(sceneToReloadOnRestart);
}