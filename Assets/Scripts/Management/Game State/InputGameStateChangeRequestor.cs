using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputGameStateChangeRequestor : Singleton<InputGameStateChangeRequestor>, IInputEventSubscriber
{
    [Serializable]
    private class GameStateInputActionMap : ListDictProxy<string, GameState>
    {
        [SerializeField] private string actionName;
        public override string Key => actionName;
        
        [SerializeField] private GameState state;
        public override GameState Value => state;
    }

    [SerializeField] private List<GameStateInputActionMap> gameStateInputMapsList;

    private Dictionary<string, GameState> gameStateInputActionMapLookup;
    private string[] gameStateChangingActions;

    private GameStateManager gameStateManager;
    private InputManager inputManager;

    #region Update and Input Manager Variables
    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;
    public string[] ActionsToSubscribeTo => gameStateChangingActions;

    private InputManager.SubscriberSettings inputSubscriberSettings;
    #endregion

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        inputManager = InputManager.instance;
        
        CreateGameStateInputActionLookup();
        CreateInputSubscribeSettings();
        
        inputManager.SubscribeToActions(inputSubscriberSettings);
    }

    private void CreateGameStateInputActionLookup()
    {
        gameStateInputActionMapLookup =
            ListDictProxy<string, GameState>.CreateDictionaryFromList(
                new List<ListDictProxy<string, GameState>>(gameStateInputMapsList));
    }

    private void CreateInputSubscribeSettings()
    {
        gameStateChangingActions = gameStateInputActionMapLookup.Keys.ToArray();

        inputSubscriberSettings = new(){ ActionsToSubscribeTo = this.ActionsToSubscribeTo, EventSubscriber = this};
    }

    public void InputPerformed(InputAction.CallbackContext callContext) => RequestGameStateChangeBasedOnInput(callContext);

    private void RequestGameStateChangeBasedOnInput(InputAction.CallbackContext callContext)
    {
        var performedActionName =  callContext.action.name;
        
        SetGameStateBasedOnPerformedAction(performedActionName);
    }

    private void SetGameStateBasedOnPerformedAction(string actionName)
    {
        if (!gameStateInputActionMapLookup.TryGetValue(actionName, out var requestedState))
            return;
        
        gameStateManager.ChangeGameState(requestedState);
    }

    private void OnDestroy()
    {
        if (inputManager == null) return;
        
        inputManager.UnsubscribeFromActions(inputSubscriberSettings);
    }
}