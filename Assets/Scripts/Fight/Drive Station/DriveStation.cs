using System;
using FMOD.Studio;
using FMODUnity;
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

    public EventReference MoveBoatSound;
    private EventInstance MoveBoatSoundInstance;
    
    public EventReference TurnBoatSound;
    private EventInstance TurnBoatSoundInstance;

    private void Awake()
    {
        LastDriveDirection = initialDrivingDirection;
        
        PlayerTransform = PlayerSingleton.instance.PlayerTransform;

        TryGetComponent(out movingController);
        TryGetComponent(out rotatingController);

        MoveBoatSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MoveBoatSound, PlayerTransform.gameObject);
        TurnBoatSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(TurnBoatSound, PlayerTransform.gameObject);
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

        if (hasDirectionChanged || rotatingController.MovingLocked) 
        {
            //Turn Boat Sound should play
            TurnBoatSoundInstance.getPlaybackState(out var turnPlaybackState);
            if (turnPlaybackState == PLAYBACK_STATE.STOPPED) TurnBoatSoundInstance.start();
            return;
        } 
        
        //Boat is Moving Sound should play
        MoveBoatSoundInstance.getPlaybackState(out var playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED) MoveBoatSoundInstance.start();
        
        movingController.MoveBoat(moveDirection);
    }

    public void TurnRotationComplete() 
    {
        TurnBoatSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MoveBoatSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        MoveBoatSoundInstance.release();
        
        TurnBoatSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        TurnBoatSoundInstance.release();

        driveAction.Disable();

        if (UpdateManager == null) return;
        UpdateManager.UnsubscribeFromManualUpdate(this);
    }
}