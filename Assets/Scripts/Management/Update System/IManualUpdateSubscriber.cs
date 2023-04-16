public interface IManualUpdateSubscriber
{
    public void ManualUpdate(){}
    public void ManualFixedUpdate(){}
    public void ManualLateUpdate(){}

    public bool SubscriberCanReceiveUpdate() => true;
}