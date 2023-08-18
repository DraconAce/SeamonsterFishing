using System.Collections;
using DG.Tweening;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class BigSwipeAttack : AbstractAttackNode
{
    [Header("Big/Side Swipe Sound")]
    [SerializeField] private EventReference SideSwipeSound;
    [SerializeField] private GameObject SoundOrigin;
    private EventInstance SideSwipeSoundInstance;
    
    [Header("Big Swipe Implementation")]
    [SerializeField] private string bigSwipeTriggerL;
    [SerializeField] private string bigSwipeTriggerR;

    [SerializeField] private float movementDelay = 1.5f;
    [SerializeField] private float movementDuration = 2f;
    [SerializeField] private Ease movementEase = Ease.InOutSine;
    [SerializeField] private Transform moveTarget;
    
    private bool bigSwipeAttackEnded;
    
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Transform monsterPivot;

    private Tween movementTween;
    private Tween movementDelayTween;
    
    private WaitUntil bigSwipeAttackEndedWait;
    
    public override MonsterAttackType AttackType => MonsterAttackType.MidRange;

    protected override void Start()
    {
        base.Start();
        
        SideSwipeSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(SideSwipeSound, SoundOrigin);

        targetPosition = moveTarget.position;
        
        monsterPivot = FightMonsterSingleton.instance.MonsterTransform;
        bigSwipeAttackEndedWait = new WaitUntil(() => bigSwipeAttackEnded);
    }

    public override float GetExecutability()
    {
        return 100f;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        bigSwipeAttackEnded = false;
        
        //play side-swipe
        SideSwipeSoundInstance.start();
        
        TriggerLeftOrRightSwipe();
        
        movementDelayTween = DOVirtual.DelayedCall(movementDelay, CacheCurrentPositionAndStartMovement);

        yield return bigSwipeAttackEndedWait;
        
        StartIdleAnimation();
        StartMovementTween(originalPosition, movementDuration);
        
        yield return movementTween.WaitForCompletion();
    }

    private void TriggerLeftOrRightSwipe()
    {
        var bigSwipeTrigger = Random.Range(0, 2) == 0 ? bigSwipeTriggerL : bigSwipeTriggerR;
        
        MonsterAnimationController.SetTrigger(bigSwipeTrigger);
    }

    private void CacheCurrentPositionAndStartMovement()
    {
        originalPosition = monsterPivot.position;
        StartMovementTween(targetPosition, movementDuration);
    }

    private void StartMovementTween(Vector3 targetPos, float duration)
    {
        movementTween?.Kill();
        
        movementTween = monsterPivot.DOMove(targetPos, duration)
            .SetEase(movementEase);
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        movementDelayTween?.Kill();
        
        //Debug.Log("interrupt Tail Sound");
        SideSwipeSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        
        StartIdleAnimation();

        StartMovementTween(originalPosition, movementDuration / 2f);

        yield return movementTween.WaitForCompletion();
    }
    
    protected override void OnAnimationFinishedImpl() => bigSwipeAttackEnded = true;

    protected override void ForceStopBehaviourImpl()
    {
        movementTween?.Kill();
        movementDelayTween?.Kill();
        
        StopSound();
    }

    private void StopSound()
    {
        SideSwipeSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        SideSwipeSoundInstance.release();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        //Destroy sound after Reset
        StopSound();
    }
}