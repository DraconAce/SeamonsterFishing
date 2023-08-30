using System;
using DG.Tweening;
using UnityEngine;

public class DelayedFlashlightMovement : MonoBehaviour
{
    [SerializeField] private float movementDelay = 2f;
    [SerializeField] private ViewDirectionHandler viewDirectionHandler;

    [Header("Animation")] 
    [SerializeField] private float maxRotationDuration = 8;
    [SerializeField] private Ease rotationEase;
    [SerializeField] private bool useAnimationCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    
    private Tween movementTween;

    private void Start()
    {
        transform.parent = viewDirectionHandler.transform.parent;
        
        viewDirectionHandler.OnTransmitTargetRotation += OnTransmitTargetRotation;
    }
    
    private void OnTransmitTargetRotation(ViewDirectionHandler.ViewRotationSettings viewRotationSettings)
    {
        movementTween?.Kill();
        
        DOVirtual.DelayedCall(movementDelay, () => StartTweenToTargetRotation(viewRotationSettings), false);
    }

    private void StartTweenToTargetRotation(ViewDirectionHandler.ViewRotationSettings viewRotationSettings)
    {
        var targetRotation = viewRotationSettings.TargetRotation;
        var rotDuration = viewRotationSettings.RotationDurationPercentage * maxRotationDuration;
        
        if (useAnimationCurve)
        {
            movementTween = transform.DORotate(targetRotation, rotDuration)
                .SetEase(rotationCurve);

            return;
        }

        movementTween = transform.DORotate(targetRotation, rotDuration)
            .SetEase(rotationEase);
    }

    private void OnDestroy()
    {
        viewDirectionHandler.OnTransmitTargetRotation -= OnTransmitTargetRotation;
    }
}