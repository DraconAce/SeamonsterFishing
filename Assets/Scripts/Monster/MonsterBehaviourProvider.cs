using System;
using UnityEngine;

public class MonsterBehaviourProvider : Singleton<MonsterBehaviourProvider>, IManualUpdateSubscriber
{
    protected MonsterStateManager monsterStateManager;

    protected AbstractMonsterBehaviour activeBehaviour;

    private UpdateManager updateManager;

    protected virtual void Start()
    {
        monsterStateManager = MonsterStateManager.instance;
        updateManager = UpdateManager.instance;
        
        updateManager.SubscribeToManualLateUpdate(this);
    }

    public void ManualLateUpdate() => UpdateBehaviour();

    protected virtual void UpdateBehaviour(){}

    public bool IsBehaviourActive(AbstractMonsterBehaviour behaviourToCheck) 
        => activeBehaviour == behaviourToCheck;

    protected virtual void OnDestroy()
    {
        if(updateManager == null) return;
        
        updateManager.UnsubscribeFromManualLateUpdate(this);
    }
}