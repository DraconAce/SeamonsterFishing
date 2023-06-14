using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FloatSliderElementUpdater : AbstractOverlayElement
{
    //Todo: proper own component
    [SerializeField] private Color colorIfZeroOnce;
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

        fillImage.color = floatInfoProvider.Info switch
        {
            <= 0 => colorIfZeroOnce,
            >= 1f => normalColor,
            _ => fillImage.color
        };
    }

    private void OnDestroy() => floatInfoProvider.InfoChanged -= OnDisplayInfoUpdated;
}