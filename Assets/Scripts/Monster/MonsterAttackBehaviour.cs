using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackBehaviour : AbstractMonsterBehaviour
{
    //Todo: Function to chose next attack

    private AbstractMonsterAttack currentAttack;
    private readonly WaitForSeconds waitBetweenMidRangeAttacks = new(5);

    private readonly Dictionary<MonsterAttackType, List<AbstractMonsterAttack>> monsterAttacks = new();
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Attacking;

    protected override void Start()
    {
        base.Start();
        
        GenerateMonsterAttacksFromChildren();
    }

    private void GenerateMonsterAttacksFromChildren()
    {
        var allMonsterAttacks = GetComponentsInChildren<AbstractMonsterAttack>();

        foreach (var attack in allMonsterAttacks)
        {
            var attackType = attack.AttackType;

            if (monsterAttacks.TryGetValue(attackType, out var monsterAttack))
            {
                monsterAttack.Add(attack);
                continue;
            }
            
            monsterAttacks.Add(attack.AttackType, new(){ attack });
        }
    }
    
    protected override IEnumerator StartBehaviourImpl()
    {
        yield return StartCoroutine(AttackRoutine());
        
        yield return base.StartBehaviourImpl();
    }

    private IEnumerator AttackRoutine()
    {
        // three mid range attacks
        for (var i = 0; i < 3; i++)
        {
            yield return StartAndWaitForAttackToFinish(MonsterAttackType.MidRange);
            yield return waitBetweenMidRangeAttacks;
        }

        // one long range attack
        //yield return StartAndWaitForAttackToFinish(MonsterAttackType.LongRange);

        // one short range attack
        //yield return StartAndWaitForAttackToFinish(MonsterAttackType.ShortRange);
    }

    private IEnumerator StartAndWaitForAttackToFinish(MonsterAttackType attackType)
    {
        var randomAttack = GetAndStartRandomAttackOfType(attackType);

        yield return null;

        currentAttack = randomAttack;
        
        currentAttack.TriggerBehaviour();
        yield return randomAttack.WaitForCompletionOrInterruption;
    }

    private AbstractMonsterAttack GetAndStartRandomAttackOfType(MonsterAttackType attackType)
    {
        if (!monsterAttacks.TryGetValue(attackType, out var attacksOfRequestedType)) return null;

        var randomAttackIndex = Random.Range(0, attacksOfRequestedType.Count);

        var randomAttack = attacksOfRequestedType[randomAttackIndex];

        return randomAttack;
    }

    protected override IEnumerator StartInterruptedRoutineImpl()
    {
        yield return StopCurrentAttack();
        
        yield return base.StartInterruptedRoutineImpl();
    }

    private IEnumerator StopCurrentAttack()
    {
        if (currentAttack == null)
            yield break;

        currentAttack.InterruptBehaviour();
        yield return currentAttack.WaitForInterruptDone;
    }
}