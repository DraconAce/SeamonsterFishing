using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DriveStation_Moving), typeof(DriveStation_Rotating))]

public class DriveStation : AbstractStation, IManualUpdateSubscriber
{
    [SerializeField] private float initialDrivingDirection = -1;

    public float InitialDrivingDirection => initialDrivingDirection;

    public Transform PlayerTransform { get; private set; }

    private InputAction driveAction;

    private DriveStation_Moving movingController;
    private DriveStation_Rotating rotatingController;

    public float LastDriveDirection { get; set; }
    public float InfluencedDrivingDirection => LastDriveDirection * initialDrivingDirection;

    public FMODUnity.EventReference MoveBoatSound;
    private FMOD.Studio.EventInstance MoveBoatSoundInstance;

    private void Awake()
    {
        LastDriveDirection = initialDrivingDirection;
        
        PlayerTransform = PlayerSingleton.instance.PlayerTransform;

        TryGetComponent(out movingController);
        TryGetComponent(out rotatingController);

        MoveBoatSoundInstance = FMODUnity.RuntimeManager.CreateInstance(MoveBoatSound);
    }

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

        GetAndEnableDriveAction();

        UpdateManager.SubscribeToManualUpdate(this);

        rotatingController.CalculateLeftRotation();
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
        
        if(movingController.BoatIsNotMoving(moveDirection)) 
        {
            MoveBoatSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            return;
        }

        var hasDirectionChanged = rotatingController.StartRotatingIfDirectionHasChanged(moveDirection);

        if (hasDirectionChanged || rotatingController.MovingLocked) return;
        
        //Boat is Moving Sound should play
        FMOD.Studio.PLAYBACK_STATE playbackState;
        MoveBoatSoundInstance.getPlaybackState(out playbackState);
        if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED) MoveBoatSoundInstance.start();
        
        movingController.MoveBoat(moveDirection);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        MoveBoatSoundInstance.release();

        driveAction.Disable();

        if (UpdateManager == null) return;
        UpdateManager.UnsubscribeFromManualUpdate(this);
    }
}