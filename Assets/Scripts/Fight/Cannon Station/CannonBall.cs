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
    private FightMonsterSingleton monsterSingleton;

    public PoolObjectContainer ContainerOfObject { get; set; }

    private const string monsterTag = "monster";
    private const string weakPointTag = "weakPoint";
    private const string waterTag = "water";


    public FMODUnity.EventReference oceanHit;

    public FMODUnity.EventReference regularHit;

    public FMODUnity.EventReference weakpointHit;

    private void Awake() => TryGetComponent(out rigidbody);

    private void Start()
    {
        monsterSingleton = FightMonsterSingleton.instance;
        
        CreateExplosionFXPool();
    }

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
        Debug.Log("collidedGameOb:" + collidedGameOb);
        if (collidedGameOb.CompareTag(monsterTag))
        {
            CannonBallRecordedValidHit();

            monsterSingleton.CannonBallMissed();

            //play sound Hit No Weakpoint
            FMODUnity.RuntimeManager.PlayOneShot(regularHit);
        }
        else if (collidedGameOb.CompareTag(weakPointTag))
        {
            CannonBallRecordedValidHit();

            monsterSingleton.WeakPointWasHit();

            //play sound Hit Weakpoint
            FMODUnity.RuntimeManager.PlayOneShot(weakpointHit);
        }
        else
        {
            //Cannon Ball missed the monster
            monsterSingleton.CannonBallMissed();
            //but could have still hit the water -
            //technically we need an additional case after this with a generic explosion sound for hitting objects with collider, but the cannonball can only hit the monster or water in our scene
            if (collidedGameOb.CompareTag(waterTag))
            {
                //play sound Hit Ocean
                FMODUnity.RuntimeManager.PlayOneShot(oceanHit);
                Debug.Log("playing ocean sound");
            }
        }
    }

    private void CannonBallRecordedValidHit()
    {
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
        
        monsterSingleton.CannonBallMissed();
    }

    public void ResetInstance() => MakeSureRigidbodyIsNotInMovement();

    private void MakeSureRigidbodyIsNotInMovement()
    {
        rigidbody.isKinematic = true;
        
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}