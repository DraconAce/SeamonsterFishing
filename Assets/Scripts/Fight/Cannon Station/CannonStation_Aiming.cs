using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonStation_Aiming : StationController, IManualUpdateSubscriber
{
    [SerializeField] private Transform cannonBarrel;
    [SerializeField] private RotationLimit aimLimit;
    [SerializeField] private float aimSpeed = 10f;
    
    private InputAction aimAction;
    private Vector3 targetAimAngle;
    private readonly WaitForSeconds deferredSubscriptionTime = new(0.25f);

    private CannonStation cannonStation => (CannonStation) ControllerStation;
    private UpdateManager updateManager;

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
    }

    private void OnGameStateMatchesCannonStation() => updateManager.SubscribeToManualUpdate(this);
    
    private void OnGameStateDoesNotMatchCannonStation() => updateManager.UnsubscribeFromManualUpdate(this);
    
    public void ManualUpdate() => AimCannon();

    private void AimCannon()
    {
        CalculateNewAimRotation();
        cannonBarrel.localRotation = ClampAimRotation();
    }

    private void CalculateNewAimRotation()
    {
        var aimInput = aimAction.ReadValue<Vector2>();

        targetAimAngle += InputBasedRotationProvider.CalculateRotationBasedOnInput(aimInput, aimSpeed);
    }

    private Quaternion ClampAimRotation()
    {
        var clampedQuaternion = InputBasedRotationProvider.ClampGivenRotationToLimits(aimLimit, targetAimAngle);

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
        
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}