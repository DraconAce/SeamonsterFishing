using System;
using DG.Tweening;
using UnityEngine;

public class ViewDirectionHandler : MonoBehaviour
{
    public struct ViewRotationSettings
    {
        public float RotationDurationPercentage;
        public Vector3 TargetRotation;
    }
    
    public enum RotationDirection
    {
        Left = -1,
        Right = 1
    }

    [SerializeField] private float MaxRotationDurationToNextView = 8;
    [SerializeField] private Transform viewLeftRightPivot;

    [SerializeField] private Ease rotationEase;
    [SerializeField] private bool useAnimationCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    
    private int lookDirection;
    private Vector3 lastTargetRotation;
    private Tween leftRightTween;
    private PlayerSingleton playerSingleton;

    private const float rightRotation = 90;

    private const float leftRotation = -90;

    private const float MaxViewRotation = 180.0f;
    
    public event Action<ViewRotationSettings> OnTransmitTargetRotation;

    private void Start()
    {
        playerSingleton = PlayerSingleton.instance;
        playerSingleton.MovementEnabledStateChanged += OnMovementEnabledStateChanged;
    }

    private void OnMovementEnabledStateChanged(bool isDisabled)
    {
        if (!isDisabled) return;
        
        leftRightTween?.Kill();
    }

    public void RotateView(RotationDirection direction)
    {
        if (!PlayerCanTurn(direction)) return;

        var rotateToLeft = direction == RotationDirection.Left;
        HandleLeftRightRotation(rotateToLeft);
    }

    private bool PlayerCanTurn(RotationDirection direction)
    {
        var nextLookDirection = lookDirection + (int)direction;

        if (Mathf.Abs(nextLookDirection) > 1) return false;

        lookDirection = nextLookDirection;
        return true;
    }

    private void HandleLeftRightRotation(bool rotateToLeft)
    {
        var rotationAngle = rotateToLeft ? leftRotation : rightRotation;

        var targetRotation = CalculateTargetRotation(rotationAngle);
        
        leftRightTween?.Kill();

        AnimateViewChangeRotation(targetRotation);
    }

    private Vector3 CalculateTargetRotation(float rotationAngle)
    {
        var baseRotation = RotationTweenIsPlaying() ? lastTargetRotation : viewLeftRightPivot.eulerAngles;

        var targetRotation = baseRotation + new Vector3(0,rotationAngle, 0);

        lastTargetRotation = targetRotation;

        return targetRotation;
    }

    private void AnimateViewChangeRotation(Vector3 targetRotation)
    {
        void OnCompleteAction() => lastTargetRotation = Vector3.zero;

        var diffToTargetRot = Quaternion.Angle(viewLeftRightPivot.rotation, Quaternion.Euler(targetRotation));
        var diffPercentage = 1f / MaxViewRotation * diffToTargetRot;
        
        OnTransmitTargetRotation?.Invoke(new(){ RotationDurationPercentage = diffPercentage, TargetRotation = targetRotation });

        var totalRotDuration = MaxRotationDurationToNextView * 2 * diffPercentage;

        if (useAnimationCurve)
        {
            leftRightTween = viewLeftRightPivot.DORotate(targetRotation, totalRotDuration)
                .SetEase(rotationCurve)
                .OnComplete(OnCompleteAction);

            return;
        }

        leftRightTween = viewLeftRightPivot.DORotate(targetRotation, totalRotDuration)
            .SetEase(rotationEase)
            .OnComplete(OnCompleteAction);
    }

    private bool RotationTweenIsPlaying() => leftRightTween != null && leftRightTween.IsPlaying();

    private void OnDestroy()
    {
        leftRightTween?.Kill();

        if (playerSingleton == null) return;
        
        playerSingleton.MovementEnabledStateChanged -= OnMovementEnabledStateChanged;
    }
}