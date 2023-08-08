using System;
using UnityEngine;

public class HitPlayerChecker : MonoBehaviour
{
    public FMODUnity.EventReference BoatIsHitSound;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Damage")) return;
        
        FMODUnity.RuntimeManager.PlayOneShot(BoatIsHitSound, other.ClosestPoint(transform.position));

        GameStateManager.instance.ChangeGameState(GameState.Dead);
    }
}