using UnityEngine;

public class DriveStation_Moving : AbstractStationSegment
{
    [SerializeField] private float maxDriveSpeed = 5.0f;
    [SerializeField] private float driveSpeedincrease = 0.025f;
    public float CurrentSpeed { get; set; }
    private float muckSpeedMultiplier = 1f;

    [SerializeField] private MinMaxLimit moveLimit;
    
    private float hasNotMovedTime;
    private Vector3 boatForwardDirection;
    private Transform boatParent;
    private Transform boatTransform;

    private DriveStation driveStation => (DriveStation) ControllerStation;

    private const float StationaryTimeLimit = 10f;
    
    public bool PlayerIsStationary => hasNotMovedTime >= StationaryTimeLimit;
    
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

        //Debug.Log("currentSpeed"+currentSpeed);
        newBoatPosition = ClampToMovementLimits(newBoatPosition);
        
        boatTransform.position = newBoatPosition;
    }
    
    private Vector3 CalculateMoveAmount(float driveDirection) 
        => boatForwardDirection * (CurrentSpeed * driveDirection * muckSpeedMultiplier * Time.deltaTime);
    
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

    public void IncreaseCurrentBoatSpeed() 
    {
        //increase current speed
        if (CurrentSpeed < maxDriveSpeed) 
        {
            CurrentSpeed += driveSpeedincrease;
        }
        else 
        {
            CurrentSpeed = maxDriveSpeed;
        }
    }
    
    public void SetBoatInMuck(bool boatIsInMuck)
    {
        if (boatIsInMuck)
        {
            //is in Muck
            muckSpeedMultiplier = 0.5f;
        }
        else
        {
            //left Muck
            muckSpeedMultiplier = 1f;
        }
    }

    public bool BoatIsStanding (float newDirection)
    {
        var isBoatStanding = Mathf.Approximately(0, newDirection);
        
        if(isBoatStanding)
            hasNotMovedTime += Time.deltaTime;
        else
            hasNotMovedTime = 0f;

        return isBoatStanding;
    }
}