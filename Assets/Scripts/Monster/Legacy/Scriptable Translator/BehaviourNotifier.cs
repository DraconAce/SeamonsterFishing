using System;
using UnityEngine;

public class BehaviourNotifier : AbstractBehaviourTranslator<AbstractMonsterBehaviour>
{
    private MonsterKI monsterKi;

    private void Start()
    {
        monsterKi = MonsterKI.instance;

        monsterKi.startNodeActionNotificationEvent += OnStartActionNotification;
        monsterKi.interruptNodeActionNotificationEvent += OnInterruptActionNotification;
    }

    private void OnStartActionNotification(string behaviourID)
    {
        if (!allBehaviours.TryGetValue(behaviourID, out var behaviour)) return;
        
        behaviour.TriggerBehaviour();
    }
    
    private void OnInterruptActionNotification(string behaviourID)
    {
        if (!allBehaviours.TryGetValue(behaviourID, out var behaviour)) return;
        
        behaviour.InterruptBehaviour();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (monsterKi == null) return;
        
        monsterKi.startNodeActionNotificationEvent -= OnStartActionNotification;
        monsterKi.interruptNodeActionNotificationEvent -= OnInterruptActionNotification;
    }
}