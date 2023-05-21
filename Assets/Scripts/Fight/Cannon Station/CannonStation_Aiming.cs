using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CannonStation_Aiming : AbstractStationController, IManualUpdateSubscriber
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

        cannonStation.StationGameStateMatchesEvent += OnGameStateMatchesCannonStation;
        cannonStation.StationGameStateDoesNotMatchEvent += OnGameStateDoesNotMatchCannonStation;

        driveStation = StationManager.instance.GetStationOfGameState(GameState.FightOverview) as DriveStation;
    }

    private void OnGameStateMatchesCannonStation() => updateManager.SubscribeToManualUpdate(this);
    
    private void OnGameStateDoesNotMatchCannonStation() => updateManager.UnsubscribeFromManualUpdate(this);
    
    public void ManualUpdate() => AimCannon();

    private void AimCannon()
    {
        CalculateNewAimRotation();
        cannonBarrelMovementPivot.localRotation = ClampAimRotation();
    }

    private void CalculateNewAimRotation()
    {
        var aimInput = aimAction.ReadValue<Vector2>();
        aimInput *= driveStation.InfluencedDrivingDirection;

        targetAimAngle += InputBasedRotationProvider.CalculateRotationBasedOnInput(aimInput, aimSpeed);
    }
    
    private Quaternion ClampAimRotation()
    {
        var directionAdjustmentFactor = new Vector3(driveStation.InfluencedDrivingDirection, 1, 1);
        var clampedQuaternion = InputBasedRotationProvider.ClampGivenRotationToLimits(aimLimit, targetAimAngle, Vector3.zero, directionAdjustmentFactor);

        targetAimAngle = clampedQuaternion.eulerAngles;

        return clampedQuaternion;
    }

    private void OnDestroy()
    {
        if (cannonStation != null)
        {
            cannonStation.StationGameStateMatchesEvent -= OnGameStateMatchesCannonStation;
            cannonStation.StationGameStateDoesNotMatchEvent -= OnGameStateDoesNotMatchCannonStation;
        }
        
        aimAction.Disable();
        
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}