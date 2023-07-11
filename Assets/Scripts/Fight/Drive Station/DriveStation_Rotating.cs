using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class DriveStation_Rotating : AbstractStationSegment
{
    [Header("Boat Rotation")]
    [SerializeField] private bool forwardIsRight = true;
    [SerializeField] private float localRightRotation;

    private float localLeftRotation;
    
    private Transform boatTransform;

    public bool MovingLocked { get; private set; }

    #region Rotation Animation
    [Header("Cannon Rotation")] 
    [SerializeField] private Axis cannonBaseMovementAxis = Axis.XAXIS;
    [SerializeField] private Axis cannonBarrelRotationAxis = Axis.XAXIS;
    [SerializeField] private Transform cannonBaseTransform;
    [SerializeField] private Transform cannonBarrelPivot;
    [SerializeField] private Ease cannonAnimationEase = Ease.InCubic; 
    
    [Header("Flash Rotation")]
    [SerializeField] private Axis lampRotationAxis = Axis.XAXIS;
    [SerializeField] private Transform lampPivot;
    [SerializeField] private Ease lampAnimationEase = Ease.InCubic; 

    [Header("Animation")]
    [SerializeField] private float rotationDuration = 3;
    [SerializeField] private Ease rotationEase = Ease.InSine;
    
    private float initialCannonBaseCoordinate;
    private float forwardCannonBarrelRotation;
    private float backwardsCannonBarrelRotation;
    private Sequence rotationSequence;

    public float forwardLampRotation = 95f;
    private float backwardsLampRotation;
    #endregion

    private void Start()
    {
        initialCannonBaseCoordinate = cannonBaseTransform.localPosition[(int)cannonBaseMovementAxis];
        
        forwardCannonBarrelRotation = cannonBarrelPivot.localEulerAngles[(int)cannonBarrelRotationAxis];
        backwardsCannonBarrelRotation = CalculateBackwardsLocalRotation(cannonBarrelPivot.parent);

        forwardLampRotation = lampPivot.localEulerAngles[(int)lampRotationAxis];
        backwardsLampRotation = CalculateBackwardsLocalRotation(lampPivot.parent); //0f - forwardLampRotation;
    }

    private float CalculateBackwardsLocalRotation(Transform fixParent)
    {
        var offsetObjectRotation = GetFixParentOffsetLocalRotation(fixParent);
        return CalculateOn180MirroredRotation(offsetObjectRotation);
    }

    private float GetFixParentOffsetLocalRotation(Transform fixParent) => fixParent.localEulerAngles[(int)cannonBarrelRotationAxis];

    private float CalculateOn180MirroredRotation(float rotationToMirror)
    {
        var mirrorRotationClampedBetween180s = Converter.AdjustRotationToNegative(0 - rotationToMirror);
        var rotationOffsetTo180InParent = mirrorRotationClampedBetween180s;

        var totalOffset = rotationOffsetTo180InParent + mirrorRotationClampedBetween180s;
        
        return Converter.AdjustRotationToNegative(Mathf.Abs(totalOffset) <= 90 ? 180 + totalOffset : -totalOffset);
    }

    private DriveStation driveStation => (DriveStation)ControllerStation;

    protected override void OnControllerSetup() => boatTransform = driveStation.PlayerTransform;

    public void CalculateLeftRotation() => localLeftRotation = localRightRotation - 180.0f;

    private bool MoveDirectionHasChanged(float newDirection) => newDirection != driveStation.LastDriveDirection;

    public bool StartRotatingIfDirectionHasChanged(float moveDirection)
    {
        if (!MoveDirectionHasChanged(Mathf.Sign(moveDirection))) return false;
        
        StartRotationToChangeDirection(moveDirection);
        return true;
    }
    
    #region Change Direction Animation
    private void StartRotationToChangeDirection(float newDirection)
    {
        MovingLocked = true;
        driveStation.GameStateManager.BlockGameStateChange = true;

        CreateAndPlayRotationSequence(newDirection);

        driveStation.LastDriveDirection = newDirection;
    }

    private void CreateAndPlayRotationSequence(float newDirection)
    {
        rotationSequence?.Kill();
        
        rotationSequence = DOTween.Sequence();

        rotationSequence.Append(CreateBoatRotationTween(newDirection));
        rotationSequence.Join(CreateCannonBaseMoveTween(newDirection));
        rotationSequence.Join(CreateCannonBarrelRotationTween(newDirection));
        rotationSequence.Join(CreateLampRotationTween(newDirection));
        rotationSequence.OnComplete(() =>
        {
            MovingLocked = false;
            driveStation.GameStateManager.BlockGameStateChange = false;
            driveStation.TurnRotationComplete(); //to stop the turning-sound
        });

        rotationSequence.Play();
    }

    private Tween CreateBoatRotationTween(float newDirection)
    {
        var targetRotation = GetTargetRotation(newDirection);

        return boatTransform.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(rotationEase);
    }

    private Vector3 GetTargetRotation(float newDirection)
    {
        var targetRotation = boatTransform.localEulerAngles;

        if (forwardIsRight)
            targetRotation.y = newDirection < 0 ? localLeftRotation : localRightRotation;
        else
            targetRotation.y = newDirection < 0 ? localRightRotation : localLeftRotation;

        return targetRotation;
    }

    private Tween CreateCannonBaseMoveTween(float newDirection)
    {
        var targetCoord = newDirection * initialCannonBaseCoordinate * driveStation.InitialDrivingDirection;

        var targetPosition = cannonBaseTransform.localPosition;
        targetPosition[(int)cannonBaseMovementAxis] = targetCoord;

        return cannonBaseTransform.DOLocalMove(targetPosition, rotationDuration)
            .SetEase(cannonAnimationEase);
    }

    private Tween CreateCannonBarrelRotationTween(float newDirection)
    {
        var targetLocalBarrelRot = Vector3.zero;
        targetLocalBarrelRot[(int)cannonBarrelRotationAxis] = DetermineTargetBarrelRotation(newDirection * driveStation.InitialDrivingDirection);

        return cannonBarrelPivot
            .DOLocalRotate(targetLocalBarrelRot, rotationDuration)
            .SetEase(cannonAnimationEase);
    }

    private Tween CreateLampRotationTween(float newDirection)
    {
        var targetLocalLampRot = Vector3.zero;
        targetLocalLampRot[(int)lampRotationAxis] = DetermineTargetLampRotation(newDirection * driveStation.InitialDrivingDirection);

        return lampPivot
            .DOLocalRotate(targetLocalLampRot, rotationDuration, RotateMode.Fast)
            .SetEase(lampAnimationEase);
    }

    private float DetermineTargetBarrelRotation(float newDirection) => newDirection >= 0 ? forwardCannonBarrelRotation : backwardsCannonBarrelRotation;
    
    private float DetermineTargetLampRotation(float newDirection) => newDirection >= 0 ? forwardLampRotation : backwardsLampRotation;
    #endregion
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        rotationSequence?.Kill();
    }
}