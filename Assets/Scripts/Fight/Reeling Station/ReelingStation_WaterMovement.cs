using System;
using DG.Tweening;
using UnityEngine;

public class ReelingStation_WaterMovement : AbstractStationSegment, IManualUpdateSubscriber
{
    [SerializeField] private float waterMovementSpeed = 10f;
    [SerializeField] private Vector3 movementDirection;
    [SerializeField] private Transform waterSurfaceTransform;
    
    [SerializeField] private float reelBeginDuration;
    [SerializeField] private float reelEndDuration;
    
    [SerializeField] private Ease reelBeginEase;
    [SerializeField] private Ease reelEndEase;

    private float movementFactor;
    
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;

    private void Start() => movementDirection = movementDirection.normalized;

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
    }

    private void OnReelingStarted()
    {
        reelingStation.UpdateManager.SubscribeToManualUpdate(this);
        
        AnimateMovementFactor(1, reelBeginDuration, reelBeginEase);
    }
    
    private void OnReelingCompleted()
    {
        AnimateMovementFactor(0, reelEndDuration, reelEndEase, 
            () => reelingStation.UpdateManager.UnsubscribeFromManualUpdate(this));
    }

    private void AnimateMovementFactor(float target, float duration, Ease ease, Action complete = null)
    {
        DOVirtual.Float(movementFactor, target, duration,(value) => movementFactor = value)
            .SetEase(ease)
            .OnComplete(() => complete?.Invoke());
    }

    public void ManualUpdate() => MoveWaterSurface();

    private void MoveWaterSurface()
    {
        waterSurfaceTransform.localPosition += movementDirection * (waterMovementSpeed * movementFactor * Time.deltaTime);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(reelingStation == null) return;
        
        reelingStation.UpdateManager.UnsubscribeFromManualUpdate(this);
    }
}