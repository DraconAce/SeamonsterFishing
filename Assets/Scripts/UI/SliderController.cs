using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [Serializable]
    private struct SliderThreshold
    {
        public float Threshold;
        public Color Color;
        public UnityEvent Event;
        
        public int ThresholdIndex { get; set; }
    }
    
    [SerializeReference] private GameObject floatInfoProviderOb;

    [SerializeField] private bool displayProgressText;
    [SerializeField] private TextMeshProUGUI progressText;

    [SerializeField] private bool useThresholds;
    [SerializeField] private List<SliderThreshold> sliderThresholds;

    private IFloatInfoProvider floatInfoProvider;
    
    private Slider slider;
    private Image sliderFillImage;

    private void Start()
    {
        floatInfoProviderOb.TryGetComponent(out floatInfoProvider);
        floatInfoProvider.InfoChanged += OnFloatUpdated;

        TryGetComponent(out slider);
        sliderFillImage = slider.fillRect.GetComponent<Image>();

        if (!useThresholds) return;
        SetThresholdIndices();
    }

    private void SetThresholdIndices()
    {
        for (var i = 0; i < sliderThresholds.Count; i++)
        {
            var sliderThreshold = sliderThresholds[i];
            sliderThreshold.ThresholdIndex = i;
        }
    }

    private void OnFloatUpdated()
    {
        UpdateSliderValue();

        if (!useThresholds) return;
        
        SetCurrentThreshold();
    }

    private void UpdateSliderValue()
    {
        slider.value = floatInfoProvider.Info;
        
        if(!displayProgressText) return;
        
        var sliderAsPercentage = Mathf.RoundToInt(slider.value * 100);
        progressText.text = $"{sliderAsPercentage}%";
    }

    private void SetCurrentThreshold() => SetCurrentThreshold(floatInfoProvider.Info);

    private void SetCurrentThreshold(float value)
    {
        var currentThreshold = GetCurrentThreshold(value);
        
        UpdateSlider(currentThreshold);
    }
    
    private SliderThreshold GetCurrentThreshold(float value)
    {
        var currentThreshold = sliderThresholds.Aggregate((currentThreshold, nextThreshold) 
            => value < nextThreshold.Threshold ? currentThreshold : nextThreshold);

        return currentThreshold;
    }

    private void UpdateSlider(SliderThreshold threshold)
    {
        sliderFillImage.color = threshold.Color;
        threshold.Event?.Invoke();
    }

    private void OnDestroy()
    {
        if(floatInfoProvider == null) return;
        
        floatInfoProvider.InfoChanged -= OnFloatUpdated;
    }
}