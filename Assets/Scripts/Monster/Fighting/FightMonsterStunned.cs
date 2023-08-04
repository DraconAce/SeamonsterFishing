using System.Collections;
using UnityEngine;

public class FightMonsterStunned : AbstractMonsterBehaviour
{
    [SerializeField] private float stunnedTime = 5f;

    private WaitForSeconds waitStunned;

    protected override MonsterState BehaviourState => MonsterState.Stunned;

    protected override void Start()
    {
        base.Start();

        waitStunned = new(stunnedTime);
    }

    public override float GetExecutability() => IsNodeExecutable ? 100f : 0f;

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Started Stun");

        yield return StartCoroutine(StunnedRoutine());
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        yield break;
    }

    private IEnumerator StunnedRoutine()
    {
        yield return waitStunned;
        Debug.Log("Stun Done");
    }
}