using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightMonsterIdle : AbstractMonsterBehaviour
{
    [SerializeField] private Transform monster;
    [SerializeField] private Vector3 upMovement = new(0,1,0);
    [SerializeField] private MinMaxLimit loopLimits = new (){ MinLimit = 10, MaxLimit = 25 };
    
    [Header("Animation")]
    [SerializeField] private float idleAnimationDuration = 1;
    [SerializeField] private Ease idleEase = Ease.InOutSine;

    private Vector3 originalPosition;
    private Sequence idleSequence;

    protected override MonsterState BehaviourState => MonsterState.Idle;

    protected override void Start()
    {
        base.Start();

        originalPosition = monster.position;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        yield return StartCoroutine(StartIdle());
    }

    private IEnumerator StartIdle()
    {
        idleSequence = DOTween.Sequence();

        var numberOfLoops = GenerateEvenNumberOfLoops();

        idleSequence.Append(
            monster.DOMove(upMovement, idleAnimationDuration)
                .SetRelative(true)
                .SetEase(idleEase)
            );

        idleSequence.SetLoops(numberOfLoops, LoopType.Yoyo);

        yield return idleSequence.WaitForCompletion();
    }

    private int GenerateEvenNumberOfLoops()
    {
        var generatedNumber = (int) Random.Range(loopLimits.MinLimit, loopLimits.MaxLimit);

        if (generatedNumber % 2 != 0)
            generatedNumber++;

        return generatedNumber;
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        idleSequence?.Kill();

        if (Vector3.SqrMagnitude(monster.position - originalPosition) < 0.00001f) yield break;
        
        var returnToOriginalPosTween = monster.DOMove(originalPosition, idleAnimationDuration)
            .SetEase(idleEase);

        yield return returnToOriginalPosTween.WaitForCompletion();
    }

    public override float GetExecutability() => 1f;

    protected override void OnDestroy() => idleSequence?.Kill();
}