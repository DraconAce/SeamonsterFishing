using System.Collections;
using UnityEngine;

//Todo: Later replaced with normal monster behaviour provider when decision tree is implemented
public class MonsterFightBehaviourProvider : MonsterBehaviourProvider
{
    [SerializeField] private MinMaxLimit timeUntilNextStateCheck;

    [Header("Idle")]
    [SerializeField] private FightMonsterIdle monsterIdle;

    [Header("Attack")] 
    [SerializeField] private MonsterAttackBehaviour monsterAttack;

    /*
     * attack provider
     *
     * idle behaviour provider
     *
     * Fleeing Behaviour provider
     *
     * Stunned/Repelled Behaviour provider
     */

    private bool restartBehaviourLoop;
    private IEnumerator currentBehaviourRoutine;

    private WaitForSeconds waitUntilNextStateCheck;
    private WaitUntil waitForNeutralIdlePosition;
    private WaitUntil waitForLoopUnblocked;

    public bool BlockBehaviourLoop { get; set; }

    protected override void Start()
    {
        base.Start();
        
        waitForNeutralIdlePosition = new (() => monsterIdle.IsInNeutralPosition);
        waitForLoopUnblocked = new(() => !BlockBehaviourLoop);
    }

    public void RequestRestartOfBehaviourLoop()
    {
        restartBehaviourLoop = true;

        StopCurrentBehaviourRoutine();
    }

    private void StopCurrentBehaviourRoutine()
    {
        if (currentBehaviourRoutine == null) return;
        StopCoroutine(currentBehaviourRoutine);
    }

    public void AdvanceBehaviourRoutine() => StopCurrentBehaviourRoutine();

    protected override IEnumerator UpdateBehaviour()
    {
        var loop = true;

        while (loop)
        {
            restartBehaviourLoop = false;

            currentBehaviourRoutine = IdleBehaviour();
            yield return currentBehaviourRoutine;
            
            if(restartBehaviourLoop) continue;

            currentBehaviourRoutine = AttackBehaviour();
            yield return currentBehaviourRoutine;
            
            if(restartBehaviourLoop) continue;


            //Fleeing
        }
    }

    private IEnumerator IdleBehaviour()
    {
        monsterIdle.StartBehaviour();

        GenerateWaitUntilNextCheck();
        yield return waitUntilNextStateCheck;

        yield return waitForNeutralIdlePosition;
        
        yield return CheckIfLoopBlocked();
    }

    private void GenerateWaitUntilNextCheck() 
        => waitUntilNextStateCheck = new(timeUntilNextStateCheck.GetRandomBetweenLimits());

    private IEnumerator CheckIfLoopBlocked()
    {
        if (!BlockBehaviourLoop) yield break;

        yield return waitForLoopUnblocked;
    }

    private IEnumerator AttackBehaviour()
    {
        monsterAttack.StartBehaviour();

        yield return monsterAttack.BehaviourRoutine;

        yield return CheckIfLoopBlocked();
    }
}