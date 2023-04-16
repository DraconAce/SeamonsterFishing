using System.Collections.Generic;
using UnityEngine;

public class PrefabPoolFactory : Singleton<PrefabPoolFactory>
{
    private Dictionary<GameObject, PrefabPool> listOfAllRequestedPools; 
    
    public PrefabPool RequestNewPool(GameObject attachOb, GameObject objectToPool)
    {
        if (listOfAllRequestedPools.TryGetValue(objectToPool, out var requestedPool))
        {
            if(requestedPool != null)
                return requestedPool;

            listOfAllRequestedPools.Remove(objectToPool);
        }

        return CreateNewPoolForObject(attachOb, objectToPool);
    }

    private PrefabPool CreateNewPoolForObject(GameObject attachOb, GameObject objectToPool)
    {
        var newPool = CreateAndAttachPoolInstance(attachOb);

        SetupPool(objectToPool, newPool);
        
        listOfAllRequestedPools.Add(objectToPool, newPool);

        return newPool;
    }

    private static PrefabPool CreateAndAttachPoolInstance(GameObject attachOb) => attachOb.AddComponent<PrefabPool>();

    private static void SetupPool(GameObject objectToPool, PrefabPool newPool)
    {
        newPool.SetPoolingObject(objectToPool);

        newPool.SetPoolParent(new GameObject("Inactive Instances: " + objectToPool.name));
    }
}