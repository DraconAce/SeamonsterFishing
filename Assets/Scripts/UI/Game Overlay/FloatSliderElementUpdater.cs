using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FloatSliderElementUpdater : AbstractOverlayElement
{
    //Todo: proper own component
    [SerializeField] private float limit = 0.2f;
    [FormerlySerializedAs("colorIfZeroOnce")] [SerializeField] private Color colorIfLimitReached;
    [SerializeField] private Color normalColor;
    
    private IFloatInfoProvider floatInfoProvider;
    private Slider slider;
    private Image fillImage;

    private void Awake()
    {
        TryGetComponent(out slider);

        infoProviderOb.TryGetComponent(out floatInfoProvider);
        floatInfoProvider.InfoChanged += OnDisplayInfoUpdated;

        slider.fillRect.gameObject.TryGetComponent(out fillImage);
    }

    private void Start() => OnDisplayInfoUpdated();

    protected override void OnDisplayInfoUpdated()
    {
        slider.value = floatInfoProvider.Info;
        
        if(floatInfoProvider.Info <= limit)
            fillImage.color = colorIfLimitReached;
        else if(floatInfoProvider.Info >= 1f)
            fillImage.color = normalColor;
    }

    private void OnDestroy() => floatInfoProvider.InfoChanged -= OnDisplayInfoUpdated;
}