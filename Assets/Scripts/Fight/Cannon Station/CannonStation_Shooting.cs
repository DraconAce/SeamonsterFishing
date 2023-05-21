using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CannonStation_Shooting : AbstractStationController, IInputEventSubscriber
{
    [SerializeField] private float reloadTime = 5f;
    [SerializeField] private float shootDelay = 3f;
    [SerializeField] private float fireForce = 1000f;
    [SerializeField] private Transform barrelOpening;
    
    [Header("Pooling")]
    [SerializeField] private Transform cannonBallParent;
    [SerializeField] private GameObject cannonBallPrefab;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onCannonShoot;
    
    private bool isLoaded = true;
    private bool isReloading;
    private bool shootIsScheduled;

    private Tween shootDelayTween;
    private Tween reloadingTween;

    private PrefabPool cannonBallPool;
    private CannonBall currentCannonBall;
    private InputManager inputManager;

    private const string shootActionName = "Fire";
    private const string reloadActionName = "Reload";

    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    public string[] ActionsToSubscribeTo { get; } = { shootActionName, reloadActionName };

    private void Start()
    {
        SubscribeToInputManager();

        PrepareCannonBallPool();
    }

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();

        ControllerStation.StationGameStateDoesNotMatchEvent += OnGameStateDoesNotMatchCannonStation;
    }

    private void OnGameStateDoesNotMatchCannonStation()
    {
        reloadingTween?.Kill();
        isReloading = false;

        if (shootIsScheduled || !isLoaded || currentCannonBall == null) return;
        
        cannonBallPool.ReturnInstance(currentCannonBall.ContainerOfObject);
    }

    private void SubscribeToInputManager()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToActions(this);
    }

    private void PrepareCannonBallPool() 
        => cannonBallPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, cannonBallPrefab, cannonBallParent);

    public void InputPerformed(InputAction.CallbackContext callContext)
    {
        var performedActionName = callContext.action.name;

        switch (performedActionName)
        {
            case shootActionName:
                TryToShoot();
                break;
            case reloadActionName:
                TryToReload();
                break;
        }
    }

    private void TryToShoot()
    {
        if (!CannonCanShoot()) return;
        
        ScheduleShoot();
    }

    private bool CannonCanShoot() => isLoaded && !isReloading && !shootIsScheduled;

    private void ScheduleShoot()
    {
        var cannonBallOb = cannonBallPool.RequestInstance(barrelOpening.transform.position, barrelOpening);

        cannonBallOb.Ob.transform.localScale = Vector3.one;
        cannonBallOb.TryGetCachedComponent(out currentCannonBall);
        
        shootDelayTween = DOVirtual.DelayedCall(shootDelay, Shoot, false);
        shootIsScheduled = true;
    }

    private void Shoot()
    {
        currentCannonBall.transform.parent = null;

        var forceVectorCannonBall = barrelOpening.forward * fireForce;
        currentCannonBall.ApplyForceToCannonBall(forceVectorCannonBall);
        
        onCannonShoot?.Invoke();
        
        shootIsScheduled = false;
        isLoaded = false;
    }

    private void TryToReload()
    {
        if (isLoaded || isReloading) return;

        isReloading = true;

        reloadingTween = DOVirtual.DelayedCall(reloadTime, Reload, false);
    }

    private void Reload()
    {
        isLoaded = true;
        isReloading = false;
    }

    private void OnDestroy()
    {
        shootDelayTween?.Kill();
        reloadingTween?.Kill();

        if (ControllerStation != null)
            ControllerStation.StationGameStateDoesNotMatchEvent -= OnGameStateDoesNotMatchCannonStation;

        if (inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}