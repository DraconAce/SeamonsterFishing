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
    
    [Header("Water Interaction VFX")] 
    [SerializeField] private GameObject VfxLeftRotationWaterInteractionObject;
    [SerializeField] private GameObject VfxRightRotationWaterInteractionObject;
    private ParticleSystem BoatLeftMist;
    private ParticleSystem BoatRightMist;

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
    private bool blockOnSequenceComplete;
    #endregion

    private void Start()
    {
        initialCannonBaseCoordinate = cannonBaseTransform.localPosition[(int)cannonBaseMovementAxis];
        
        forwardCannonBarrelRotation = cannonBarrelPivot.localEulerAngles[(int)cannonBarrelRotationAxis];
        backwardsCannonBarrelRotation = CalculateBackwardsLocalRotation(cannonBarrelPivot.parent);

        forwardLampRotation = lampPivot.localEulerAngles[(int)lampRotationAxis];
        backwardsLampRotation = CalculateBackwardsLocalRotation(lampPivot.parent); //0f - forwardLampRotation;
        
        BoatLeftMist = VfxLeftRotationWaterInteractionObject.GetComponent<ParticleSystem>();
        BoatRightMist = VfxRightRotationWaterInteractionObject.GetComponent<ParticleSystem>();
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

    private bool MoveDirectionHasChanged(float newDirection) => !Mathf.Approximately(Mathf.Sign(newDirection), Mathf.Sign(driveStation.LastDriveDirection));

    public bool StartRotatingIfDirectionHasChanged(float moveDirection)
    {
        if (!MoveDirectionHasChanged(moveDirection)) return false;
        
        StartRotationToChangeDirection(moveDirection);
        return true;
    }
    
    #region Change Direction Animation
    private void StartRotationToChangeDirection(float newDirection)
    {
        MovingLocked = true;
        driveStation.GameStateManager.BlockGameStateChangeWithExceptions = true;

        CreateAndPlayRotationSequence(newDirection);

        driveStation.LastDriveDirection = newDirection;
        
        if (newDirection > 0)
        {
            //play left
            SetActivationParticleSystem(true, BoatLeftMist);
            //deactivate right
            SetActivationParticleSystem(false, BoatRightMist);
        }
        else if (newDirection < 0)
        {
            //play right
            SetActivationParticleSystem(true, BoatRightMist);
            //deactivate left
            SetActivationParticleSystem(false, BoatLeftMist);
        }
    }

    private void CreateAndPlayRotationSequence(float newDirection)
    {
        blockOnSequenceComplete = false;
        rotationSequence?.Kill();
        
        rotationSequence = DOTween.Sequence();

        rotationSequence.Append(CreateBoatRotationTween(newDirection));
        rotationSequence.Join(CreateCannonBaseMoveTween(newDirection));
        rotationSequence.Join(CreateCannonBarrelRotationTween(newDirection));
        rotationSequence.Join(CreateLampRotationTween(newDirection));
        rotationSequence.OnComplete(OnRotationSequenceComplete);

        rotationSequence.Play();
    }

    private void OnRotationSequenceComplete()
    {
        if(blockOnSequenceComplete)
        {
            blockOnSequenceComplete = false;
            return;
        }
        
        driveStation.GameStateManager.BlockGameStateChangeWithExceptions = false;
        OnRotationEnded();
    }

    private void OnRotationEnded()
    {
        MovingLocked = false;
        driveStation.TurnRotationComplete(); //to stop the turning-sound
        //deactivate Mist-Rotation-Particles
        SetActivationParticleSystem(false, BoatRightMist);
        SetActivationParticleSystem(false, BoatLeftMist);
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

        if (newDirection > 0)
        {
            targetLocalBarrelRot[2] = 180f;
        }
        //Debug.Log("Tween target rot: "+targetLocalBarrelRot);
        
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
    
    private void SetActivationParticleSystem(bool newState, ParticleSystem ps)
    {
        //if the effect is already emitting and gets activated again, do nothing
        if (newState && ps.isEmitting) return;
        //if the effect is not emitting and gets deactivated again, do nothing
        if (!newState && !(ps.isEmitting)) return;
        
        if (newState)
        {
            ps.Play(true);
        }
        else
        {
            ps.Stop();
        }
        
    }

    public void StopRotationSequence()
    {
        blockOnSequenceComplete = true;
        OnRotationEnded();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        rotationSequence?.Kill();
    }
}