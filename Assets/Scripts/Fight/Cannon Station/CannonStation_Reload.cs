using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
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
    
    public EventReference reloadCannonSound;
    private EventInstance reloadCannonSoundInstance;

    private CannonStation cannonStation => (CannonStation)ControllerStation;

    private void Start() => SubscribeToInputManager();

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        reloadCannonSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(reloadCannonSound, cannonStation.CannonPivot.gameObject);
    }

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
        
        //play sound
        reloadCannonSoundInstance.start();

        reloadingTween = DOVirtual.DelayedCall(reloadTime, Reload, false);
        
        InvokeSegmentStateChangedEvent();
    }

    private void Reload()
    {
        IsLoaded = true;
        IsReloading = false;
        
        //stop sound
        reloadCannonSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        InvokeSegmentStateChangedEvent();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        reloadingTween?.Kill();
        
        reloadCannonSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        reloadCannonSoundInstance.release();

        if (inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}