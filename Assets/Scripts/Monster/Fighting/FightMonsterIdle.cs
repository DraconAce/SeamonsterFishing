using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightMonsterIdle : AbstractMonsterBehaviour
{
    [SerializeField] private Transform monster;
    [SerializeField] private Vector3 upMovement = new(0,1,0);
    [SerializeField] private MinMaxLimit loopLimits = new MinMaxLimit { MinLimit = 10, MaxLimit = 25 };
    
    [Header("Animation")]
    [SerializeField] private float idleAnimationDuration = 1;
    [SerializeField] private Ease idleEase = Ease.InOutSine;

    private Vector3 originalPosition;
    private Sequence idleSequence;
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Idle;

    public bool IsInNeutralPosition { get; private set; } = true;

    protected override void Start()
    {
        base.Start();

        originalPosition = monster.position;
    }

    protected override IEnumerator StartBehaviourImpl()
    {
        yield return StartCoroutine(StartIdle());
        
        yield return base.StartBehaviourImpl();
    }

    private IEnumerator StartIdle()
    {
        idleSequence = DOTween.Sequence();

        IsInNeutralPosition = false;

        var numberOfLoops = GenerateEvenNumberOfLoops();

        idleSequence.Append(
            monster.DOMove(upMovement, idleAnimationDuration)
                .SetRelative(true)
                .SetEase(idleEase)
            );

        idleSequence.SetLoops(numberOfLoops, LoopType.Yoyo);

        idleSequence.OnStepComplete(() => { IsInNeutralPosition = idleSequence.CompletedLoops() % 2 == 0; });

        yield return idleSequence.WaitForCompletion();
    }

    private int GenerateEvenNumberOfLoops()
    {
        var generatedNumber = (int) Random.Range(loopLimits.MinLimit, loopLimits.MaxLimit);

        if (generatedNumber % 2 != 0)
            generatedNumber++;

        return generatedNumber;
    }

    protected override IEnumerator StartInterruptedRoutineImpl()
    {
        idleSequence?.Kill();

        if (Vector3.SqrMagnitude(monster.position - originalPosition) < 0.00001f) yield break;
        
        var returnToOriginalPosTween = monster.DOMove(originalPosition, idleAnimationDuration)
            .SetEase(idleEase);

        yield return returnToOriginalPosTween.WaitForCompletion();

        yield return base.StartInterruptedRoutineImpl();
    }

    protected override void OnDestroy() => idleSequence?.Kill();
}