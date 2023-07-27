using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CannonStation_Aiming : AbstractStationSegment, IManualUpdateSubscriber
{
    [FormerlySerializedAs("cannonBarrel")] [SerializeField] private Transform cannonBarrelMovementPivot;
    [SerializeField] private RotationLimit aimLimit;
    [SerializeField] private float aimSpeed = 10f;
    
    private InputAction aimAction;
    private Vector3 targetAimAngle;
    private readonly WaitForSeconds deferredSubscriptionTime = new(0.25f);

    private CannonStation cannonStation => (CannonStation) ControllerStation;
    private UpdateManager updateManager;
    private DriveStation driveStation;

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        updateManager = cannonStation.UpdateManager;
        
        GetAndEnableAimAction();

        StartCoroutine(DeferredUpdateSubscription());
    }

    private void GetAndEnableAimAction()
    {
        aimAction = cannonStation.CustomPlayerInputs.Fight_Cannon.Aim;
        aimAction.Enable();
    }

    private IEnumerator DeferredUpdateSubscription()
    {
        yield return deferredSubscriptionTime;
        
        if(cannonStation.GameStateMatchesStationGameState())
            updateManager.SubscribeToManualUpdate(this);

        driveStation = StationManager.instance.GetStationOfGameState(GameState.FightOverview) as DriveStation;
    }

    protected override void OnGameStateMatchesCannonStation() => updateManager.SubscribeToManualUpdate(this);
    
    protected override void OnGameStateDoesNotMatchCannonStation() => updateManager.UnsubscribeFromManualUpdate(this);
    
    public void ManualUpdate() => AimCannon();

    private void AimCannon()
    {
        CalculateNewAimRotation();
        cannonBarrelMovementPivot.localRotation = ClampAimRotation();
    }

    private void CalculateNewAimRotation()
    {
        var aimInput = aimAction.ReadValue<Vector2>();
        aimInput *= driveStation.InfluencedDrivingDirection * 0.4f; //also reduce cannon movement by player
        
        //Debug.Log("drivingDirection: "+driveStation.InfluencedDrivingDirection);
        if (driveStation.InfluencedDrivingDirection < 0)
        {
            //rotation of the cannonbarrel to show the fuse inverts left/right- and up/down-movement of the player
            //-> so we invert it again here:
            aimInput = -aimInput;
        }
        //Debug.Log("aiminput: "+aimInput);
        
        targetAimAngle += InputBasedRotationProvider.CalculateRotationBasedOnInput(aimInput, aimSpeed);
    }
    
    private Quaternion ClampAimRotation()
    {
        var directionAdjustmentFactor = new Vector3(driveStation.InfluencedDrivingDirection, 1, 1);
        var clampedQuaternion = InputBasedRotationProvider.ClampGivenRotationToLimits(aimLimit, targetAimAngle, Vector3.zero, directionAdjustmentFactor);

        targetAimAngle = clampedQuaternion.eulerAngles;

        return clampedQuaternion;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        aimAction.Disable();
        
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}