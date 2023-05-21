using System;
using DG.Tweening;
using UnityEngine.Events;

[Serializable]
public struct TweenSettings
{
    public float Duration;
    public Ease TweenEase;
    
    public UnityEvent OnStartAction;
    public UnityEvent OnCompleteAction;
}