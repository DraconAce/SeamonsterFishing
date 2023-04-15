using System;

public class UpdateManager : Singleton<UpdateManager>
{
    public event Action UpdateEvent;
    public event Action FixedUpdateEvent;
    public event Action LateUpdateEvent;

    private void Update() => UpdateEvent?.Invoke();

    private void FixedUpdate() => FixedUpdateEvent?.Invoke();

    private void LateUpdate() => LateUpdateEvent?.Invoke();
}