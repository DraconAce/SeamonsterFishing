using UnityEngine;

public abstract class AbstractMonsterBehaviour : MonoBehaviour
{
    public bool ChangeMonsterStateOnStartBehaviour => false;
    public MonsterState MonsterStateOnBehaviourStart => MonsterState.Idle;

    private MonsterStateManager monsterStateManager;
    private MonsterBehaviourProvider monsterBehaviourProvider;

    protected virtual void Start()
    {
        monsterStateManager = MonsterStateManager.instance;
        monsterBehaviourProvider = MonsterBehaviourProvider.instance;
    }

    public virtual void StartBehaviour()
    {
        if (monsterBehaviourProvider.IsBehaviourActive(this)) return;
        
        if (!ChangeMonsterStateOnStartBehaviour) return;

        monsterStateManager.CurrentState = MonsterStateOnBehaviourStart;
    }
    
    public virtual void InterruptBehaviour(){}
}