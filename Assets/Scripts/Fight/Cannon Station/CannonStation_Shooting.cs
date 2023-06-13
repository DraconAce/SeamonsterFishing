using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CannonStation_Shooting : AbstractStationSegment, IInputEventSubscriber
{
    [SerializeField] private float shootDelay = 3f;
    [SerializeField] private float fireForce = 1000f;
    [SerializeField] private Transform barrelOpening;
    [SerializeField] private CannonStation_Reload reloadSegment;
    
    [Header("Pooling")]
    [SerializeField] private Transform cannonBallParent;
    [SerializeField] private GameObject cannonBallPrefab;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onCannonShoot;


    public bool ShootIsScheduled { get; private set; }

    private Tween shootDelayTween;

    private PrefabPool cannonBallPool;
    private CannonBall currentCannonBall;
    private InputManager inputManager;

    private const string shootActionName = "Fire";

    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    public string[] ActionsToSubscribeTo { get; } = { shootActionName };
    
    public FMODUnity.EventReference CannonShotSound;
    public FMODUnity.EventReference CannonFuseSound;
    private FMOD.Studio.EventInstance CannonShotSoundInstance;
    private FMOD.Studio.EventInstance CannonFuseSoundInstance;


    private void Start()
    {
        SubscribeToInputManager();

        PrepareCannonBallPool();
    }

    private void SubscribeToInputManager()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToActions(this);
    }

    private void PrepareCannonBallPool() 
        => cannonBallPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, cannonBallPrefab, cannonBallParent);

    protected override void OnGameStateDoesNotMatchCannonStation()
    {
        if (ShootIsScheduled || currentCannonBall == null) return;
        
        cannonBallPool.ReturnInstance(currentCannonBall.ContainerOfObject);
    }

    public void InputPerformed(InputAction.CallbackContext callContext)
    {
        var performedActionName = callContext.action.name;
        
        if (performedActionName != shootActionName) return;
        
        TryToShoot();
    }

    private void TryToShoot()
    {
        if (!CannonCanShoot()) return;
        
        ScheduleShoot();
    }

    private bool CannonCanShoot() => reloadSegment.CannonIsPrepared && !ShootIsScheduled;

    private void ScheduleShoot()
    {
        var cannonBallOb = cannonBallPool.RequestInstance(barrelOpening.transform.position, barrelOpening);

        cannonBallOb.Ob.transform.localScale = Vector3.one;
        cannonBallOb.TryGetCachedComponent(out currentCannonBall);
        
        //play fuse sound
        CannonFuseSoundInstance = FMODUnity.RuntimeManager.CreateInstance(CannonFuseSound);
        CannonFuseSoundInstance.start();

        shootDelayTween = DOVirtual.DelayedCall(shootDelay, Shoot, false);
        ShootIsScheduled = true;
        
        InvokeSegmentStateChangedEvent();
    }

    private void Shoot()
    {
        currentCannonBall.transform.parent = null;

        var forceVectorCannonBall = barrelOpening.forward * fireForce;
        currentCannonBall.ApplyForceToCannonBall(forceVectorCannonBall);
        
        onCannonShoot?.Invoke();
        
        ShootIsScheduled = false;
        reloadSegment.IsLoaded = false;

        currentCannonBall = null;
        
        //stop and release fuse sound
        CannonFuseSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        CannonFuseSoundInstance.release();
        //play and release shot sound
        CannonShotSoundInstance = FMODUnity.RuntimeManager.CreateInstance(CannonShotSound);
        CannonShotSoundInstance.start();
        CannonShotSoundInstance.release();

        InvokeSegmentStateChangedEvent();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        shootDelayTween?.Kill();

        if (inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}