using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class InvokeWithDelay : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private UnityEvent invokeEvent;

    public void Invoke()
    {
        if (delay <= 0)
        {
            invokeEvent?.Invoke();
            return;
        }
        
        DOVirtual.DelayedCall(delay, () => invokeEvent?.Invoke());
    }
}