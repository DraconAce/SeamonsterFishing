using System;
using UnityEngine;

public class BaitingFlash : SpotFlash
{
    private BaitingMonsterSingleton monsterSingleton;

    public event Action onFlashUsed;

    protected override void Start()
    {
        base.Start();

        monsterSingleton = BaitingMonsterSingleton.instance;
    }

    protected override void FlashActivatedImpl()
    {
        onFlashUsed?.Invoke();
        
        var ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(ray, out var hit) || !hit.collider.gameObject.CompareTag("monster")) return;
        
        monsterSingleton.MonsterWasRepelled();
    }

    public void ToggleFlashReady(bool ready)
    {
        if (ready) SetFlashToReady();
        else
            FlashIsReady = false;
    }
}