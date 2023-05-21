using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackBehaviour : AbstractMonsterBehaviour
{
    //Todo: Function to chose next attack

    private WaitForSeconds waitBetweenMidRangeAttacks = new(5);
    private Coroutine currentAttack;

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

    public override void StartBehaviour()
    {
        base.StartBehaviour();

        BehaviourRoutine = StartCoroutine(AttackRoutine());
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

        currentAttack = randomAttack.BehaviourRoutine;
        yield return currentAttack;
    }

    private AbstractMonsterAttack GetAndStartRandomAttackOfType(MonsterAttackType attackType)
    {
        if (!monsterAttacks.TryGetValue(attackType, out var attacksOfRequestedType)) return null;

        var randomAttackIndex = Random.Range(0, attacksOfRequestedType.Count);

        var randomAttack = attacksOfRequestedType[randomAttackIndex];
        randomAttack.StartBehaviour();

        return randomAttack;
    }

    protected override void InterruptBehaviour()
    {
        base.InterruptBehaviour();

        StopCurrentAttack();
    }

    private void StopCurrentAttack()
    {
        if (currentAttack == null) return;
        StopCoroutine(currentAttack);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopCurrentAttack();
    }
}