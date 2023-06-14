using System;
using UnityEngine;

[RequireComponent(typeof(BaitingMonsterState))]
[RequireComponent(typeof(MonsterPositionFaker))]

public class MonsterApproachManager : MonoBehaviour
{
    private BaitingMonsterState monsterState;
    
    private MonsterPositionFaker positionFaker;

    private void Awake()
    {
        TryGetComponent(out monsterState);

        TryGetComponent(out positionFaker);
    }

    public void StartApproach()
    {
        positionFaker.StartPositionVariationRoutine();
        
        monsterState.CurrentState = MonsterState.Approaching;
    }
}