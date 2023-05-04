using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InGameMenu : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string[] MenuInputActions = { "ToggleMenu" };
    [SerializeField] private GameObject menuContainer;
    [SerializeField] private UnityEvent onMenuOpened;
    [SerializeField] private UnityEvent onMenuClosed;
    
    private GameState previousGameState;
    private CanvasGroup menuGroup;
    
    private SceneController sceneController;
    private GameStateManager gameStateManager;

    private bool gameIsPaused;
    public bool GameIsPaused
    {
        get => gameIsPaused;
        private set
        {
            gameIsPaused = value;

            Time.timeScale = gameIsPaused ? 0 : 1;
        }
    }
    
    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;
    
    public string[] ActionsToSubscribeTo => MenuInputActions;

    private void Start()
    {
        TryGetComponent(out menuGroup);
        
        sceneController = SceneController.instance;
        gameStateManager = GameStateManager.instance;
    }

    public void InputPerformed(InputAction.CallbackContext callContext) => ToggleGamePause();

    private void ToggleGamePause()
    {
        GameIsPaused = !GameIsPaused;

        if (GameIsPaused)
        {
            OpenMenu();
            onMenuOpened?.Invoke();
            
            return;
        }
        
        CloseMenu();
        onMenuClosed?.Invoke();
    }

    private void OpenMenu()
    {
        previousGameState = gameStateManager.CurrentGameState;

        ToggleMenuCanvas(true);

        gameStateManager.ChangeGameState(GameState.Menu);
        
        OpenMenuImpl();
    }

    private void ToggleMenuCanvas(bool activate)
    {
        menuContainer.SetActive(activate);
        menuGroup.alpha = activate ? 1 : 0;
    }
    
    protected virtual void OpenMenuImpl(){}

    protected virtual void CloseMenu()
    {
        ToggleMenuCanvas(false);
        
        gameStateManager.ChangeGameState(previousGameState);
        
        CloseMenuImpl();
    }
    
    protected virtual void CloseMenuImpl(){}


    public void ReturnToMenu() => sceneController.SwitchToScene(Level.MainMenu);
}