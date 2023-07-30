using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CannonStation_Reload : AbstractStationSegment, IInputEventSubscriber
{
    [SerializeField] private GameObject fuse;
    private Material fuseMat;
    private IEnumerator burnFuseCoroutine;
    private float shootDelayforFuse;
    
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

    private void Start() 
    {
        SubscribeToInputManager();
        //Fetch the Material from the Renderer of the fuse-GameObject
        fuseMat = fuse.GetComponent<Renderer>().sharedMaterial;
        shootDelayforFuse = shootDelayforFuse/100; // set fuse burntime
    } 

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

        //turn fuse back on
        resetFuse();
        
        InvokeSegmentStateChangedEvent();
    }
    
    private void resetFuse()
    {
        fuseMat.SetFloat("_isFuseActive", 0f);
        fuseMat.SetFloat("_Transparancy", 1f);
    }
    
    public void burnFuse() 
    {
        burnFuseCoroutine = DoFuseBurn();
        StartCoroutine(burnFuseCoroutine);
    }
    
    public void SetShootDelayforFuse(float delay)
    {
        shootDelayforFuse = delay/100;
    }
    
    private IEnumerator DoFuseBurn()
    {
        fuseMat.SetFloat("_isFuseActive", 1f);
        shootDelayforFuse -= 0.003f; //slightly reduce the delay (Coroutine and Tween seem to run on different times)
        //fuseTime in %
        for (float fuseTime = 100f; fuseTime >= 0f; fuseTime--)
        {
            fuseMat.SetFloat("_Transparancy", fuseTime/100f);
            yield return new WaitForSeconds(shootDelayforFuse); //100*0.03f = 3 seconds of shoot delay
        }
        fuseMat.SetFloat("_isFuseActive", 0f);
        yield return null;
        //StopCoroutine(burnFuseCoroutine);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        //StopCoroutine(burnFuseCoroutine);
        resetFuse();
        
        reloadingTween?.Kill();
        
        reloadCannonSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        reloadCannonSoundInstance.release();

        if (inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}