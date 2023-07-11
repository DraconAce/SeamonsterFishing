using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ReelingStation_Animation : AbstractStationSegment
{
    [SerializeField] private Transform reelingPivotTransform;
    
    [SerializeField] private float reelEntryDuration;
    [SerializeField] private float reelExitDuration;
    [SerializeField] private float returnToPositionDuration = 2f;

    [SerializeField] private Ease reelEntryEase;
    [SerializeField] private Ease reelExitEase;
    [SerializeField] private Ease returnToPositionEase = Ease.InCubic;

    private Transform monsterTransform;
    
    private Vector3 positionBeforeReeling;
    
    private Quaternion initialBoatRotation;
    private Quaternion oppositeBoatRotation;
    
    private Quaternion rotationAfterReeling;

    private Tween moveBoatForReelTween;
    private GameStateManager gameStateManager;

    private const float pathToMonsterPercentage = 0.95f;
    
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;

    private void Start()
    {
        gameStateManager = GameStateManager.instance;
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;

        monsterTransform = FightMonsterSingleton.instance.MonsterTransform;
        
        SetupBoatVariables();
    }

    private void SetupBoatVariables()
    {
        initialBoatRotation = reelingPivotTransform.rotation;
        oppositeBoatRotation = Quaternion.Euler(initialBoatRotation.eulerAngles + Vector3.up * 180);
    }

    private void OnReelingStarted()
    {
        positionBeforeReeling = reelingPivotTransform.position;
        
        ReelingEntryAnimation();
        StartMoveTowardsMonsterTween();
    }

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

    private void StartMoveTowardsMonsterTween()
    {
        var monsterPos = monsterTransform.position;
        
        var toMonsterDirection = (monsterPos - positionBeforeReeling).normalized;
        var targetDistanceToMonster = Vector3.Distance(positionBeforeReeling, monsterPos) * pathToMonsterPercentage;
        
        var targetPosition = positionBeforeReeling + toMonsterDirection * targetDistanceToMonster;

        moveBoatForReelTween = reelingPivotTransform.DOMove(targetPosition, reelingStation.MaxTimeToReel)
            .SetEase(Ease.InQuad);
    }

    private void OnReelingCompleted()
    {
        ReelingExitAnimation();
        
        StartReturnToOriginalPositionTween();
    }

    private void ReelingExitAnimation()
    {
        reelingPivotTransform.DORotateQuaternion(rotationAfterReeling, reelExitDuration)
            .SetEase(reelExitEase)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.FightOverview));
    }

    private void StartReturnToOriginalPositionTween()
    {
        moveBoatForReelTween?.Kill();
        
        moveBoatForReelTween = reelingPivotTransform.DOMove(positionBeforeReeling, returnToPositionDuration)
            .SetEase(returnToPositionEase);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(reelingStation == null) return;
        reelingStation.OnReelingStartedEvent -= OnReelingStarted;
        reelingStation.OnReelingCompletedEvent -= OnReelingCompleted;
    }
}