using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractAttackNode : AbstractMonsterBehaviour, IMonsterAnimationClient
{
    [SerializeField] private List<AnimationClip> animationClipsToListenForEnd;
    [SerializeField] protected string idleAnimationTrigger;

    public abstract MonsterAttackType AttackType { get; }
    
    protected override MonsterState BehaviourState => MonsterState.Attacking;
    
    protected MonsterAnimationController monsterAnimationController;
    
    protected readonly List<string> animationClipNamesList= new ();

    protected override void Start()
    {
        base.Start();
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;

        CreateAnimationClipNamesList();

        monsterAnimationController.AnimationFinishedEvent += OnAnimationFinished;
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
        if (!animationClipNamesList.Contains(finishedAnimationName)) return;

        OnAnimationFinishedImpl();
    }
    
    protected virtual void OnAnimationFinishedImpl(){}

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        monsterAnimationController.AnimationFinishedEvent -= OnAnimationFinished;
    }
}