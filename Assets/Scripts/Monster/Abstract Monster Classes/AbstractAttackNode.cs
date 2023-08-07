using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractAttackNode : AbstractMonsterBehaviour, IMonsterAnimationClient
{
    [SerializeField] protected string idleAnimationTrigger = "Idle";
    [SerializeField] private List<AnimationClip> animationClipsToListenForEnd;

    protected MonsterAnimationController MonsterAnimationController { get; private set; }
    private readonly List<string> animationClipNamesList = new ();
    
    public abstract MonsterAttackType AttackType { get; }
    protected override MonsterState BehaviourState => MonsterState.Attacking;

    protected override void Start()
    {
        base.Start();
        MonsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;

        CreateAnimationClipNamesList();

        MonsterAnimationController.AnimationFinishedEvent += OnAnimationFinished;
    }

    private void CreateAnimationClipNamesList()
    {
        #if UNITY_EDITOR
        if (animationClipsToListenForEnd.Count == 0)
        {
            Debug.LogWarning("Animation clips to listen for end are not assigned!", gameObject);
            return;
        }
        #endif

        foreach (var animationClip in animationClipsToListenForEnd) 
            animationClipNamesList.Add(animationClip.name);
    }

    public void OnAnimationFinished(string finishedAnimationName)
    {
        if (!IsAttackListeningToAnimationEnd(finishedAnimationName)) return;

        OnAnimationFinishedImpl();
    }

    private bool IsAttackListeningToAnimationEnd(string animationName)
    {
        return animationClipNamesList.Contains(animationName);
    }

    protected virtual void OnAnimationFinishedImpl(){}

    protected void StartIdleAnimation() => TriggerAnimation(idleAnimationTrigger);
    
    protected void TriggerAnimation(string trigger) => MonsterAnimationController.SetTrigger(trigger);

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        MonsterAnimationController.AnimationFinishedEvent -= OnAnimationFinished;
    }
}