using System;
using TMPro;
using UnityEngine;

public class DepthHandler : MonoBehaviour, IManualUpdateSubscriber, IMovePerSecondProvider
{
    [SerializeField] private float depthOffset = 110f;
    [SerializeField] private float meterPerSecond = 0.1f;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private Transform waterSurface;
    [SerializeField] private float[] depthThresholds;
    
    public float MovePerSecond => meterPerSecond;

    private float currentDepth;
    public float CurrentDepth
    {
        get => currentDepth;
        private set
        {
            var formerValue = currentDepth;
            
            currentDepth = value;

            if (Mathf.Approximately(formerValue, currentDepth)
                || currentDepth <= depthThresholds[currentDepthThresholdIndex]) return;
            
            if(currentDepthThresholdIndex < depthThresholds.Length-1)
                currentDepthThresholdIndex++;
            
            DepthThresholdChangedEvent?.Invoke(currentDepthThresholdIndex);
        }
    }

    private UpdateManager updateManager;
    private float timer;
    private int currentDepthThresholdIndex;

    public event Action<int> DepthThresholdChangedEvent;

    private void Start()
    {
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualUpdate(this);
        
        currentDepthThresholdIndex = 0;
    }

    public void ManualUpdate()
    {
        UpdateCounter();
        UpdateSurfacePosition();
    }

    private void UpdateCounter()
    {
        timer += Time.deltaTime;

        CurrentDepth = CalculateCurrentDepth();
        
        depthText.text = "Depth: " + CurrentDepth;
    }

    private int CalculateCurrentDepth() => (int) Mathf.Round(timer * meterPerSecond + depthOffset);

    private void UpdateSurfacePosition()
    {
        var currentSurfacePosition = waterSurface.position;
        
        waterSurface.position = currentSurfacePosition + Vector3.up * (Time.deltaTime * meterPerSecond);
    }
    
    public void ResetTimer() => timer = 0;

    private void OnDestroy()
    {
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}