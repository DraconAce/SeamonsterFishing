using System;
using DG.Tweening;
using UnityEngine;

public class PlayerKilledChecker : MonoBehaviour
{
    [SerializeField] private float distancePercentageCloserPosition = 0.85f;
    [SerializeField] private float distancePercentageKillPosition = 0.5f;
    [SerializeField] private float getCloserDuration = 1.5f;
    [SerializeField] private Ease getCloserEase = Ease.InQuint;
    [SerializeField] private float waitBeforeKillDuration = 2f;
    [SerializeField] private float moveInToKillDuration = 0.75f;
    [SerializeField] private Ease moveInToKillEase = Ease.InQuint;
    [SerializeField] private MaterialSwitcher matSwitcher;
    
    private Transform playerTransform;
    private MonsterSoundPlayer soundPlayer;

    private BaitingMonsterSingleton monsterSingleton;
    private GameStateManager gameStateManager;

    private void Start()
    {
        playerTransform = PlayerSingleton.instance.PlayerTransform;

        monsterSingleton = BaitingMonsterSingleton.instance;
        gameStateManager = GameStateManager.instance;

        TryGetComponent(out soundPlayer);
    }

    public void StartKillingPlayer()
    {
        if(monsterSingleton.PlayerIsBeingKilled) return;

        monsterSingleton.PlayerIsBeingKilled = true;
        monsterSingleton.InvokeMonsterStartedKill(transform);

        PlayKillAnimation();
    }
    
    private void PlayKillAnimation()
    {
        SwitchToAttackMat();
        
        soundPlayer.PlayKillSound();

        var getCloserPosition = GetMonsterTargetPos(distancePercentageCloserPosition);
        var killTargetPos = GetMonsterTargetPos(distancePercentageKillPosition);
        
        var sequence = DOTween.Sequence();

        sequence.Append(transform.DOMove(getCloserPosition, getCloserDuration)
            .SetEase(getCloserEase));
        
        sequence.AppendInterval(waitBeforeKillDuration);
        
        sequence.Append(transform.DOMove(killTargetPos, moveInToKillDuration)
            .SetEase(moveInToKillEase)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.Dead)));
        
        sequence.Play();
    }

    private void SwitchToAttackMat() => matSwitcher.SwitchMaterial(1);

    private Vector3 GetMonsterTargetPos(float distanceToPlayerPercentage)
    {
        var playerPos = playerTransform.position;
        
        var directionPlayerToMonster = (transform.position - playerPos).normalized;
        
        return playerPos + directionPlayerToMonster * distanceToPlayerPercentage;
    }
}