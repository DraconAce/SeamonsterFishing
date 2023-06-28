using System;
using System.Collections.Generic;

public class UpdateManager : Singleton<UpdateManager>
{
    private readonly List<IManualUpdateSubscriber> manualUpdateSubscribers = new();
    private readonly List<IManualUpdateSubscriber> manualFixedUpdateSubscribers = new();
    private readonly List<IManualUpdateSubscriber> manualLateUpdateSubscribers = new();
    
    private GameStateManager gameStateManager;

    private void Start() => gameStateManager = GameStateManager.instance;

    private void Update()
    {        
        if (!UpdateCanBeExecuted(manualUpdateSubscribers)) return;

        foreach(var subscriber in manualUpdateSubscribers)
        {
            if(!subscriber.SubscriberCanReceiveUpdate()) continue;

            subscriber.ManualUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!UpdateCanBeExecuted(manualFixedUpdateSubscribers)) return;

        foreach(var subscriber in manualFixedUpdateSubscribers)
        {
            if(!subscriber.SubscriberCanReceiveUpdate()) continue;
            
            subscriber.ManualFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        if (!UpdateCanBeExecuted(manualLateUpdateSubscribers)) return;
        
        foreach(var subscriber in manualLateUpdateSubscribers)
        {
            if (!subscriber.SubscriberCanReceiveUpdate()) continue;
            
            subscriber.ManualLateUpdate();
        }
    }

    private bool UpdateCanBeExecuted(List<IManualUpdateSubscriber> subcriberList) 
        => !gameStateManager.GameIsPaused && ListHasElements(subcriberList);

    private bool ListHasElements(List<IManualUpdateSubscriber> manualUpdateSubscriberList) 
        => manualUpdateSubscriberList.Count > 0;
    
    #region Registration

    public void SubscribeToManualUpdate(IManualUpdateSubscriber subscriber) 
        => AddSubscriberToListIfNotContained(subscriber, manualUpdateSubscribers);

    public void SubscribeToManualFixedUpdate(IManualUpdateSubscriber subscriber)
        => AddSubscriberToListIfNotContained(subscriber, manualFixedUpdateSubscribers);

    public void SubscribeToManualLateUpdate(IManualUpdateSubscriber subscriber)
        => AddSubscriberToListIfNotContained(subscriber, manualLateUpdateSubscribers);

    private void AddSubscriberToListIfNotContained(IManualUpdateSubscriber subscriber, List<IManualUpdateSubscriber> listToAddTo)
    {
        if (listToAddTo.Contains(subscriber)) return;

        listToAddTo.Add(subscriber);
    }
    
    public void UnsubscribeFromManualUpdate(IManualUpdateSubscriber subscriber) 
        => RemoveSubscriberFromList(subscriber, manualUpdateSubscribers);

    public void UnsubscribeFromManualFixedUpdate(IManualUpdateSubscriber subscriber)
        => RemoveSubscriberFromList(subscriber, manualFixedUpdateSubscribers);

    public void UnsubscribeFromManualLateUpdate(IManualUpdateSubscriber subscriber)
        => RemoveSubscriberFromList(subscriber, manualLateUpdateSubscribers);
    
    private void RemoveSubscriberFromList(IManualUpdateSubscriber subscriber, List<IManualUpdateSubscriber> listToRemoveFrom) 
        => listToRemoveFrom.Remove(subscriber);
    #endregion
}