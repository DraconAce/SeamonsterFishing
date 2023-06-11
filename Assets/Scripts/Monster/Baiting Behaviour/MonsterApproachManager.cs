using System;
using UnityEngine;

[RequireComponent(typeof(MonsterPositionFaker))]
[RequireComponent(typeof(MonsterAttackManager))]

public class MonsterApproachManager : MonoBehaviour
{
    private MonsterAttackManager attackManager;
    private MonsterPositionFaker positionFaker;

    private void Start()
    {
        TryGetComponent(out attackManager);
        TryGetComponent(out positionFaker);
    }

    public void StartApproach()
    {
        //Todo: implement approach routine
    }
    
    //Todo: subscribe to monster spawned event
}