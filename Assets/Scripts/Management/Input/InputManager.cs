using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : Singleton<InputManager>
{
    public struct SubscriberSettings
    {
        public IInputEventSubscriber EventSubscriber;
        
        public string Action;
    }

    public PlayerInput playerInput { get; private set; }

    private readonly Dictionary<string, List<IInputEventSubscriber>> inputActionAndSubscribers = new();

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnActionTriggered(InputAction.CallbackContext callbackContext)
    {
        if (!inputActionAndSubscribers.TryGetValue(callbackContext.action.name, out var subscriberList)) return;
        
        NotifySubscribers(subscriberList, callbackContext);
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

    public void SubscribeToAction(SubscriberSettings subscriberSettings)
    {
        if (inputActionAndSubscribers.TryGetValue(subscriberSettings.Action, out var subscriberList))
        {
            subscriberList.Add(subscriberSettings.EventSubscriber);
            return;
        }
        
        inputActionAndSubscribers.Add(subscriberSettings.Action, 
            new(){ subscriberSettings.EventSubscriber });
    }

    public void UnsubscribeFromAction(SubscriberSettings subscriberSettings)
    {
        if(!inputActionAndSubscribers.TryGetValue(subscriberSettings.Action, out var subscriberList)) return;

        subscriberList.Remove(subscriberSettings.EventSubscriber);
    }

    private void OnDestroy() => playerInput.onActionTriggered -= OnActionTriggered;
}