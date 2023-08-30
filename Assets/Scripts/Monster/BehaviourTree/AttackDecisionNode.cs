public class AttackDecisionNode : AbstractDecisionNodeImpl
{
    private FightMonsterState fightMonsterState;
    
    public override bool IsNodeExecutable => CheckIfMonsterCanAttack() && IsOneChildExecuteable();

    private bool CheckIfMonsterCanAttack()
    {
        fightMonsterState ??= FightMonsterSingleton.instance.FightState;
        
        return fightMonsterState.MonsterCanAttack();
    }

    public override float GetExecutability() => CheckIfMonsterCanAttack() ? 100f : 0f;
}