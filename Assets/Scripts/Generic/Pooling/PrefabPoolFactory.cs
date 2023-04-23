using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolFactory : Singleton<PrefabPoolFactory>
{
    private readonly Dictionary<GameObject, PrefabPool> listOfAllRequestedPools = new(); 
    
    public PrefabPool RequestNewPool(GameObject attachOb, GameObject objectToPool, Transform poolObjectsParent = null)
    {
        if (listOfAllRequestedPools.TryGetValue(objectToPool, out var requestedPool))
        {
            if(requestedPool != null)
                return requestedPool;

            listOfAllRequestedPools.Remove(objectToPool);
        }

        return CreateNewPoolForObject(attachOb, objectToPool, poolObjectsParent);
    }

    private PrefabPool CreateNewPoolForObject(GameObject attachOb, GameObject objectToPool, Transform poolObjectsParent)
    {
        var newPool = CreateAndAttachPoolInstance(attachOb);

        SetupPool(objectToPool, newPool, poolObjectsParent);
        
        listOfAllRequestedPools.Add(objectToPool, newPool);

        return newPool;
    }

    private static PrefabPool CreateAndAttachPoolInstance(GameObject attachOb) => attachOb.AddComponent<PrefabPool>();

    private static void SetupPool(GameObject objectToPool, PrefabPool newPool, Transform poolObjectsParent)
    {
        newPool.SetPoolingObject(objectToPool);

        newPool.SetPoolParent(poolObjectsParent);
    }
}