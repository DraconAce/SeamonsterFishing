using UnityEngine.InputSystem;

public interface IInputEventSubscriber
{
    public bool SubscribedToStarted => true;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    public string[] ActionsToSubscribeTo { get; }

    public void UnsubscribeOnDestroy();
    
    public void InputStarted(InputAction.CallbackContext callContext){}
    public void InputPerformed(InputAction.CallbackContext callContext){}
    public void InputCanceled(InputAction.CallbackContext callContext){}
}