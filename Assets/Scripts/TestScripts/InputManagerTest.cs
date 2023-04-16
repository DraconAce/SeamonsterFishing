using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerTest : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string actionToSubscribeTo = "Drive";
    
    [SerializeField] private bool subscribeToActionStarted;
    [SerializeField] private bool subscribeToActionPerformed = true;
    [SerializeField] private bool subscribeToActionCanceled;
    
    public bool SubscribedToStarted => subscribeToActionStarted;
    public bool SubscribedToPerformed => subscribeToActionPerformed;
    public bool SubscribedToCanceled => subscribeToActionCanceled;
    
    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToAction(
            new InputManager.SubscriberSettings{ Action = actionToSubscribeTo, EventSubscriber = this });
    }

    public bool SubscriberCanReceiveUpdate() => true;

    public void InputStarted(InputAction.CallbackContext callContext) => Debug.Log("Input Started");

    public void InputCanceled(InputAction.CallbackContext callContext) => Debug.Log("Input Canceled");

    public void InputPerformed(InputAction.CallbackContext callContext) => Debug.Log("Input Performed");
}