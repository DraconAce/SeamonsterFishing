using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SwipeAttack : AbstractAttackNode
{
    [Serializable]
    private struct SwipeSetting
    {
        public string SwipeTrigger;
        public float SwipeYRotation;
    }

    [Header("Swipe Implementation")]
    [SerializeField] private float rotationDuration = 0.75f;
    [SerializeField] private float delayForTailAnimation = 0.5f;
    [SerializeField] private MinMaxLimit numberSwipesLimit;
    [SerializeField] private SwipeSetting[] swipeTriggers;

    private bool swipeFinished;
    private WaitUntil waitForSwipeFinished;

    private Transform monsterPivot;
    private Tween monsterRotationTween;
    private Tween tailDelayTween;
    
    public override MonsterAttackType AttackType => MonsterAttackType.MidRange;

    protected override void Start()
    {
        base.Start();
        
        waitForSwipeFinished = new WaitUntil(() => swipeFinished);
        
        monsterPivot = FightMonsterSingleton.instance.MonsterTransform;
    }

    public override float GetExecutability()
    {
        return 100f;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        var numberSwipes = numberSwipesLimit.GetRandomBetweenLimits();
        
        for(var i = 0; i < numberSwipes; i++)
        {
            swipeFinished = false;
            
            TriggerRandomSwipe();
            
            yield return monsterRotationTween.WaitForCompletion();

            yield return waitForSwipeFinished;
        }
        
        ReturnToOriginalPositionAndStartIdle();
        
        yield return monsterRotationTween.WaitForCompletion();
    }

    private void TriggerRandomSwipe()
    {
        var randomIndex = Random.Range(0, swipeTriggers.Length);
        var swipeSetting = swipeTriggers[randomIndex];
        
        StartMonsterRotationTween(swipeSetting.SwipeYRotation);

        tailDelayTween = DOVirtual.DelayedCall(delayForTailAnimation, 
            () => MonsterAnimationController.SetTrigger(swipeSetting.SwipeTrigger));
    }

    private void StartMonsterRotationTween(float yRotation)
    {
        var targetRotation = new Vector3(0, yRotation, 0);
        
        monsterRotationTween?.Kill();
        
        var duration = Mathf.Approximately(monsterPivot.localEulerAngles.y, yRotation) ? 0.05f : rotationDuration;
        
        monsterRotationTween = monsterPivot.DOLocalRotate(targetRotation, duration)
            .SetEase(Ease.InOutSine);
    }

    private void ReturnToOriginalPositionAndStartIdle()
    {
        StartIdleAnimation();

        StartMonsterRotationTween(0);
    }

    protected override void OnAnimationFinishedImpl() => swipeFinished = true;

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        tailDelayTween?.Kill();
        monsterRotationTween?.Kill();
        
        ReturnToOriginalPositionAndStartIdle();

        yield return monsterRotationTween.WaitForCompletion();
    }
}