using System;
using DG.Tweening;
using UnityEngine;

public class RotationTween : MonoBehaviour
{
    [SerializeField] private float rotationAmount;
    [SerializeField] private Vector3 rotationVector;
    [SerializeField] private Transform targetTransform;
    
    [SerializeField] private bool loop;
    [SerializeField] private bool onStart;
    [SerializeField] private bool relative;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private Ease animationEase;

    private Tween rotationTween;
    
    private void Start()
    {
        rotationVector = rotationVector.normalized;
        
        if(!onStart) return;
        
        StartTween();
    }
    
    public void StartTween()
    {
        var targetRotation = rotationVector * rotationAmount;
        
        rotationTween = targetTransform.DOLocalRotate(targetRotation, animationDuration, RotateMode.FastBeyond360)
            .SetEase(animationEase)
            .SetRelative(relative);

        if (loop) rotationTween.SetLoops(-1, LoopType.Incremental);
    }
}