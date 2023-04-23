using DG.Tweening;
using UnityEngine;

public class DriveStation_Rotating : StationController
{
    [SerializeField] private bool forwardIsRight = true;
    [SerializeField] private float localRightRotation;
    
    private float localLeftRotation;
    private float lastDrivingDirection = 1;
    private Transform boatTransform;

    public bool MovingLocked { get; private set; }

    #region Rotation Animation
    [Header("Animation")]
    [SerializeField] private float rotationDuration = 3;
    [SerializeField] private Ease rotationEase = Ease.InSine;
    
    private Tween rotationTween;
    #endregion

    private DriveStation driveStation => (DriveStation)ControllerStation;

    protected override void OnControllerSetup() => boatTransform = driveStation.BoatTransform;

    public void CalculateLeftRotation() => localLeftRotation = localRightRotation - 180.0f;

    private bool MoveDirectionHasChanged(float newDirection) => newDirection != lastDrivingDirection;

    public bool StartRotatingIfDirectionHasChanged(float moveDirection)
    {
        if (!MoveDirectionHasChanged(Mathf.Sign(moveDirection))) return false;
        
        StartRotationToChangeDirection(moveDirection);
        return true;
    }

    private void StartRotationToChangeDirection(float newDirection)
    {
        MovingLocked = true;
        
        rotationTween?.Kill();

        var targetRotation = GetTargetRotation(newDirection);

        rotationTween = boatTransform.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(rotationEase)
            .OnComplete(() => MovingLocked = false);

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

    private void OnDestroy() => rotationTween?.Kill();
}