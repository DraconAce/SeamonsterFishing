using UnityEngine;

public class DriveStation_Moving : AbstractStationSegment
{
    [SerializeField] private float driveSpeed = 5.0f;
    [SerializeField] private MinMaxLimit moveLimit;

    private Vector3 boatForwardDirection;
    private Transform boatParent;
    private Transform boatTransform;

    private DriveStation driveStation => (DriveStation) ControllerStation;
    
    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        boatTransform = driveStation.PlayerTransform;
        
        boatForwardDirection = boatTransform.forward.normalized;
        
        boatParent = boatTransform.parent;
    }

    public void MoveBoat(float moveDirection)
    {
        var boatPosition = boatTransform.position;
        var newBoatPosition = boatPosition + CalculateMoveAmount(moveDirection);

        newBoatPosition = ClampToMovementLimits(newBoatPosition);
        boatTransform.position = newBoatPosition;
    }
    
    private Vector3 CalculateMoveAmount(float driveDirection) 
        => boatForwardDirection * (driveSpeed * driveDirection * Time.deltaTime);
    
    private Vector3 ClampToMovementLimits(Vector3 newBoatPosWorld)
    {
        var newBoatPosLocal = boatParent.InverseTransformPoint(newBoatPosWorld);

        if (newBoatPosLocal.z < moveLimit.MinLimit)
            newBoatPosLocal.z = moveLimit.MinLimit;
        else if(newBoatPosLocal.z > moveLimit.MaxLimit)
            newBoatPosLocal.z = moveLimit.MaxLimit;

        newBoatPosWorld = boatParent.TransformPoint(newBoatPosLocal);
        
        return newBoatPosWorld;
    }
    
    public bool BoatIsNotMoving (float newDirection) => Mathf.Approximately(0, newDirection);
}