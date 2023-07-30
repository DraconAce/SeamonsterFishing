using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MonsterReelingBehaviour : AbstractMonsterBehaviour
{
    [Header("Body Animations")]
    [SerializeField] private float moveDownTarget = 3f;
    [SerializeField] private Transform monsterTransform;
    
    [SerializeField] private float moveDownDuration = 1f;
    [SerializeField] private float turnDuration = 1f;
    
    [SerializeField] private Ease turnEase = Ease.InOutQuad;
    [SerializeField] private Ease moveDownEase = Ease.InOutQuad;

    [Header("Head Animations")] 
    [SerializeField] private Transform headPivot;
    [SerializeField] private Transform mouthPivot;

    [SerializeField] private float headXRotationTarget;
    [SerializeField] private float mouthXRotationTarget;
    
    [SerializeField] private float headRotationDuration = 1f;
    [SerializeField] private float mouthRotationDuration = 1f;
    
    [SerializeField] private Ease headRotationEase = Ease.InOutQuad;
    [SerializeField] private Ease mouthRotationEase = Ease.InOutQuad;

    private WaitUntil waitForReelingStopped;
    private GameStateManager gameStateManager;
    private PlayerSingleton playerSingleton;
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Reeling;
    
    private Vector3 positionBeforeReeling;
    private Quaternion rotationBeforeReeling;
    
    private Quaternion headRotationBeforeReeling;
    private Quaternion mouthRotationBeforeReeling;
    
    private Sequence moveAndTurnSequence;

    protected override void Start()
    {
        base.Start();
        
        gameStateManager = GameStateManager.instance;
        playerSingleton = PlayerSingleton.instance;
        
        waitForReelingStopped = new(() 
            => gameStateManager.CurrentGameState != GameState.FightReelingStation);
    }

    protected override IEnumerator StartBehaviourImpl()
    {
        gameStateManager.ChangeGameState(GameState.FightReelingStation);
        
        StartMoveAndTurnSequence(true);
        AttachHeadMovementToSequence(true);
        
        moveAndTurnSequence.Play();
        
        yield return waitForReelingStopped;
        
        StartMoveAndTurnSequence(false);
        AttachHeadMovementToSequence(false);

        yield return moveAndTurnSequence.WaitForCompletion();

        yield return base.StartBehaviourImpl();
    }

    private void StartMoveAndTurnSequence(bool startReeling)
    {
        var targetPos = monsterTransform.position;
        var targetRotation = monsterTransform.rotation;
        
        if(startReeling)
        {
            positionBeforeReeling = monsterTransform.position;
            rotationBeforeReeling = monsterTransform.rotation;
            
            targetRotation = CalculateLookAtMonsterRotation(targetPos);
            
            targetPos.y += moveDownTarget;
        }
        else
        {
            targetPos = positionBeforeReeling;
            targetRotation = rotationBeforeReeling;
        }

        moveAndTurnSequence = DOTween.Sequence().Append
        (
            monsterTransform.DOMove(targetPos, moveDownDuration)
                .SetEase(moveDownEase)
        ).Join
        (
            monsterTransform.DORotateQuaternion(targetRotation, turnDuration)
                .SetEase(turnEase)
        );
    }

    private Quaternion CalculateLookAtMonsterRotation(Vector3 targetPos)
    {
        var targetEuler = Quaternion.LookRotation(playerSingleton.PlayerTransform.position - targetPos).eulerAngles;

        targetEuler.x = 0;
        targetEuler.z = 0;
        
        return Quaternion.Euler(targetEuler);
    }

    private void AttachHeadMovementToSequence(bool startedReeling)
    {
        var targetHeadRotation = headPivot.localRotation;
        var targetMouthRotation = mouthPivot.localRotation;
        
        if (startedReeling)
        {
            headRotationBeforeReeling = targetHeadRotation;
            mouthRotationBeforeReeling = targetMouthRotation;
            
            targetHeadRotation *= Quaternion.Euler(headXRotationTarget, 0, 0);
            targetMouthRotation *= Quaternion.Euler(mouthXRotationTarget, 0, 0);
        }
        else
        {
            targetHeadRotation = headRotationBeforeReeling;
            targetMouthRotation = mouthRotationBeforeReeling;
        }
        
        moveAndTurnSequence.Join
        (
            headPivot.DOLocalRotateQuaternion(targetHeadRotation, headRotationDuration)
                .SetEase(headRotationEase)
        );
        
        moveAndTurnSequence.Join
        (
            mouthPivot.DOLocalRotateQuaternion(targetMouthRotation, mouthRotationDuration)
                .SetEase(mouthRotationEase)
        );
    }

    protected override IEnumerator StartInterruptedRoutineImpl()
    {
        gameStateManager.ChangeGameState(GameState.FightOverview);
        
        moveAndTurnSequence?.Kill();
        
        StartMoveAndTurnSequence(false);
        yield return moveAndTurnSequence.WaitForCompletion();
    }
}