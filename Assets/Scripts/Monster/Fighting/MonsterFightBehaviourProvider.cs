using System.Collections;
using UnityEngine;

//Todo: Later replaced with normal monster behaviour provider when decision tree is implemented
public class MonsterFightBehaviourProvider : MonsterBehaviourProvider
{
    [Header("Idle")]
    [SerializeField] private FightMonsterIdle monsterIdle;

    [Header("Attack")] 
    [SerializeField] private MonsterAttackBehaviour monsterAttack;

    [Header("Stunned")] 
    [SerializeField] private FightMonsterStunned monsterStunned;

    private bool restartBehaviourLoop;
    private bool blockBehaviourLoop;

    private AbstractMonsterBehaviour currentMonsterBehaviour;

    private WaitUntil waitForLoopUnblocked;
    private WaitForSeconds waitUntilNextStateCheck;

    private Coroutine reactToMissRoutine;
    private Coroutine reactToFlashRoutine;

    private IEnumerator currentBehaviourRoutine;
    
    protected override void Start()
    {
        base.Start();
        
        waitForLoopUnblocked = new(() => !blockBehaviourLoop);
    }

    protected override IEnumerator UpdateBehaviour()
    {
        var loop = true;

        while (loop)
        {
            yield return CheckIfLoopBlocked();
            
            restartBehaviourLoop = false;
            
            currentBehaviourRoutine = IdleBehaviour();
            yield return currentBehaviourRoutine;
            
            if(restartBehaviourLoop) continue;
            yield return CheckIfLoopBlocked();

            currentBehaviourRoutine = AttackBehaviour();
            yield return currentBehaviourRoutine;
            
            if(restartBehaviourLoop) continue;
            yield return CheckIfLoopBlocked();

            //Fleeing
        }
    }

    private IEnumerator CheckIfLoopBlocked()
    {
        if (!blockBehaviourLoop) yield break;

        yield return waitForLoopUnblocked;
    }

    private IEnumerator IdleBehaviour()
    {
        yield return TriggerGivenBehaviour(monsterIdle);
    }

    private IEnumerator TriggerGivenBehaviour(AbstractMonsterBehaviour behaviour)
    {
        Debug.LogFormat("Start Behaviour of type: {0}", nameof(behaviour));
        yield return StartMonsterBehaviourRoutine(behaviour);
        
        Debug.LogFormat("Wait For Potential Interrupt: {0}", nameof(behaviour));
        yield return WaitForInterruptBehaviourIfExists();
        
        Debug.LogFormat("Behaviour Finished: {0}", nameof(behaviour));
    }

    private IEnumerator StartMonsterBehaviourRoutine(AbstractMonsterBehaviour monsterBehaviour)
    {
        currentMonsterBehaviour = monsterBehaviour;

        currentMonsterBehaviour.TriggerBehaviour();

        yield return currentMonsterBehaviour.WaitForCompletionOrInterruption;
    }

    private IEnumerator WaitForInterruptBehaviourIfExists()
    {
        if (currentMonsterBehaviour.InterruptIsDone) yield break;

        yield return currentMonsterBehaviour.WaitForInterruptDone;
    }

    private IEnumerator AttackBehaviour()
    {
        yield return TriggerGivenBehaviour(monsterAttack);
    }

    public void ReactToCannonBallMiss()
    {
        if (reactToMissRoutine != null) return;
        
        StartCoroutine(RequestRestartOfBehaviourLoop());

        reactToMissRoutine = StartCoroutine(ReactToMissRoutine());
    }

    private IEnumerator RequestRestartOfBehaviourLoop()
    {
        restartBehaviourLoop = true;
        
        StopCurrentBehaviourRoutine();

        yield return currentMonsterBehaviour.WaitForInterruptDone;
    }

    private void StopCurrentBehaviourRoutine()
    {
        if (currentMonsterBehaviour == null) return;
        
        currentMonsterBehaviour.InterruptBehaviour();
    }

    private IEnumerator ReactToMissRoutine()
    {
        blockBehaviourLoop = true;

        yield return RequestRestartOfBehaviourLoop();
        
        currentBehaviourRoutine = AttackBehaviour();
        yield return currentBehaviourRoutine;

        blockBehaviourLoop = false;
    }

    public void ReactToStunned()
    {
        if (reactToFlashRoutine != null) 
            StopCoroutine(reactToFlashRoutine);
        
        reactToFlashRoutine = StartCoroutine(ReactToStunnedRoutine());
    }

    private IEnumerator ReactToStunnedRoutine()
    {
        yield return StartCoroutine(ResetAllBehaviour());
        
        blockBehaviourLoop = true;

        yield return StartMonsterBehaviourRoutine(monsterStunned);
        
        blockBehaviourLoop = false;
    }

    private IEnumerator ResetAllBehaviour()
    {
        if(reactToMissRoutine != null) StopCoroutine(reactToMissRoutine);

        blockBehaviourLoop = true;

        yield return RequestRestartOfBehaviourLoop();
        
        blockBehaviourLoop = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (reactToMissRoutine != null)
            StopCoroutine(reactToMissRoutine);

        if (reactToFlashRoutine == null) return;
        
        StopCoroutine(reactToFlashRoutine);
    }
}