using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RadarDotAnimation : MonoBehaviour, IPoolObject
{
    [SerializeField] private float radarBlinkDuration = 0.5f;
    [SerializeField] private Ease radarBlinkEase = Ease.InOutSine;
    
    private CanvasGroup radarDotGroup;
    
    private Tween radarDotTween;
    
    public PoolObjectContainer ContainerOfObject { get; set; }
    public void ResetInstance() => StartRadarDotTween();

    public void OnInitialisation(PoolObjectContainer container)
    {
        ContainerOfObject = container;
        
        TryGetComponent(out radarDotGroup);
    }

    private void StartRadarDotTween()
    {
        radarDotTween = radarDotGroup.DOFade(0, radarBlinkDuration)
            .SetEase(radarBlinkEase)
            .SetLoops(6, LoopType.Yoyo)
            .OnComplete(() => ContainerOfObject.ReturnToPool());
    }

    private void OnDestroy() => radarDotTween?.Kill();
}