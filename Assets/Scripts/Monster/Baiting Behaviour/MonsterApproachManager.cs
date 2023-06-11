using System;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]
[RequireComponent(typeof(MonsterPositionFaker))]
[RequireComponent(typeof(MonsterAttackManager))]

public class MonsterApproachManager : MonoBehaviour
{
    private BaitingMonsterState monsterState;
    
    private MonsterAttackManager attackManager;
    private MonsterPositionFaker positionFaker;

    private void Awake()
    {
        TryGetComponent(out monsterState);

        TryGetComponent(out attackManager);
        TryGetComponent(out positionFaker);
    }

    public void StartApproach()
    {
        //Todo: implement approach routine
        
        monsterState.CurrentState = MonsterState.Approaching;
    }
    
    //Todo: subscribe to monster spawned event
}