public enum MonsterState
{
    None = 0,
    
    //Both Modes
    Attacking = 1,
    Repelled = 2,
    Idle = 3,
    Feinting = 4,
    
    //Fighting
    Stunned = 5,
    Reeling = 6,
    PreparingAttack = 7,
    Cooldown = 8,

    //Baiting
    Approaching = 9,
}