using System;
using UnityEngine;

public class UpdateManagerTest : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private bool subToUpdate;
    [SerializeField] private bool subToFixedUpdate;
    [SerializeField] private bool subToLateUpdate;
    
    private UpdateManager updateManager;

    private void Start()
    {
        updateManager = UpdateManager.instance;
        
        if(subToUpdate)
            updateManager.SubscribeToManualUpdate(this);
        
        if(subToFixedUpdate)
            updateManager.SubscribeToManualFixedUpdate(this);

        if(subToLateUpdate)
            updateManager.SubscribeToManualLateUpdate(this);
    }

    public virtual bool SubscriberCanReceiveUpdate() => true;

    public void ManualUpdate() => Debug.Log("Update Received");
    public void ManualFixedUpdate() => Debug.Log("Fixed Update Received");
    public void ManualLateUpdate() => Debug.Log("Late Update Received");
}