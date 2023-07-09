using System;
using TMPro;
using UnityEngine;

public class DepthHandler : MonoBehaviour, IManualUpdateSubscriber, IMovePerSecondProvider
{
    [SerializeField] private float depthOffset = 110f;
    [SerializeField] private float meterPerSecond = 0.1f;
    [SerializeField] private float movementDamper = 0.01f;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private Transform waterSurface;
    [SerializeField] private float[] depthThresholds;
    
    public float ModifiedMovePerSecond => meterPerSecond * movementDamper;

    private float currentDepth;
    public float CurrentDepth
    {
        get => currentDepth;
        private set
        {
            var formerValue = currentDepth;
            
            currentDepth = value;

            if (Mathf.Approximately(formerValue, currentDepth)) return;
            
            if(currentDepthThresholdIndex >= depthThresholds.Length
               || currentDepth <= depthThresholds[currentDepthThresholdIndex]) return;
            
            currentDepthThresholdIndex++;
            
            DepthThresholdChangedEvent?.Invoke(currentDepthThresholdIndex);
        }
    }
    
    public int NumberDepthThresholds => depthThresholds.Length;

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