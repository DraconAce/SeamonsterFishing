using TMPro;
using UnityEngine;

public class DepthHandler : MonoBehaviour, IManualUpdateSubscriber
{
    [SerializeField] private float meterPerSecond = 0.1f;
    [SerializeField] private TextMeshProUGUI depthText;

    private UpdateManager updateManager;
    private float timer;

    public void ManualUpdate() => UpdateCounter();

    private void Start()
    {
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualUpdate(this);
    }

    private void UpdateCounter()
    {
        timer += Time.deltaTime;

        var currentDepth = CalculateCurrentDepth();
        
        depthText.text = "Depth: " + currentDepth;
    }

    private int CalculateCurrentDepth() => (int) Mathf.Round(timer * meterPerSecond);
    
    public void ResetTimer() => timer = 0;

    private void OnDestroy()
    {
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}