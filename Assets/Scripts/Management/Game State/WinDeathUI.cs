using UnityEngine;
using UnityEngine.Events;

public class WinDeathUI : AbstractMenu
{
    [Header("Events")]
    [SerializeField] private UnityEvent playerIsDeadEvent;
    [SerializeField] private UnityEvent playerWonEvent;
    
    private GameStateManager gameStateManager;
    private SceneController sceneController;

    protected override bool UseInputActions => false;

    protected override void Start()
    {
        base.Start();

        gameStateManager = GameStateManager.instance;
        sceneController = SceneController.instance;
        
        SetInitialTimeScale();

        gameStateManager.GameStateChangedEvent += OnGameStateChanged;
    }

    private void SetInitialTimeScale() => Time.timeScale = 1;

    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState == GameState.Won)
        {
            sceneController.ToggleCursorForLevel(true);

            OpenMenu();
            OnRunEnded(playerWonEvent);
        }

        if (newGameState != GameState.Dead) return;

        sceneController.ToggleCursorForLevel(true);

        OpenMenu();
        OnRunEnded(playerIsDeadEvent);
    }

    private void OnRunEnded(UnityEvent eventToTrigger)
    {
        Time.timeScale = 0;
        eventToTrigger?.Invoke();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (gameStateManager == null) return;
        
        gameStateManager.GameStateChangedEvent -= OnGameStateChanged;
    }
}