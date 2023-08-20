using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MuckAttack : AbstractAttackNode
{
    [Header("Muck Attack Implementation")]
    [SerializeField] private MuckSpewController muckSpewController;
    
    [Header("Face Player Rotation")]
    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private Ease rotationEase = Ease.InOutSine;

    private bool muckAttackEnded;

    private Quaternion originalRotation;
    private Tween monsterRotationTween;
    private Transform playerPos;
    private Transform monsterPivot;

    private WaitUntil muckAttackEndedWait;
    private readonly WaitForSeconds muckEndedBuffer = new(1f);

    public override MonsterAttackType AttackType => MonsterAttackType.LongRange;

    protected override void Start()
    {
        base.Start();
        
        playerPos = PlayerSingleton.instance.PhysicalPlayerRepresentation;
        monsterPivot = FightMonsterSingleton.instance.MonsterTransform;
        
        muckAttackEndedWait = new WaitUntil(() => muckAttackEnded);
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        muckAttackEnded = false;
        monsterRotationTween?.Kill();
     
        StartFacePlayerTween();
        
        muckSpewController.StartMuckRoutine();

        yield return muckAttackEndedWait;
        
        yield return muckEndedBuffer;

        StartIdleAnimation();
        
        StartReturnToOriginalRotationTween();
        
        yield return monsterRotationTween.WaitForCompletion();
    }

    private void StartFacePlayerTween()
    {
        originalRotation = monsterPivot.rotation;
        
        var lookAtPlayerRotation = Quaternion.LookRotation(playerPos.position - monsterPivot.position).eulerAngles;
        lookAtPlayerRotation.x = 0f;
        lookAtPlayerRotation.z = 0f;
        
        monsterRotationTween = monsterPivot.DORotate(lookAtPlayerRotation, rotationDuration)
            .SetEase(rotationEase);
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        StopRotationAndMuckRoutine();

        StartReturnToOriginalRotationTween();

        StartIdleAnimation();
        
        yield return monsterRotationTween.WaitForCompletion();
    }

    private void StopRotationAndMuckRoutine()
    {
        muckSpewController.StopMuckRoutine();
        monsterRotationTween?.Kill();
    }

    private void StartReturnToOriginalRotationTween()
    {
        monsterRotationTween = monsterPivot.DORotateQuaternion(originalRotation, rotationDuration)
            .SetEase(rotationEase);
    }
    
    public override float GetExecutability() => executability.GetRandomBetweenLimits();
    
    public void TriggerMuckAttackEnd() => muckAttackEnded = true;

    protected override void ForceStopBehaviourImpl() => StopRotationAndMuckRoutine();
}