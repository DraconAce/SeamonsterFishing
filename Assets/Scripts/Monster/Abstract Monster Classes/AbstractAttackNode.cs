using System.Collections;
using UnityEngine;

public abstract class AbstractAttackNode : AbstractMonsterBehaviour
{
    public abstract MonsterAttackType AttackType { get; }
    
    protected override MonsterState BehaviourState => MonsterState.Attacking;

}