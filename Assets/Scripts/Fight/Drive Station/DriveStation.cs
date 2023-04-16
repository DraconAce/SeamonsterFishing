using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class DriveStation : MonoBehaviour, IManualUpdateSubscriber
{
    [Serializable]
    public struct MovementLimit
    {
        public float MinLimit;
        public float MaxLimit;
    }
    
    [SerializeField] private Transform boatTransform;
    [SerializeField] private float driveSpeed = 5.0f;
    [SerializeField] private MovementLimit driveLimit;
    [SerializeField] private bool forwardIsRight = true;
    [SerializeField] private float localRightRotation;
    private float localLeftRotation;

    [SerializeField] private Ease rotationEase = Ease.InSine;
    [SerializeField] private float rotationDuration = 3;
    
    private UpdateManager updateManager;

    private InputAction driveAction; //TODO: Reassign on game state change 
    private PlayerInputs customPlayerInputs;
    private Vector3 boatForwardDirection;
    private Transform boatParent;

    private bool drivingLocked;
    private float lastDrivingDirection = 1;
    private Tween rotationTween;

    private void Start()
    {
        customPlayerInputs = new();
        driveAction = customPlayerInputs.Player_FightOverview.Drive;
        driveAction.Enable();
        
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualUpdate(this);

        boatForwardDirection = boatTransform.forward.normalized;
        
        boatParent = boatTransform.parent;
        
        CalculateLeftRotation();
    }

    private void CalculateLeftRotation() => localLeftRotation = localRightRotation - 180.0f;

    public void ManualUpdate() => MoveBoat();

    #region Driving and Rotation
    private void MoveBoat()
    {
        var driveDirection = driveAction.ReadValue<Vector2>().x;
        
        if(BoatIsNotMoving(driveDirection)) return;

        if (DriveDirectionHasChanged(Mathf.Sign(driveDirection)))
        {
            StartRotationToChangeDirection(driveDirection);
            return;
        }
        
        if (drivingLocked) return;

        var boatPosition = boatTransform.position;
        var newBoatPosition = boatPosition + CalculateMoveAmount(driveDirection);

        newBoatPosition = ClampToMovementLimits(newBoatPosition);
        boatTransform.position = newBoatPosition;
    }
    
    private bool BoatIsNotMoving (float newDirection) => Mathf.Approximately(0, newDirection);

    private bool DriveDirectionHasChanged(float newDirection) => newDirection != lastDrivingDirection;

    private void StartRotationToChangeDirection(float newDirection)
    {
        drivingLocked = true;
        
        rotationTween?.Kill();

        var targetRotation = GetTargetRotation(newDirection);

        rotationTween = boatTransform.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(rotationEase)
            .OnComplete(() => drivingLocked = false);

        lastDrivingDirection = newDirection;
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

    private Vector3 CalculateMoveAmount(float driveDirection) 
        => boatForwardDirection * (driveSpeed * driveDirection * Time.deltaTime);

    private Vector3 ClampToMovementLimits(Vector3 newBoatPosWorld)
    {
        var newBoatPosLocal = boatParent.InverseTransformPoint(newBoatPosWorld);

        if (newBoatPosLocal.z < driveLimit.MinLimit)
            newBoatPosLocal.z = driveLimit.MinLimit;
        else if(newBoatPosLocal.z > driveLimit.MaxLimit)
            newBoatPosLocal.z = driveLimit.MaxLimit;

        newBoatPosWorld = boatParent.TransformPoint(newBoatPosLocal);
        
        return newBoatPosWorld;
    }
    #endregion

    private void OnDestroy()
    {
        rotationTween?.Kill();

        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}