using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DriveStation_Moving), typeof(DriveStation_Rotating))]

public class DriveStation : AbstractStation, IManualUpdateSubscriber
{
    [SerializeField] private Transform boatTransform;
    public Transform BoatTransform => boatTransform;

    private InputAction driveAction;

    private DriveStation_Moving movingController;
    private DriveStation_Rotating rotatingController;

    protected override void GameStateMatches()
    {
        base.GameStateMatches();
        UpdateManager.SubscribeToManualUpdate(this);
    }

    protected override void GameStateDoesNotMatch()
    {
        base.GameStateDoesNotMatch();
        UpdateManager.UnsubscribeFromManualUpdate(this);
    }

    protected override void Start()
    {
        base.Start();

        SetupDrivingControllers();
        
        GetAndEnableDriveAction();

        UpdateManager.SubscribeToManualUpdate(this);

        rotatingController.CalculateLeftRotation();
    }

    private void SetupDrivingControllers()
    {
        TryGetComponent(out movingController);
        TryGetComponent(out rotatingController);

        movingController.SetupController(this);
        rotatingController.SetupController(this);
    }

    private void GetAndEnableDriveAction()
    {
        driveAction = CustomPlayerInputs.Fight_Overview.Drive;
        driveAction.Enable();
    }

    public void ManualUpdate()
    {
        if (GameStateManager.CurrentGameState != StationGameState) return;
        MoveAndRotateBoat();
    }

    private void MoveAndRotateBoat()
    {
        var moveDirection = driveAction.ReadValue<Vector2>().x;
        
        if(movingController.BoatIsNotMoving(moveDirection)) return;

        var hasDirectionChanged = rotatingController.StartRotatingIfDirectionHasChanged(moveDirection);

        if (hasDirectionChanged || rotatingController.MovingLocked) return;
        
        movingController.MoveBoat(moveDirection);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (UpdateManager == null) return;
        UpdateManager.UnsubscribeFromManualUpdate(this);
    }
}