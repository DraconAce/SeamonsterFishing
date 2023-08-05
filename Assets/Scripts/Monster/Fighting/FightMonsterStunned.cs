using System.Collections;
using UnityEngine;

public class FightMonsterStunned : AbstractMonsterBehaviour
{
    [SerializeField] private float stunnedTime = 5f;

    private FightMonsterBehaviourTreeManager behaviourTreeManager;
    private MonsterAnimationController monsterAnimationController;
    private WaitForSeconds waitStunned;
    
    private const string IdleTrigger = "Idle";
    private const string StunTrigger = "Stun";

    protected override MonsterState BehaviourState => MonsterState.Stunned;

    protected override void Start()
    {
        base.Start();

        behaviourTreeManager = FightMonsterSingleton.instance.BehaviourTreeManager;
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;
        
        waitStunned = new(stunnedTime);
    }

    public override float GetExecutability() => IsNodeExecutable ? 100f : 0f;

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Started Stunned Behaviour");
        
        behaviourTreeManager.ToggleBlockBehaviour(true);
        monsterAnimationController.SetTrigger(StunTrigger);

        yield return waitStunned;
        
        behaviourTreeManager.ToggleBlockBehaviour(false);
        
        monsterAnimationController.SetTrigger(IdleTrigger);
        Debug.Log("Ended Stunned Behaviour");
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield break;
    }
}