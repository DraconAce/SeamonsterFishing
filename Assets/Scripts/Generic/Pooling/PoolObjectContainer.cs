using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolObjectContainer
{
    public PoolObjectContainer(PrefabPool sourcePool, GameObject ob)
    {
        SourcePool = sourcePool;
        Ob = ob;
    }

    public PrefabPool SourcePool { get; }

    public GameObject Ob { get; }


    private readonly Dictionary<Type, List<Object>> components = new();

    private readonly List<Type> fullyQueriedTypesList = new();

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

            var compList = new List<Object>();
            CopyComponentsToObjectList(matchingComponents, compList);

            components.Add(searchedType, compList);
            
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
    
    private void CopyComponentsToObjectList<T> (List<T> compList, List<Object> objectList) where T : class
    {
        foreach(var comp in compList)
        {
            if(comp is Object ob)
                objectList.Add(ob);
        }
    }
}