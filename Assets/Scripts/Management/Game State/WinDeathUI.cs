using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class WinDeathUI : AbstractMenu
{
    [SerializeField] private float deathDelay = 1f;
    
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

        //prevent player from acting in anticipation of Death-Menu
        OpenMenu();
        //wait for Death-Animation
        StartCoroutine(DoDeathAfterDelay());
    }
    
    private IEnumerator DoDeathAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        //End Run after Death-Animation
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