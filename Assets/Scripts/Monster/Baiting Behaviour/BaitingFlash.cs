using UnityEngine;

public class BaitingFlash : SpotFlash
{
    private BaitingMonsterSingleton monsterSingleton;

    protected override void Start()
    {
        base.Start();

        monsterSingleton = BaitingMonsterSingleton.instance;
    }

    protected override void FlashActivatedImpl()
    {
        var ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(ray, out var hit) || !hit.collider.gameObject.CompareTag("monster")) return;
        
        monsterSingleton.MonsterWasRepelled();
    }
}