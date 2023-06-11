using System;
using UnityEngine;

public class PlayerKilledChecker : MonoBehaviour
{
    private BaitingMonsterSingleton monsterSingleton;
    private GameStateManager gameStateManager;

    private void Start()
    {
        monsterSingleton = BaitingMonsterSingleton.instance;
        gameStateManager = GameStateManager.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        monsterSingleton.PlayerIsBeingKilled = true;
        
        PlayKillAnimation();
    }

    private void PlayKillAnimation()
    {
        Debug.Log("Killed");
        
        //Todo: play Killed sound + blackout screen, After delay change game state

        gameStateManager.ChangeGameState(GameState.Dead);
    }
}