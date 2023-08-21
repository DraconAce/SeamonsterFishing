using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum PlayerDevice
{
    Gamepad = 0,
    KeyboardMouse = 1,
    Other = 2
}

public class InputManager : Singleton<InputManager>
{
    [Serializable]
    private class GameStateInputMap : ListDictProxy<GameState, string>
    {
        [SerializeField] private GameState state;
        public override GameState Key => state;

        [SerializeField] private string stateInputMap;
        public override string Value => stateInputMap;
    }

    private class ActionEvent
    {
        public event Action<InputAction.CallbackContext> StartedAction;
        public event Action<InputAction.CallbackContext> PerformedAction;
        public event Action<InputAction.CallbackContext> CanceledAction;

        public void InvokeStarted(InputAction.CallbackContext context) => StartedAction?.Invoke(context);
        public void InvokePerformed(InputAction.CallbackContext context) => PerformedAction?.Invoke(context);
        public void InvokeCanceled(InputAction.CallbackContext context) => CanceledAction?.Invoke(context);
    }
    
    [SerializeField] private List<GameStateInputMap> inputMapsList;

    public PlayerInput playerInput { get; private set; }
    
    private EventSystem eventSystem;
    public EventSystem EventSystem 
    {
        get
        {
            if (eventSystem != null) return eventSystem;
                
            eventSystem = FindFirstObjectByType<EventSystem>();
            
            return eventSystem;
        }
    }

    private GameStateManager gameStateManager;
    private Dictionary<GameState, string> inputMapsLookup = new();
    private readonly Dictionary<string, ActionEvent> inputActionAndActionEvents = new();

    public PlayerDevice LatestDevice { get; private set; } = PlayerDevice.KeyboardMouse;
    private bool blockPlayerInput;
    
    public event Action InputDeviceChangedEvent;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += OnActionTriggered;

        CreateInputMapsLookup();
    }

    private void OnActionTriggered(InputAction.CallbackContext callbackContext)
    {
        if(blockPlayerInput) return;
        
        DetermineUsedDevice(callbackContext);
        
        if (!inputActionAndActionEvents.TryGetValue(callbackContext.action.name, out var actionEvent)) return;

        NotifySubscribers(callbackContext, actionEvent);
    }

    private void DetermineUsedDevice(InputAction.CallbackContext callbackContext)
    {
        var usedDevice = callbackContext.control.device;
        
        var formerDevice = LatestDevice;

        LatestDevice = usedDevice switch
        {
            Gamepad => PlayerDevice.Gamepad,
            Keyboard => PlayerDevice.KeyboardMouse,
            Mouse => PlayerDevice.KeyboardMouse,
            _ => PlayerDevice.Other
        };
        
        if (formerDevice == LatestDevice) return;
        
        InputDeviceChangedEvent?.Invoke();
    }

    private void NotifySubscribers(InputAction.CallbackContext callbackContext, ActionEvent triggeredActionEvent)
    {
        if (callbackContext.started)
            triggeredActionEvent.InvokeStarted(callbackContext);
        if (callbackContext.performed)
            triggeredActionEvent.InvokePerformed(callbackContext);
        if (callbackContext.canceled)
            triggeredActionEvent.InvokeCanceled(callbackContext);
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

    public void SubscribeToActions(IInputEventSubscriber subscriber)
    {
        foreach(var action in subscriber.ActionsToSubscribeTo)
        {
            if (inputActionAndActionEvents.TryGetValue(action, out var actionEvent))
            {
                AddSubscriberToSuitableEvents(subscriber, actionEvent);

                continue;
            }

            var newActionEvent = new ActionEvent();
            
            AddSubscriberToSuitableEvents(subscriber, newActionEvent);
            
            inputActionAndActionEvents.Add(action, newActionEvent);
        }
    }

    private void AddSubscriberToSuitableEvents(IInputEventSubscriber subscriber, ActionEvent actionEvent)
    {
        if (subscriber.SubscribedToStarted)
            actionEvent.StartedAction += subscriber.InputStarted;
        if (subscriber.SubscribedToPerformed)
            actionEvent.PerformedAction += subscriber.InputPerformed;
        if (subscriber.SubscribedToStarted)
            actionEvent.CanceledAction += subscriber.InputCanceled;
    }

    public void UnsubscribeFromActions(IInputEventSubscriber subscriber)
    {
        foreach(var action in subscriber.ActionsToSubscribeTo)
        {
            if (!inputActionAndActionEvents.TryGetValue(action, out var actionEvent))
                continue;

            RemoveSubscriberFromSuitableEvents(subscriber, actionEvent);
        }
    }
    
    private void RemoveSubscriberFromSuitableEvents(IInputEventSubscriber subscriber, ActionEvent actionEvent)
    {
        if (subscriber.SubscribedToStarted)
            actionEvent.StartedAction -= subscriber.InputStarted;
        if (subscriber.SubscribedToPerformed)
            actionEvent.PerformedAction -= subscriber.InputPerformed;
        if (subscriber.SubscribedToStarted)
            actionEvent.CanceledAction -= subscriber.InputCanceled;
    }
    
    public void SetInputBlocked(bool isBlocked) => blockPlayerInput = isBlocked;
    
    public bool IsPlayerUsingController => LatestDevice == PlayerDevice.Gamepad;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        playerInput.onActionTriggered -= OnActionTriggered;

        if (gameStateManager == null) return;
        gameStateManager.GameStateChangedEvent -= ChangeToInputMapOfState;
    }
}