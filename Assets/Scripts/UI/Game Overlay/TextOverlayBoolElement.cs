using UnityEngine;

public class TextOverlayBoolElement : AbstractOverlayTextElement
{
    [SerializeField] private string trueText;
    [SerializeField] private string falseText;

    private IBoolInfoProvider infoProvider;

    protected override void Awake()
    {
        base.Awake();
        
        infoProviderOb.TryGetComponent(out infoProvider);
        infoProvider.InfoChanged += OnDisplayInfoUpdated;
    }

    private void Start() => AssignTextBasedOnProviderInfo();

    private void AssignTextBasedOnProviderInfo() => textElement.text = infoProvider.Info ? trueText : falseText;

    protected override void OnDisplayInfoUpdated() => AssignTextBasedOnProviderInfo();

    private void OnDestroy() => infoProvider.InfoChanged -= OnDisplayInfoUpdated;
}