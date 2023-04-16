using System.Collections.Generic;

public class UpdateManager : Singleton<UpdateManager>
{
    private readonly List<IManualUpdateSubscriber> manualUpdateSubscribers = new();
    private readonly List<IManualUpdateSubscriber> manualFixedUpdateSubscribers = new();
    private readonly List<IManualUpdateSubscriber> manualLateUpdateSubscribers = new();

    private void Update()
    {        
        if (!ListHasElements(manualUpdateSubscribers)) return;

        foreach(var subscriber in manualUpdateSubscribers)
        {
            if(!subscriber.SubscriberCanReceiveUpdate()) continue;

            subscriber.ManualUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!ListHasElements(manualFixedUpdateSubscribers)) return;

        foreach(var subscriber in manualFixedUpdateSubscribers)
        {
            if(!subscriber.SubscriberCanReceiveUpdate()) continue;
            
            subscriber.ManualFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        if (!ListHasElements(manualLateUpdateSubscribers)) return;
        
        foreach(var subscriber in manualLateUpdateSubscribers)
        {
            if (!subscriber.SubscriberCanReceiveUpdate()) continue;
            
            subscriber.ManualLateUpdate();
        }
    }

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
    
    public void UnsubscribeToManualUpdate(IManualUpdateSubscriber subscriber) 
        => RemoveSubscriberFromList(subscriber, manualUpdateSubscribers);

    public void UnsubscribeToManualFixedUpdate(IManualUpdateSubscriber subscriber)
        => RemoveSubscriberFromList(subscriber, manualFixedUpdateSubscribers);

    public void UnsubscribeToManualLateUpdate(IManualUpdateSubscriber subscriber)
        => RemoveSubscriberFromList(subscriber, manualLateUpdateSubscribers);
    
    private void RemoveSubscriberFromList(IManualUpdateSubscriber subscriber, List<IManualUpdateSubscriber> listToRemoveFrom) 
        => listToRemoveFrom.Remove(subscriber);
    #endregion
}