using System;
using UnityEngine;

//Base class for Singletons
//A class T inheriting from it turns that class into a singleton
//where the instance is created when it is called on for the first time

public abstract class Singleton<T> : SingletonMonoBehaviour where T : SingletonMonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void RunOnApplicationStart()
    {
        _instance = null;
    }
    
    //instance of the singleton
    private static T _instance;
    public static T instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindOrCreateInstance();
            }
            
            return _instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == this || _instance == null) return;
        
        Destroy(gameObject);
    }

    //tries to find the instance of this singleton
    //if it can't find one it will create one
    private static T FindOrCreateInstance()
    {
        var callCreated = _instance == null;

        //attempt to find the instance of this singleton
        var instance = FindObjectOfType<T>(true);

        //if the instance is found return it
        if(instance != null)
        {
            if (callCreated) Created(instance);
            
            if (instance.AddToDontDestroy) 
                AddObToDontDestroy(instance);

            return instance;
        }

        //if not create an instance
        var name = typeof(T).Name + "Singleton";

        //container gameobject so that the somponent can be attached to it
        var containerGameObject = new GameObject(name);

        //add the Singleton as component to the gameobject
        var singletonComponent = containerGameObject.AddComponent<T>();
        
        if (singletonComponent.AddToDontDestroy) 
            AddObToDontDestroy(containerGameObject);
        
        Created(singletonComponent);

        return singletonComponent;
    }

    private static void AddObToDontDestroy(T instance)
    {
        var ob = instance.gameObject;
        ob.transform.parent = null;
        
        DontDestroyOnLoad(ob);
    }
    
    private static void AddObToDontDestroy(GameObject ob)
    {
        ob.transform.parent = null;
        
        DontDestroyOnLoad(ob);
    }

    //Function callers use to activate the instance before working with its values
    public void Activation() { }

    private static void Created(SingletonMonoBehaviour single) => single.OnCreated();

    protected virtual void OnDestroy()
    {
        if (AddToDontDestroy) return;
        
        _instance = null;
    }
}
