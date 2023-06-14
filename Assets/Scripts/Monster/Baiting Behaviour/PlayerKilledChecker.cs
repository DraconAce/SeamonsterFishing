using System;
using DG.Tweening;
using UnityEngine;

public class PlayerKilledChecker : MonoBehaviour
{
    [SerializeField] private MaterialSwitcher matSwitcher;
    
    private Transform playerAttackRepTrans;
    private MonsterSoundPlayer soundPlayer;

    private BaitingMonsterSingleton monsterSingleton;
    private GameStateManager gameStateManager;

    private void Start()
    {
        playerAttackRepTrans = PlayerSingleton.instance.PlayerRepresentation;

        monsterSingleton = BaitingMonsterSingleton.instance;
        gameStateManager = GameStateManager.instance;

        TryGetComponent(out soundPlayer);
    }

    public void StartKillingPlayer()
    {
        monsterSingleton.PlayerIsBeingKilled = true;
        PlayKillAnimation();
    }
    
    private void PlayKillAnimation()
    {
        Debug.Log("Killed");
        
        //Todo: blackout screen
        SwitchToAttackMat();
        
        soundPlayer.PlayKillSound();
        
        transform.DOMove(playerAttackRepTrans.position, 1f) //Todo: faster
            .SetEase(Ease.InQuint)
            .OnComplete(() => gameStateManager.ChangeGameState(GameState.Dead));
    }

    private void SwitchToAttackMat()
    {
        matSwitcher.SwitchMaterial(1);
    }
}