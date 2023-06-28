using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ReelingStation_Animation : AbstractStationSegment
{
    [SerializeField] private Transform reelingPivotTransform;
    
    [SerializeField] private float reelEntryDuration;
    [SerializeField] private float reelExitDuration;

    [SerializeField] private Ease reelEntryEase;
    [SerializeField] private Ease reelExitEase;

    private Transform monsterTransform;
    
    private Quaternion initialBoatRotation;
    private Quaternion oppositeBoatRotation;
    
    private Quaternion rotationAfterReeling;
    private GameStateManager gameStateManager;
    
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;

        monsterTransform = FightMonsterSingleton.instance.transform;
        
        SetupBoatVariables();
    }

    private void SetupBoatVariables()
    {
        initialBoatRotation = reelingPivotTransform.rotation;
        oppositeBoatRotation = Quaternion.Euler(initialBoatRotation.eulerAngles + Vector3.up * 180);
    }

    private void OnReelingStarted() => ReelingEntryAnimation();

    private void OnReelingCompleted() => ReelingExitAnimation();

    private void ReelingEntryAnimation()
    {
        rotationAfterReeling = DetermineCloserBoatRotation(reelingPivotTransform.rotation);
        
        var lookAtRotation = Quaternion.LookRotation(monsterTransform.position - reelingPivotTransform.position);
        
        reelingPivotTransform.DORotateQuaternion(lookAtRotation, reelEntryDuration)
            .SetEase(reelEntryEase);
    }

    private Quaternion DetermineCloserBoatRotation(Quaternion currentBoatRotation)
    {
        var angleInitial = CalculateAngleBetweenRotations(initialBoatRotation, currentBoatRotation);
        var angleOpposite = CalculateAngleBetweenRotations(oppositeBoatRotation, currentBoatRotation);
        
        return angleInitial < angleOpposite ? initialBoatRotation : oppositeBoatRotation;
    }
    
    private float CalculateAngleBetweenRotations(Quaternion rotation1, Quaternion rotation2)
    {
        var angle = Quaternion.Angle(rotation1, rotation2);
        return angle;
    }
    
    private void ReelingExitAnimation()
    {
        reelingPivotTransform.DORotateQuaternion(rotationAfterReeling, reelExitDuration)
            .SetEase(reelExitEase)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.FightOverview));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(reelingStation == null) return;
        reelingStation.OnReelingStartedEvent -= OnReelingStarted;
        reelingStation.OnReelingCompletedEvent -= OnReelingCompleted;
    }
}