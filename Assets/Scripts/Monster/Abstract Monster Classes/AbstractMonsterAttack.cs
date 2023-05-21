using UnityEngine;

public class AbstractMonsterAttack : AbstractMonsterBehaviour
{
    public virtual MonsterAttackType AttackType => MonsterAttackType.MidRange;
}