using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using FMOD.Studio;
using FMODUnity;

public class SwipeAttack : AbstractAttackNode
{
    [Serializable]
    private struct SwipeSetting
    {
        public string SwipeTrigger;
        public float SwipeYRotation;
    }
    
    [Header("Down Swipe Sound")]
    [SerializeField] private EventReference DownSwipeSound;
    [SerializeField] private GameObject SoundOrigin;
    private EventInstance DownSwipeSoundInstance;

    [Header("Swipe Implementation")] 
    [SerializeField] private float moveBackAmount = 2f;
    [SerializeField] private float moveBackDuration;
    [SerializeField] private Ease moveBackEase;
    [SerializeField] private float rotationDuration = 0.75f;
    [SerializeField] private float delayForTailAnimation = 0.5f;
    [SerializeField] private MinMaxLimit numberSwipesLimit;
    [SerializeField] private SwipeSetting[] swipeTriggers;

    private bool swipeFinished;
    private int lastIndex = -1;
    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private WaitUntil waitForSwipeFinished;

    private Transform monsterPivot;
    private Tween moveTween;
    private Tween monsterRotationTween;
    private Tween tailDelayTween;
    private readonly List<int> swipeIndices = new();
    
    private FightMonsterSingleton fightMonsterSingleton;
    
    public override MonsterAttackType AttackType => MonsterAttackType.MidRange;

    protected override void Start()
    {
        base.Start();
        
        DownSwipeSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(DownSwipeSound, SoundOrigin);
        
        waitForSwipeFinished = new WaitUntil(() => swipeFinished);
        
        fightMonsterSingleton = FightMonsterSingleton.instance;
        monsterPivot = fightMonsterSingleton.MonsterTransform;
        
        FillSwipeIndices();
    }
    
    private void FillSwipeIndices()
    {
        for (var i = 0; i < swipeTriggers.Length; i++) swipeIndices.Add(i);
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        //set body hits since last attack
        
        originalPosition = monsterPivot.position;
        originalRotation = monsterPivot.localEulerAngles;
        
        var moveBackPosition = originalPosition - monsterPivot.forward * moveBackAmount;
        StartMoveTween(moveBackPosition);
        
        yield return moveTween.WaitForCompletion();
        
        var numberSwipes = numberSwipesLimit.GetRandomBetweenLimits();
        
        for(var i = 0; i < numberSwipes; i++)
        {
            swipeFinished = false;
            
            //play monster sound
            DownSwipeSoundInstance.start();
            
            TriggerRandomSwipe();
            
            yield return monsterRotationTween.WaitForCompletion();

            yield return waitForSwipeFinished;
        }
        
        ReturnToOriginalPositionAndStartIdle();
        StartMoveTween(originalPosition);
        
        yield return monsterRotationTween.WaitForCompletion();
        
        if (!moveTween.IsActive()) yield break;
        
        yield return moveTween.WaitForCompletion();
    }

    private void StartMoveTween(Vector3 backPosition)
    {
        moveTween?.Kill();
        
        moveTween = monsterPivot.DOMove(backPosition, moveBackDuration)
            .SetEase(moveBackEase);
    }

    private void TriggerRandomSwipe()
    {
        swipeIndices.Remove(lastIndex);
        
        var randomIndex = Random.Range(0, swipeIndices.Count);
        var chosenSwipeIndex = swipeIndices[randomIndex];
        
        var swipeSetting = swipeTriggers[chosenSwipeIndex];
        
        StartMonsterRotationTween(new Vector3(0, swipeSetting.SwipeYRotation, 0));

        tailDelayTween = DOVirtual.DelayedCall(delayForTailAnimation, 
            () => MonsterAnimationController.SetTrigger(swipeSetting.SwipeTrigger));
        
        if(lastIndex >= 0) swipeIndices.Add(lastIndex);
        lastIndex = chosenSwipeIndex;
    }

    private void StartMonsterRotationTween(Vector3 targetRotation)
    {
        monsterRotationTween?.Kill();
        
        var duration = Mathf.Approximately(monsterPivot.localEulerAngles.y, targetRotation.y) ? 0.05f : rotationDuration;
        
        monsterRotationTween = monsterPivot.DOLocalRotate(targetRotation, duration)
            .SetEase(Ease.InOutSine);
    }

    private void ReturnToOriginalPositionAndStartIdle()
    {
        StartIdleAnimation();

        StartMonsterRotationTween(originalRotation);
    }

    protected override void OnAnimationFinishedImpl() => swipeFinished = true;

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        tailDelayTween?.Kill();
        monsterRotationTween?.Kill();
        moveTween?.Kill();
        
        DownSwipeSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        
        ReturnToOriginalPositionAndStartIdle();
        StartMoveTween(originalPosition);

        yield return monsterRotationTween.WaitForCompletion();

        if (!moveTween.IsActive()) yield break;
        
        yield return moveTween.WaitForCompletion();
    }

    protected override void ForceStopBehaviourImpl()
    {
        tailDelayTween?.Kill();
        monsterRotationTween?.Kill();
        moveTween?.Kill();
        
        DownSwipeSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        DownSwipeSoundInstance.release();
        
        foreach(var swipe in swipeTriggers) 
            MonsterAnimationController.UnsetTrigger(swipe.SwipeTrigger);
    }

    #region Executability Calculations
    [Header("Executeability")]
    [SerializeField] private int numberBodyHitsNeededForFullExecutability = 3;
    
    private int bodyHitsOnLastAttack;
    
    public override float GetExecutability()
    {
        var bodyHits = fightMonsterSingleton.NumberOfBodyHits;
        var hitsSinceLastAttack = bodyHits - bodyHitsOnLastAttack;
        
        var executeabilityPercentage = Mathf.Clamp01((float) hitsSinceLastAttack 
                                                    / numberBodyHitsNeededForFullExecutability);
        
        return executability.GetRandomBetweenLimits() * executeabilityPercentage;
    }
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        //Destroy sound after Reset
        DownSwipeSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        DownSwipeSoundInstance.release();
    }
}