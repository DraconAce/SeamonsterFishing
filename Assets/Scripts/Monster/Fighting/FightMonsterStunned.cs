using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FightMonsterStunned : AbstractMonsterBehaviour
{
    [SerializeField] private float stunnedTime = 5f;
    [SerializeField] private float returnToOriginalTime = 3f;
    [SerializeField] private Ease returnToOriginalEase = Ease.InOutSine;

    private Transform monsterTransform;
    private MonsterAnimationController monsterAnimationController;
    private WaitForSeconds waitStunned;

    private Sequence returnToOriginalSequence;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private const string IdleTrigger = "Idle";
    private const string StunTrigger = "Stun";

    protected override MonsterState BehaviourState => MonsterState.Stunned;

    protected override void Start()
    {
        base.Start();

        var fightMonsterSingleton = FightMonsterSingleton.instance;
        
        behaviourTreeManager = fightMonsterSingleton.BehaviourTreeManager;
        monsterAnimationController = fightMonsterSingleton.MonsterAnimationController;
        monsterTransform = fightMonsterSingleton.MonsterTransform;
        
        originalPosition = monsterTransform.position;
        originalRotation = monsterTransform.rotation;
        
        waitStunned = new(stunnedTime);
    }

    public override float GetExecutability() => IsNodeExecutable ? 100f : 0f;

    protected override IEnumerator BehaviourRoutineImpl()
    {
        Debug.Log("Started Stunned Behaviour");

        behaviourTreeManager.ToggleBlockBehaviour(true);
        monsterAnimationController.SetTrigger(StunTrigger);
        
        CreateReturnToOriginalSequence();
        
        yield return waitStunned;

        if (returnToOriginalSequence.IsActive())
            yield return returnToOriginalSequence.WaitForCompletion();
        
        behaviourTreeManager.ToggleBlockBehaviour(false);
        
        monsterAnimationController.SetTrigger(IdleTrigger);
        Debug.Log("Ended Stunned Behaviour");
    }

    private void CreateReturnToOriginalSequence()
    {
        returnToOriginalSequence = DOTween.Sequence();
        
        returnToOriginalSequence.Append(monsterTransform.DOMove(originalPosition, returnToOriginalTime));
        returnToOriginalSequence.Join(monsterTransform.DORotateQuaternion(originalRotation, returnToOriginalTime));
        
        returnToOriginalSequence.SetEase(returnToOriginalEase);

        returnToOriginalSequence.Play();
    }
    
    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        behaviourTreeManager.ToggleBlockBehaviour(false);

        if (returnToOriginalSequence.IsActive())
            yield return returnToOriginalSequence.WaitForCompletion();
    }

    protected override void ForceStopBehaviourImpl() => returnToOriginalSequence?.Kill();
}