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
    Hit,
    Cooldown,
    
    //Baiting
    Approaching
}