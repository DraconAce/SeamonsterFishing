using System;
using DG.Tweening;
using UnityEngine;

public class PlayerKilledChecker : MonoBehaviour
{
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

        var attackTargetPos = GetAttackTargetPos();
        
        transform.DOMove(attackTargetPos, moveInToKillDuration)
            .SetEase(moveInToKillEase)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.Dead));
    }

    private void SwitchToAttackMat() => matSwitcher.SwitchMaterial(1);

    private Vector3 GetAttackTargetPos()
    {
        var playerPos = playerTransform.position;
        
        var directionPlayerToMonster = (transform.position - playerPos).normalized;
        
        return playerPos + directionPlayerToMonster * 0.5f;
    }
}