using System;
using UnityEngine;
using UnityEngine.Events;

public class CannonBall : MonoBehaviour, IPoolObject
{
    [SerializeField] private UnityEvent onCannonBallHit;
    
    private Rigidbody rigidbody;

    public PoolObjectContainer ContainerOfObject { get; set; }

    private const string monsterTag = "monster";
    private const string weakPointTag = "weakPoint";

    private void Start() => TryGetComponent(out rigidbody);

    public void ApplyForceToCannonBall(Vector3 forceDirection)
    {
        rigidbody.isKinematic = false;;
        
        rigidbody.AddForce(forceDirection, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        var collidedGameOb = other.gameObject;

        if (!collidedGameOb.CompareTag(monsterTag) 
            && !collidedGameOb.CompareTag(weakPointTag)) return;
        
        CannonBallHit();
        ((IPoolObject)this).ReturnInstanceToPool();
    }

    private void CannonBallHit() => onCannonBallHit?.Invoke();

    private void OnTriggerExit(Collider other)
    {
        var collidedGameOb = other.gameObject;

        if (!collidedGameOb.CompareTag("water")) return;
        
        ((IPoolObject)this).ReturnInstanceToPool();
    }

    public void ResetInstance() => MakeSureRigidbodyIsNotInMovement();

    private void MakeSureRigidbodyIsNotInMovement()
    {
        rigidbody.isKinematic = true;
        
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}