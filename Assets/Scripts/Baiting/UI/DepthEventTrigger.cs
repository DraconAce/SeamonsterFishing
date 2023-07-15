using UnityEngine;
using UnityEngine.Events;

public class DepthEventTrigger : MonoBehaviour
{
    [SerializeField] private DepthHandler depthHandler;
    [SerializeField] private UnityEvent lastThresholdEvent;
    [SerializeField] private UnityEvent[] depthEvents;

    private void Start() => depthHandler.DepthThresholdChangedEvent += OnDepthThresholdChanged;

    private void OnDepthThresholdChanged(int currentDepthThreshold)
    {
        if(currentDepthThreshold >= depthHandler.NumberDepthThresholds)
            lastThresholdEvent?.Invoke();
        
        currentDepthThreshold--; 
        
        if(currentDepthThreshold >= depthEvents.Length) return;
        
        depthEvents[currentDepthThreshold]?.Invoke();
    }
    
    private void OnDestroy() => depthHandler.DepthThresholdChangedEvent -= OnDepthThresholdChanged;
}