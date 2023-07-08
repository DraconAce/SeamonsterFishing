using System;
using System.Collections.Generic;
using DG.Tweening;
using Suimono.Core;
using UnityEngine;

public class EnvironmentProgressController : MonoBehaviour
{
    [SerializeField] private DepthHandler depthHandler;
    [SerializeField] private SuimonoObject suimonoObject;
    [SerializeField] private float colorTransitionTime;
    [SerializeField] private Ease colorTransitionEase;
    [SerializeField] private List<Color> depthEnvironmentColors;

    private void Start()
    {
        SetWaterDepthColor(0);
        
        depthHandler.DepthThresholdChangedEvent += OnDepthThresholdChanged;
    }

    private void SetWaterDepthColor(int depthIndex) => suimonoObject.underwaterColor = depthEnvironmentColors[depthIndex];

    private void OnDepthThresholdChanged(int currentDepthThreshold)
    {
        if(currentDepthThreshold >= depthEnvironmentColors.Count) return;
        
        StartColorChangeTween(depthEnvironmentColors[currentDepthThreshold]);
    }

    private void StartColorChangeTween(Color targetColor)
    {
        var startColor = suimonoObject.underwaterColor;
        
        DOVirtual.Float(0, 1, colorTransitionTime, value => LerpColor(startColor, targetColor, value))
            .SetEase(colorTransitionEase);
    }

    private void LerpColor(Color startColor, Color targetColor, float lerpValue) 
        => suimonoObject.underwaterColor = Color.Lerp(startColor, targetColor, lerpValue);

    private void OnDestroy()
    {
        depthHandler.DepthThresholdChangedEvent -= OnDepthThresholdChanged;
    }
}