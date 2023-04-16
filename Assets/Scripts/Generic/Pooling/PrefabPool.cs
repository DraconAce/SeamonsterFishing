using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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

    public void SetPoolParent(GameObject ob)
    {
        poolParent = ob.transform;
    }

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
        newOb.ob.transform.position = newPosition;

        return newOb;
    }

    public PoolObjectContainer RequestInstance(Transform parent = null)
    {
        if (poolQueue.TryDequeue(out var newObjectContainer))
        {
            var dequeuedObject = newObjectContainer.ob;
            
            if(dequeuedObject != null)
            {
                dequeuedObject.SetActive(true);

                TryParentObject(dequeuedObject.transform, parent);

                TrySetupPoolObject(newObjectContainer);

                return newObjectContainer;
            }
        }

        var newOb = Instantiate(poolingObject);
        var container = new PoolObjectContainer 
            { ob = newOb, components = new Dictionary<Type, Object>() };

        TryParentObject(container.ob.transform, parent);
        
        return container;
    }

    private void TryParentObject(Transform obTrans, Transform parent)
    {
        if (obTrans.TryGetComponent<RectTransform>(out var rectTrans))
            rectTrans.SetParent(parent, false);
        else
            obTrans.parent = parent;
    }

    private void TrySetupPoolObject(PoolObjectContainer newContainer)
    {
        if (!newContainer.components.TryGetValue(typeof(IPoolObject), out var o))
        {
            if (!newContainer.ob.TryGetComponent<IPoolObject>(out var poolOb)) return;

            poolOb.ResetInstance();
            newContainer.components.Add(typeof(IPoolObject), poolOb as Object);
        }
        else if(o is IPoolObject poolOb)
        {
            poolOb.ResetInstance();
        }
    }

    public void ReturnInstance(PoolObjectContainer container)
    {
        TryParentObject(container.ob.transform, poolParent);
        container.ob.SetActive(false);
        poolQueue.Enqueue(container);
    }
}

public class PoolObjectContainer
{
     public GameObject ob;
     public Dictionary<Type, Object> components;

     public bool TryGetCachedComponent<T>(out T component) where T : class
     {
         var type = typeof(T);
         component = null;

         if (components.TryGetValue(type, out var o))
         {
             if (o is T comp) component = comp;
         }
         else
         {
             if(ob.TryGetComponent(out component))
                 components.Add(type, component as Object);
         }

         return component != null;
     }
}