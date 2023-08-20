using System;
using DG.Tweening;
using UnityEngine;

public class PlayerKilledChecker : MonoBehaviour
{
    [SerializeField] private float distancePercentageKillPosition = 0.5f;
    [SerializeField] private float killDuration;
    [SerializeField] private AnimationCurve killCurve;
    [SerializeField] private string attackMatName = "Kraken_Mat";
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

        var killTargetPos = GetMonsterTargetPos(distancePercentageKillPosition);
        
        transform.DOMove(killTargetPos, killDuration)
            .SetEase(killCurve)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.Dead));
    }

    private void SwitchToAttackMat() => matSwitcher.SwitchMaterial(attackMatName);

    private Vector3 GetMonsterTargetPos(float distanceToPlayerPercentage)
    {
        var playerPos = playerTransform.position;
        
        var directionPlayerToMonster = (transform.position - playerPos).normalized;
        
        return playerPos + directionPlayerToMonster * distanceToPlayerPercentage;
    }
}