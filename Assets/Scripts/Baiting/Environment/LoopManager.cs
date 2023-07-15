using System;
using UnityEngine;

public abstract class LoopManager : MonoBehaviour
{
    [SerializeField] private GameObject prefabToUseForLoop;
    [SerializeField] private Transform loopObjectParent;

    protected PrefabPool loopObjectPool;

    protected virtual void Start()
    {
        CreateLoopObjectPool();
        
        SetupLoop();
    }

    private void CreateLoopObjectPool()
    {
        loopObjectPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, prefabToUseForLoop, loopObjectParent);
    }

    protected virtual void SetupLoop() {}

    public void SpawnNewLoopElement() => SpawnLoopElementImpl();

    protected virtual void SpawnLoopElementImpl(){}

    protected void RequestLoopElementInstance(Vector3 position)
    {
        var obContainer = loopObjectPool.RequestInstance(position);
        
        obContainer.Ob.SetActive(true);
    }
}