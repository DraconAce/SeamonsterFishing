using System.Collections.Generic;
using UnityEngine.InputSystem;

public interface IInputEventSubscriber
{
    public bool SubscribedToStarted { get; }
    public bool SubscribedToPerformed { get; }
    public bool SubscribedToCanceled { get; }

    public string[] ActionsToSubscribeTo { get; }
    
    public void InputStarted(InputAction.CallbackContext callContext){}
    public void InputPerformed(InputAction.CallbackContext callContext){}
    public void InputCanceled(InputAction.CallbackContext callContext){}
    public bool SubscriberCanReceiveUpdate() => true;
}