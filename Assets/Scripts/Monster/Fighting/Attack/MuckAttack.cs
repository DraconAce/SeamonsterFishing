using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MuckAttack : AbstractAttackNode
{
    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private Ease rotationEase = Ease.InOutSine;
    [SerializeField] private MuckSpewController muckSpewController;
    
    public override MonsterAttackType AttackType => MonsterAttackType.LongRange;

    private readonly WaitForSeconds stopBehaviourBufferWait = new(0.5f);

    private bool muckAttackEnded;
    private WaitUntil muckAttackEndedWait;

    private readonly WaitForSeconds muckEndedBuffer = new(1f);
    
    private Quaternion originalRotation;
    private Tween monsterRotationTween;
    
    private Transform playerPos;
    private Transform monsterPivot;

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
        
        monsterAnimationController.SetTrigger(idleAnimationTrigger);
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
        muckSpewController.StopMuckRoutine();
        
        monsterRotationTween?.Kill();
        
        StartReturnToOriginalRotationTween();

        monsterAnimationController.SetTrigger(idleAnimationTrigger);
        
        yield return monsterRotationTween.WaitForCompletion();

        yield return stopBehaviourBufferWait;
    }

    private void StartReturnToOriginalRotationTween()
    {
        monsterRotationTween = monsterPivot.DORotateQuaternion(originalRotation, rotationDuration)
            .SetEase(rotationEase);
    }
    
    public override float GetExecutability() => 100f;
    
    public void TriggerMuckEnd() => muckAttackEnded = true;
}