public enum MonsterState
{
    None,
    
    //Both Modes
    Attacking,
    Repelled,
    Idle,
    Feinting,
    
    //Fighting
    Stunned,
    Fleeing,
    PreparingAttack,
    Cooldown,

    //Baiting
    Approaching
}