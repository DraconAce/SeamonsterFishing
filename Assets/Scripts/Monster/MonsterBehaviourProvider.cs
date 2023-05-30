using System;
using System.Collections;
using UnityEngine;

public class MonsterBehaviourProvider : MonoBehaviour
{
    private Coroutine updateBehaviourRoutine;
    
    protected AbstractMonsterBehaviour activeBehaviour;

    protected virtual void Start() => updateBehaviourRoutine = StartCoroutine(UpdateBehaviour());

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