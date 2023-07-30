using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AbstractViewDirectionControls : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string[] actionsToSubscribeTo = {"Change_View_Left", "Change_View_Right"};
    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    public ViewDirectionHandler ViewDirectionHandler { get; private set; }

    private InputManager inputManager;
    private PlayerSingleton playerSingleton;
    
    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;

    public bool SubscribedToCanceled => false;
    
    public abstract PlayerDevice ControlDevice { get; }

    protected virtual void Start()
    {
        ViewDirectionHandler = GetComponent<ViewDirectionHandler>();
        
        playerSingleton = PlayerSingleton.instance;
        inputManager = InputManager.instance;

        ManageInputSubscriptionBasedOnMatchingDevice();
        inputManager.InputDeviceChangedEvent += ManageInputSubscriptionBasedOnMatchingDevice;
    }

    private void ManageInputSubscriptionBasedOnMatchingDevice()
    {
        ToggleTrackInputForView(inputManager.LatestDevice == ControlDevice);
    }
    
    protected virtual void ToggleTrackInputForView(bool trackInput)
    {
        if(trackInput)
            inputManager.SubscribeToActions(this);
        else
            inputManager.UnsubscribeFromActions(this);
    }
    
    public void InputPerformed(InputAction.CallbackContext callContext) => CheckViewInput(callContext);

    private void CheckViewInput(InputAction.CallbackContext callContext) => CheckViewInput(callContext.action.name);

    protected void CheckViewInput(string actionName)
    {
        if (playerSingleton.DisableMovementControls) return;

        ChangeViewBasedOnInput(actionName);
    }

    protected abstract void ChangeViewBasedOnInput(string actionName);
    
    private void OnDestroy()
    {
        if (inputManager == null) return;
        
        UnsubscribeOnDestroy();
    }
    
    public void UnsubscribeOnDestroy()
    {
        inputManager.UnsubscribeFromActions(this);
        
        inputManager.InputDeviceChangedEvent -= ManageInputSubscriptionBasedOnMatchingDevice;
    }
}