using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public struct SubscriberSettings
    {
        public IInputEventSubscriber EventSubscriber;
        
        public string[] ActionsToSubscribeTo;
    }

    [Serializable]
    private class GameStateInputMap : ListDictProxy<GameState, string>
    {
        [SerializeField] private GameState state;
        public override GameState Key => state;

        [SerializeField] private string stateInputMap;
        public override string Value => stateInputMap;
    }

    [SerializeField] private List<GameStateInputMap> inputMapsList;

    public PlayerInput playerInput { get; private set; }

    private GameStateManager gameStateManager;
    private Dictionary<GameState, string> inputMapsLookup = new();
    private readonly Dictionary<string, List<IInputEventSubscriber>> inputActionAndSubscribers = new();

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;

        CreateInputMapsLookup();
    }

    private void OnActionTriggered(InputAction.CallbackContext callbackContext)
    {
        if (!inputActionAndSubscribers.TryGetValue(callbackContext.action.name, out var subscriberList)) return;
        
        NotifySubscribers(subscriberList, callbackContext);
    }

    private void CreateInputMapsLookup() 
        => inputMapsLookup = ListDictProxy<GameState, string>.CreateDictionaryFromList(new List<ListDictProxy<GameState, string>>(inputMapsList));

    private void Start()
    {
        gameStateManager = GameStateManager.instance;

        gameStateManager.GameStateChangedEvent += ChangeToInputMapOfState;
    }

    private void ChangeToInputMapOfState(GameState state)
    {
        if (!inputMapsLookup.TryGetValue(state, out var mapName))
            return;

        playerInput.SwitchCurrentActionMap(mapName);
    }

    private void NotifySubscribers(List<IInputEventSubscriber> subscriberList, InputAction.CallbackContext callbackContext)
    {
        foreach (var subscriber in subscriberList) 
            TriggerSubscriberContextFunction(callbackContext, subscriber);
    }

    private void TriggerSubscriberContextFunction(InputAction.CallbackContext callbackContext, IInputEventSubscriber subscriber)
    {
        if (!subscriber.SubscriberCanReceiveUpdate()) return;
        
        if (callbackContext.started && subscriber.SubscribedToStarted)
            subscriber.InputStarted(callbackContext);
        if (callbackContext.performed && subscriber.SubscribedToPerformed)
            subscriber.InputPerformed(callbackContext);
        if (callbackContext.canceled && subscriber.SubscribedToCanceled)
            subscriber.InputCanceled(callbackContext);
    }

    public void SubscribeToActions(SubscriberSettings subscriberSettings)
    {
        foreach(var action in subscriberSettings.ActionsToSubscribeTo)
        {
            if (inputActionAndSubscribers.TryGetValue(action, out var subscriberList))
            {
                subscriberList.Add(subscriberSettings.EventSubscriber);
                continue;
            }

            inputActionAndSubscribers.Add(action,
                new() { subscriberSettings.EventSubscriber });
        }
    }

    public void UnsubscribeFromActions(SubscriberSettings subscriberSettings)
    {
        foreach(var action in subscriberSettings.ActionsToSubscribeTo)
        {
            if (!inputActionAndSubscribers.TryGetValue(action, out var subscriberList))
                continue;

            subscriberList.Remove(subscriberSettings.EventSubscriber);
        }
    }

    private void OnDestroy()
    {
        playerInput.onActionTriggered -= OnActionTriggered;

        if (gameStateManager == null) return;
        gameStateManager.GameStateChangedEvent -= ChangeToInputMapOfState;
    }
}