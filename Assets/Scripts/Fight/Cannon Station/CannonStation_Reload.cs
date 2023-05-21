using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonStation_Reload : AbstractStationSegment, IInputEventSubscriber
{
    [SerializeField] private float reloadTime = 3f;
    
    public bool IsLoaded { get; set; } = true;
    public bool IsReloading { get; private set; }
    public bool CannonIsPrepared => !IsReloading && IsLoaded;
    
    private Tween reloadingTween;

    private InputManager inputManager;
    
    private const string reloadActionName = "Reload";

    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    public string[] ActionsToSubscribeTo { get; } = { reloadActionName };
    
    private void Start() => SubscribeToInputManager();

    protected override void OnGameStateDoesNotMatchCannonStation()
    {
        reloadingTween?.Kill();
        IsReloading = false;
    }

    private void SubscribeToInputManager()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToActions(this);
    }
    
    public void InputPerformed(InputAction.CallbackContext callContext)
    {
        var performedActionName = callContext.action.name;

        if (performedActionName != reloadActionName) return;

        TryToReload();
    }
    
    private void TryToReload()
    {
        if (IsLoaded || IsReloading) return;

        IsReloading = true;

        reloadingTween = DOVirtual.DelayedCall(reloadTime, Reload, false);
        
        InvokeSegmentStateChangedEvent();
    }

    private void Reload()
    {
        IsLoaded = true;
        IsReloading = false;
        
        InvokeSegmentStateChangedEvent();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        reloadingTween?.Kill();

        if (inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}