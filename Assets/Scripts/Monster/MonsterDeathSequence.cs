using System;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

public class MonsterDeathSequence : MonoBehaviour
{
    [SerializeField] private string deathTriggerName;
    [SerializeField] private EventReference monsterDeathSound;

    [Header("Animation Settings")]
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float moveDownTime = 5f;
    [SerializeField] private float rotateBackwardsTime = 3f;
    [SerializeField] private float moveDownDistance = 100f;
    [SerializeField] private Vector3 rotateBackwardsAngle;
    
    [SerializeField] private Ease moveDownEase = Ease.InQuad;
    [SerializeField] private Ease rotateBackwardsEase = Ease.InQuad;
    
    [SerializeField] private float moveDownDelay = 0.5f;
    [SerializeField] private float rotateBackwardsDelay = 1f;
    
    private Transform monsterTransform;
    private FightMonsterSingleton monsterSingleton;
    private FightMonsterState fightMonsterState;
    private MonsterAnimationController monsterAnimationController;

    private Sequence deathSequence;

    private void Start()
    {
        monsterSingleton = FightMonsterSingleton.instance;
        
        fightMonsterState = monsterSingleton.FightState;
        monsterAnimationController = monsterSingleton.MonsterAnimationController;
        monsterTransform = monsterSingleton.MonsterTransform;
        
        fightMonsterState.MonsterStateChangedEvent += OnMonsterStateChanged;
    }

    private void OnMonsterStateChanged(MonsterState newState)
    {
        if(newState != MonsterState.Dead) return;
        
        TriggerMonsterDeath();
    }

    private void TriggerMonsterDeath()
    {
        var behaviourTreeManager = monsterSingleton.BehaviourTreeManager;
        
        behaviourTreeManager.TryForceStopCurrentBehaviour();
        behaviourTreeManager.ToggleBlockBehaviour(true);
        
        CreateDeathAnimationSequence();
    }

    private void CreateDeathAnimationSequence()
    {
        var gameStateManager = GameStateManager.instance;
        gameStateManager.BlockGameStateChangeWithoutExceptions = true;
        
        var inputManager = InputManager.instance;
        inputManager.SetInputBlocked(true);
        
        deathSequence = DOTween.Sequence();
        
        monsterAnimationController.SetTrigger(deathTriggerName);
        RuntimeManager.PlayOneShotAttached(monsterDeathSound, monsterTransform.gameObject);
        
        deathSequence.AppendInterval(GetTotalSequenceTime());

        deathSequence.Insert(moveDownDelay + initialDelay, CreateMoveDownTeen());

        deathSequence.Insert(rotateBackwardsDelay + initialDelay, CreateRotateBackwardsTween());

        deathSequence.OnComplete(() =>
        {
            inputManager.SetInputBlocked(false);
            
            gameStateManager.BlockGameStateChangeWithoutExceptions = false;
            gameStateManager.ChangeGameState(GameState.Won);
        });
        
        deathSequence.Play();
    }

    private float GetTotalSequenceTime()
    {
        return moveDownDelay + moveDownTime + rotateBackwardsDelay + rotateBackwardsTime + initialDelay;
    }

    private Tween CreateMoveDownTeen()
    {
        return monsterTransform.DOMoveY(moveDownDistance, moveDownTime)
            .SetEase(moveDownEase);
    }
    
    private Tween CreateRotateBackwardsTween()
    {
        return monsterTransform.DOLocalRotate(rotateBackwardsAngle, rotateBackwardsTime)
            .SetEase(rotateBackwardsEase);
    }

    private void OnDestroy()
    {
        deathSequence?.Kill();
        
        if (fightMonsterState == null) return;
        
        fightMonsterState.MonsterStateChangedEvent -= OnMonsterStateChanged;
    }
}