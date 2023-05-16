using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        newOb.Ob.transform.position = newPosition;

        return newOb;
    }

    public PoolObjectContainer RequestInstance(Transform parent = null)
    {
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
        TryCallInstantiationFunction(container);

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

    private void TryCallInstantiationFunction(PoolObjectContainer container)
    {
        if(!container.TryGetCachedComponents(out List<IPoolObject> poolObjects)) return;
        
        foreach(var poolOb in poolObjects)
            poolOb.OnInstantiation(container);
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

public class PoolObjectContainer
{
    public PoolObjectContainer(PrefabPool sourcePool, GameObject ob)
    {
        SourcePool = sourcePool;
        Ob = ob;
    }

    public PrefabPool SourcePool { get; }

    public GameObject Ob { get; }


    private Dictionary<Type, List<Object>> components = new();

    private List<Type> fullyQueriedTypesList = new();

    public bool TryGetCachedComponent<T>(out T component) where T : class
    {
        var type = typeof(T);
        component = null;

        if (components.TryGetValue(type, out var typeComponents))
        {
            var firstItemInList = typeComponents[0];
            if (firstItemInList is T comp) component = comp;
        }
        else
        {
            if(Ob.TryGetComponent(out component))
            {
                var componentAsObject = component as Object;

                if (!components.ContainsKey(type))
                    components.Add(type, new() { componentAsObject });
                else
                {
                    var currentListOfType = components[type];
                    currentListOfType.Add(componentAsObject);
                }
            }
        }

        return component != null;
    }

    public bool TryGetCachedComponents<T>(out List<T> matchingComponents) where T : class
    {
        var searchedType = typeof(T);
        matchingComponents = new();

        if (!fullyQueriedTypesList.Contains(searchedType))
        {
            matchingComponents.AddRange(Ob.GetComponents<T>());
         
            fullyQueriedTypesList.Add(searchedType);
            return matchingComponents.Count > 0;
        }

        if (!components.TryGetValue(searchedType, out var componentsList)) return false;

        CopyComponentsToGenericList(matchingComponents, componentsList);
        
        return true;
    }

    private void CopyComponentsToGenericList<T> (List<T> targetList, List<Object> listToCopyFrom) where T : class
    {
        foreach(var ob in listToCopyFrom)
        {
            if(ob is T comp)
                targetList.Add(comp);
        }
    }
}