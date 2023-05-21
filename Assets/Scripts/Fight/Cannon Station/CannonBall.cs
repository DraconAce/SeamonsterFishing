using System;
using UnityEngine;
using UnityEngine.Events;

public class CannonBall : MonoBehaviour, IPoolObject
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private UnityEvent onCannonBallHit;

    private GameObject explosionPoolParent;
    private PrefabPool explosionPool;
    private Rigidbody rigidbody;

    public PoolObjectContainer ContainerOfObject { get; set; }

    private const string monsterTag = "monster";
    private const string weakPointTag = "weakPoint";

    private void Awake() => TryGetComponent(out rigidbody);

    private void Start() => CreateExplosionFXPool();

    private void CreateExplosionFXPool()
    {
        explosionPoolParent = new GameObject();
        
        explosionPool =
            PrefabPoolFactory.instance.RequestNewPool(explosionPoolParent, explosionPrefab,
                explosionPoolParent.transform);
    }

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

    private void CannonBallHit()
    {
        onCannonBallHit?.Invoke();
        
        InstantiateExplosion();
    }

    private void InstantiateExplosion() => explosionPool.RequestInstance(transform.position);

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