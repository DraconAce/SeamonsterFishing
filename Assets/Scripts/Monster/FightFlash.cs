using UnityEngine;

public class FightFlash : SpotFlash
{
    private FightMonsterSingleton monsterSingleton;

    protected override void Start()
    {
        base.Start();
        
        monsterSingleton = FightMonsterSingleton.instance;
    }

    protected override void FlashActivatedImpl() => monsterSingleton.FlashWasUsed();
}