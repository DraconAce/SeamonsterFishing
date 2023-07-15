using System.Collections.Generic;
using UnityEngine;

public class PrefabPool : MonoBehaviour
{
    private GameObject poolingObject;
    private Transform poolParent;
    
    private readonly Queue<PoolObjectContainer> poolQueue = new();

    public void SetPoolingObject(GameObject ob)
    {
        if (poolingObject != null) return;
        poolingObject = ob;
    }

    public void SetPoolParent(Transform newParent) => poolParent = newParent;

    public List<PoolObjectContainer> PoolMultipleInstances(int numberToPool, Transform parent = null)
    {
        var poolObList = new List<PoolObjectContainer>();
        
        for(var i = 0; i < numberToPool; i++)
        {
            var instance = RequestInstance(parent);
            
            poolObList.Add(instance);
        }

        return poolObList;
    }

    public PoolObjectContainer RequestInstance(Vector3 newPosition, Transform parent = null)
    {
        var newOb = RequestInstance(parent);
        
        var newObTrans = newOb.Ob.transform;
        
        if(newObTrans is RectTransform rectTrans)
            rectTrans.anchoredPosition = newPosition;
        else
            newObTrans.position = newPosition;

        return newOb;
    }

    public PoolObjectContainer RequestInstance(Transform parent = null)
    {
        parent ??= poolParent;
        
        if (poolQueue.TryDequeue(out var newObjectContainer))
        {
            var dequeuedObject = newObjectContainer.Ob;
            
            if(dequeuedObject != null)
            {
                dequeuedObject.SetActive(true);

                TryParentObject(dequeuedObject.transform, parent);

                TrySetupPoolObject(newObjectContainer);

                return newObjectContainer;
            }
        }

        var newOb = Instantiate(poolingObject);
        
        var container = new PoolObjectContainer(this, newOb);
        TryInitializePoolObject(container);

        TryParentObject(container.Ob.transform, parent);
        
        return container;
    }

    private void TryParentObject(Transform obTrans, Transform parent)
    {
        if (obTrans.TryGetComponent<RectTransform>(out var rectTrans))
            rectTrans.SetParent(parent, false);
        else
            obTrans.parent = parent;
    }

    private void TryInitializePoolObject(PoolObjectContainer container)
    {
        if(!container.TryGetCachedComponents(out List<IPoolObject> poolObjects)) return;
        
        foreach(var poolOb in poolObjects)
        {
            poolOb.OnInitialisation(container);
            
            poolOb.ResetInstance();
        }
    }

    private void TrySetupPoolObject(PoolObjectContainer newContainer)
    {
        if (!newContainer.TryGetCachedComponent<IPoolObject>(out var o))
        {
            if (!newContainer.Ob.TryGetComponent<IPoolObject>(out var poolOb)) return;

            poolOb.ResetInstance();
            newContainer.TryGetCachedComponents<IPoolObject>(out _);
        }
        else if(o is IPoolObject poolOb) poolOb.ResetInstance();
    }

    public void ReturnInstance(PoolObjectContainer container)
    {
        TryParentObject(container.Ob.transform, poolParent);

        if (container.TryGetCachedComponents<IPoolObject>(out var poolObjects))
        {
            foreach(var poolObject in poolObjects)
                poolObject.OnReturnInstance();
        }
        
        container.Ob.SetActive(false);
        poolQueue.Enqueue(container);
    }
}