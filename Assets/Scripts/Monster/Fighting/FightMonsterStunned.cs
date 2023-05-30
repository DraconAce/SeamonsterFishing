using System.Collections;
using UnityEngine;

public class FightMonsterStunned : AbstractMonsterBehaviour
{
    [SerializeField] private float stunnedTime = 5f;

    private WaitForSeconds waitStunned;
    
    public override bool ChangeMonsterStateOnStartBehaviour => true;
    public override MonsterState MonsterStateOnBehaviourStart => MonsterState.Stunned;

    protected override void Start()
    {
        base.Start();

        waitStunned = new(stunnedTime);
    }

    protected override IEnumerator StartBehaviourImpl()
    {
        Debug.Log("Started Stun");

        yield return StartCoroutine(StunnedRoutine());
        
        yield return base.StartBehaviourImpl();
    }

    private IEnumerator StunnedRoutine()
    {
        yield return waitStunned;
        Debug.Log("Stun Done");
    }
}