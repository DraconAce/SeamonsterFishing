using System;
using UnityEngine;

public class PlayerEnteredMuckTracker : MonoBehaviour
{
    private const string PlayerTag = "Player";

    private MuckPlayerCollider muckPlayerCollider;
    private bool isPlayerInMuck;

    private void OnTriggerEnter(Collider other)
    {
        var collidedOb = other.gameObject;
        
        if (!collidedOb.CompareTag(PlayerTag)) return;
        
        if(muckPlayerCollider == null)
            collidedOb.TryGetComponent(out muckPlayerCollider);
        
        isPlayerInMuck = true;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag(PlayerTag)) return;
        
        isPlayerInMuck = false;
    }
    
    public void InterruptMuckEffects()
    {
        if (!isPlayerInMuck) return;
        
        muckPlayerCollider.InterruptMuckEffects();
    }
}