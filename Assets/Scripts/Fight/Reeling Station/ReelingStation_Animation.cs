using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ReelingStation_Animation : AbstractStationSegment
{
    [SerializeField] private Transform reelingPivotTransform;
    
    [SerializeField] private float reelEntryDuration;
    [SerializeField] private float reelExitDuration;
    [SerializeField] private float returnToPositionDuration = 2f;

    [SerializeField] private Ease reelEntryEase;
    [SerializeField] private Ease reelExitEase;
    [SerializeField] private Ease returnToPositionEase = Ease.InCubic;

    private Transform reelingTarget;
    
    private Vector3 positionBeforeReeling;
    
    private Quaternion initialBoatRotation;

    private Tween moveBoatForReelTween;
    private GameStateManager gameStateManager;
    private AimConstraint lookAtConstraint;

    private ReelingStation reelingStation => (ReelingStation) ControllerStation;

    private void Start()
    {
        gameStateManager = GameStateManager.instance;

        reelingTarget = FightMonsterSingleton.instance.ReelingTarget;
        
        lookAtConstraint = reelingPivotTransform.GetComponent<AimConstraint>();
        
        SetupBoatVariables();
        SetupLookConstraint();
    }

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
    }

    private void OnReelingStarted()
    {
        positionBeforeReeling = reelingPivotTransform.position;
        
        ReelingEntryAnimation();
        StartMoveTowardsMonsterTween();
    }

    private void ReelingEntryAnimation()
    {
        var reelingTargetPos = GetReelingTargetPositionOnBoatHeight();

        var lookAtRotation = Quaternion.LookRotation(reelingTargetPos - reelingPivotTransform.position);
        
        reelingPivotTransform.DORotateQuaternion(lookAtRotation, reelEntryDuration)
            .SetEase(reelEntryEase)
            .OnComplete(() => lookAtConstraint.constraintActive = true);
    }

    private Vector3 GetReelingTargetPositionOnBoatHeight()
    {
        var reelingTargetPos = reelingTarget.position;
        reelingTargetPos.y = reelingPivotTransform.position.y;
        return reelingTargetPos;
    }

    private void StartMoveTowardsMonsterTween() => moveBoatForReelTween = CreateMoveToMonsterTween();

    private Tween CreateMoveToMonsterTween()
    {
        return DOVirtual.Float(0, 1, reelingStation.MaxTimeToReel, value =>
        {
            var reelingTargetPos = GetReelingTargetPositionOnBoatHeight();

            var toMonsterVector = reelingTargetPos - positionBeforeReeling;
        
            var targetPosition = positionBeforeReeling + toMonsterVector * value;
            
            reelingPivotTransform.position = targetPosition;
        })
            .SetEase(Ease.InQuad);
    }

    private void OnReelingCompleted()
    {
        lookAtConstraint.constraintActive = false;
        
        ReelingExitAnimation();
        
        StartReturnToOriginalPositionTween();
    }

    private void ReelingExitAnimation()
    {
        reelingPivotTransform.DOLocalRotateQuaternion(initialBoatRotation, reelExitDuration)
            .SetEase(reelExitEase)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.FightOverview));
    }

    private void StartReturnToOriginalPositionTween()
    {
        moveBoatForReelTween?.Kill();
        
        moveBoatForReelTween = reelingPivotTransform.DOMove(positionBeforeReeling, returnToPositionDuration)
            .SetEase(returnToPositionEase);
    }

    private void SetupBoatVariables() => initialBoatRotation = reelingPivotTransform.localRotation;

    private void SetupLookConstraint()
    {
        lookAtConstraint.constraintActive = false;
        
        var lookConstrainSource = new ConstraintSource()
        {
            sourceTransform = reelingTarget,
            weight = 1
        };

        lookAtConstraint.AddSource(lookConstrainSource);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(reelingStation == null) return;
        reelingStation.OnReelingStartedEvent -= OnReelingStarted;
        reelingStation.OnReelingCompletedEvent -= OnReelingCompleted;
    }
}