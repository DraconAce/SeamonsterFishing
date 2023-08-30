using System;
using UnityEngine;

public class WeakpointPlacer : MonoBehaviour
{
    [SerializeField] private float maxDistanceToMonsterTransform = 3f;
    [SerializeField] private GameObject weakpointPrefab;
    [SerializeField] private Transform monsterTransform;
    [SerializeField] private Transform weakpointParent;
    
    private PrefabPool weakpointPool;
    
    private const string CannonBallTag = "cannonBall";

    private void Start()
    {
        weakpointPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, weakpointPrefab, weakpointParent);
    }

    private void OnCollisionEnter(Collision other)
    {
        var collidedOn = other.gameObject;
                
        if (!collidedOn.CompareTag(CannonBallTag)) return;
        
        var contact = other.GetContact(0);
        
        var collisionPosition = contact.point;
        
        if(!IsHitWithinWeakpointRange(collisionPosition)) return;
        
        PlaceNewWeakpointAlongHitNormal(-contact.normal, collisionPosition);
    }

    private bool IsHitWithinWeakpointRange(Vector3 hitPosition)
    {
        var distancePivotHit = Vector3.Distance(monsterTransform.position, hitPosition);
        
        return distancePivotHit < maxDistanceToMonsterTransform;
    }
    
    private void PlaceNewWeakpointAlongHitNormal(Vector3 hitNormal, Vector3 hitPosition)
    {
        var weakpoint = weakpointPool.RequestInstance(hitPosition);
        
        //rotate forward to hit normal
        weakpoint.Ob.transform.rotation = Quaternion.LookRotation(hitNormal);
    }
}
