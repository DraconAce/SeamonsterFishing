using TMPro;
using UnityEngine;

public class DepthHandler : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private float meterPerSecond = 0.1f;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private Transform waterSurface;

    private UpdateManager updateManager;
    private float timer;

    private void Start()
    {
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualUpdate(this);
    }

    public void ManualUpdate()
    {
        UpdateCounter();
        UpdateSurfacePosition();
    }

    private void UpdateCounter()
    {
        timer += Time.deltaTime;

        var currentDepth = CalculateCurrentDepth();
        
        depthText.text = "Depth: " + currentDepth;
    }

    private int CalculateCurrentDepth() => (int) Mathf.Round(timer * meterPerSecond);

    private void UpdateSurfacePosition()
    {
        var currentSurfacePosition = waterSurface.position;
        
        waterSurface.position = currentSurfacePosition + Vector3.up * Time.deltaTime * meterPerSecond;
    }
    
    public void ResetTimer() => timer = 0;

    private void OnDestroy()
    {
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}