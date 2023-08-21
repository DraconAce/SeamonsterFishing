using System;
using UnityEngine;

public class SkinnedMeshColliderUpdater : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private int UpdateEveryNthFrame = 12;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    
    private MeshCollider meshCollider;
    private UpdateManager updateManager;

    private void Start()
    {
        TryGetComponent(out meshCollider);
        
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualLateUpdate(this);
        
        UpdateMeshCollider();
    }
    
    public void ManualLateUpdate()
    {
        if (Time.frameCount % UpdateEveryNthFrame != 0) return;
        
        UpdateMeshCollider();
    }

    private void UpdateMeshCollider()
    {
        var newColliderMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(newColliderMesh);
        
        meshCollider.sharedMesh = newColliderMesh;
    }

    private void OnDestroy()
    {
        if(updateManager == null) return;
        
        updateManager.UnsubscribeFromManualLateUpdate(this);
    }
}