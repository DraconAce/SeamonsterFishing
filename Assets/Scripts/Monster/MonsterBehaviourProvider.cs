using System;
using System.Collections;
using UnityEngine;

public class MonsterBehaviourProvider : MonoBehaviour
{
    private Coroutine updateBehaviourRoutine;
    
    protected AbstractMonsterBehaviour activeBehaviour;
    protected GameStateManager gameStateManager;

    protected virtual void Start()
    {
        gameStateManager = GameStateManager.instance;
        updateBehaviourRoutine = StartCoroutine(UpdateBehaviour());
    }

    protected virtual IEnumerator UpdateBehaviour()
    {
        yield break;
    }

    public bool IsBehaviourActive(AbstractMonsterBehaviour behaviourToCheck) 
        => activeBehaviour == behaviourToCheck;

    protected virtual void OnDestroy()
    {
        if (updateBehaviourRoutine == null) return;
        StopCoroutine(updateBehaviourRoutine);
    }
}