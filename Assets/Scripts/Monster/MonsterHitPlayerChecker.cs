using System;
using UnityEngine;

public class MonsterHitPlayerChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        GameStateManager.instance.ChangeGameState(GameState.Dead);
    }
}