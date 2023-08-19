using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CannonStation_Aiming : AbstractStationSegment, IManualUpdateSubscriber
{
    [FormerlySerializedAs("cannonBarrel")] [SerializeField] private Transform cannonBarrelMovementPivot;
    [SerializeField] private DriveStation driveStation;
    [SerializeField] private RotationLimit aimLimit;
    [SerializeField] private float aimSpeed = 10f;
    [SerializeField] private float playerAimInfluence = 0.4f;
    [SerializeField] private float controllerBoost = 2f;
    
    private RotationLimit invertedXAimLimit;
    private RotationLimit currentAimLimit;
    private InputAction aimAction;
    private Vector3 targetAimAngle;
    private readonly WaitForSeconds deferredSubscriptionTime = new(0.25f);

    private CannonStation CannonStation => (CannonStation) ControllerStation;
    private UpdateManager updateManager;
    private InputManager inputManager;

    private void Start()
    {
        var minVector = aimLimit.MinLimit;
        minVector.x *= -1;
        
        var maxVector = aimLimit.MaxLimit;
        maxVector.x *= -1;

        if (maxVector.x < minVector.x) 
            (maxVector.x, minVector.x) = (minVector.x, maxVector.x);

        invertedXAimLimit = new RotationLimit{ MinLimit = minVector, MaxLimit = maxVector };
    }

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        updateManager = CannonStation.UpdateManager;
        inputManager = InputManager.instance;
        
        GetAndEnableAimAction();

        StartCoroutine(DeferredUpdateSubscription());
    }

    private void GetAndEnableAimAction()
    {
        var customPlayerInput = CannonStation.GetPlayerInputs();
        
        aimAction = customPlayerInput.Fight_Cannon.Aim;
        aimAction.Enable();
    }

    private IEnumerator DeferredUpdateSubscription()
    {
        yield return deferredSubscriptionTime;
        
        if(CannonStation.GameStateMatchesStationGameState())
            updateManager.SubscribeToManualUpdate(this);
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
        var aimInput = aimAction.ReadValue<Vector2>() * (playerAimInfluence * GetControllerBoost());

        if (driveStation.InfluencedDrivingDirection < 0)
            currentAimLimit = invertedXAimLimit;
        else
            currentAimLimit = aimLimit;

        targetAimAngle += InputBasedRotationProvider.CalculateRotationBasedOnInput(aimInput, aimSpeed);
    }

    private float GetControllerBoost()
    {
        return inputManager.IsPlayerUsingController ? controllerBoost : 1f;
    }
    
    private Quaternion ClampAimRotation()
    {
        var directionAdjustmentFactor = new Vector3(driveStation.InfluencedDrivingDirection, 1, 1);
        var clampedQuaternion = InputBasedRotationProvider.ClampGivenRotationToLimits(currentAimLimit, targetAimAngle, Vector3.zero, directionAdjustmentFactor);

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