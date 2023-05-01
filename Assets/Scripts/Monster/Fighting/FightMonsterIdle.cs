using System;
using DG.Tweening;
using UnityEngine;

public class FightMonsterIdle : AbstractMonsterBehaviour
{
    [SerializeField] private Transform monster;
    [SerializeField] private Vector3 upMovement = new(0,1,0);
    
    [Header("Animation")]
    [SerializeField] private float idleAnimationDuration = 1;
    [SerializeField] private Ease idleEase = Ease.InOutSine;

    private Sequence idleSequence;
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Idle;

    public bool IsInNeutralPosition { get; private set; } = true;

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        
        StartIdle();
    }

    private void StartIdle()
    {
        idleSequence = DOTween.Sequence();

        IsInNeutralPosition = false;

        idleSequence.Append(
            monster.DOMove(upMovement, idleAnimationDuration)
                .SetRelative(true)
                .SetEase(idleEase)
            );

        idleSequence.SetLoops(-1, LoopType.Yoyo);

        idleSequence.OnStepComplete(() => { IsInNeutralPosition = idleSequence.CompletedLoops() % 2 == 0; });
    }

    protected override void InterruptBehaviour()
    {
        base.InterruptBehaviour();
        
        idleSequence?.Kill();
    }

    protected override void OnDestroy() => idleSequence?.Kill();
}