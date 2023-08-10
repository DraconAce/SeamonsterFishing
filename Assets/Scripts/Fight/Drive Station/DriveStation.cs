using System;
using System.Collections;
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

    public DriveStation_Moving MovingController { get; private set; }
    public DriveStation_Rotating RotatingController { get; private set; }

    public float LastDriveDirection { get; set; }
    public float InfluencedDrivingDirection => LastDriveDirection * initialDrivingDirection;

    private bool stoppingBoatMoveCoroutingIsRunning = false;
    private IEnumerator stoppingBoatMoveCoroutine;
    private float lastMoveDirection = 0f;
    [SerializeField] private float stoppingBoatSpeedReduction = 0.2f;

    public EventReference MoveBoatSound;
    private EventInstance MoveBoatSoundInstance;
    
    public EventReference TurnBoatSound;
    private EventInstance TurnBoatSoundInstance;
    
    [SerializeField] private GameObject VfxDrivingWaterInteractionObject;
    private ParticleSystem BoatTrailMist;
    private ParticleSystem BoatTrailWake;

    private void Awake()
    {
        LastDriveDirection = initialDrivingDirection;

        var playerSingleton = PlayerSingleton.instance;
        PlayerTransform = playerSingleton.PlayerTransform;
        playerSingleton.DriveStation = this;

        MovingController = GetComponent<DriveStation_Moving>();
        RotatingController = GetComponent<DriveStation_Rotating>();

        MoveBoatSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MoveBoatSound, PlayerTransform.gameObject);
        TurnBoatSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(TurnBoatSound, PlayerTransform.gameObject);
        
        BoatTrailMist = VfxDrivingWaterInteractionObject.GetComponent<ParticleSystem>();
        BoatTrailWake = VfxDrivingWaterInteractionObject.transform.GetChild(0).GetComponent<ParticleSystem>();
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

        RotatingController.CalculateLeftRotation();
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
        
        //Debug.Log("currentBoatSpeed: "+ movingController.currentSpeed);
        
        if(MovingController.BoatIsNotMoving(moveDirection)) 
        {
            //movingController.currentSpeed = 0;
            if (lastMoveDirection != 0f && !RotatingController.MovingLocked)
            {
                float rememberlastMoveDirection = lastMoveDirection;
                lastMoveDirection = 0f;
                MoveBoatSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                tryStartingCoroutine(rememberlastMoveDirection);
            }
            return;
        }

        var hasDirectionChanged = RotatingController.StartRotatingIfDirectionHasChanged(moveDirection);

        if (hasDirectionChanged || RotatingController.MovingLocked) 
        {
            //Turn Boat Sound should play
            TurnBoatSoundInstance.getPlaybackState(out var turnPlaybackState);
            if (turnPlaybackState == PLAYBACK_STATE.STOPPED) TurnBoatSoundInstance.start();
            return;
        } 
        
        //Boat is Moving Sound should play
        MoveBoatSoundInstance.getPlaybackState(out var playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED) MoveBoatSoundInstance.start();
        
        //activate Water displacement VFX behind boat
        SetWaterDisplacementVfxWhileDriving(true);
        
        if (stoppingBoatMoveCoroutingIsRunning)
        {
            //stop move-fadeout coroutine while driving 
            StopCoroutineIfItExists();
        }
        
        MovingController.IncreaseCurrentBoatSpeed();
        MovingController.MoveBoat(moveDirection);
        
        //save moveDirection to determine if the BoatStopper should play
        lastMoveDirection = moveDirection; 
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
    
    private void tryStartingCoroutine(float directionToStopIn)
    {
        if (stoppingBoatMoveCoroutingIsRunning) return;
        stoppingBoatMoveCoroutingIsRunning = true;
        stoppingBoatMoveCoroutine = DoBoatStop(directionToStopIn);
        StartCoroutine(stoppingBoatMoveCoroutine);
        //deactivate Emission of Water displacement VFX behind boat when driving-fadeout starts, so the particles naturally fade out
        SetWaterDisplacementVfxWhileDriving(false);
    }
    
    private void StopCoroutineIfItExists()
    {
        //deactivate Water displacement VFX behind boat after driving-fadeout stopped
        SetWaterDisplacementVfxWhileDriving(false);
        
        if (stoppingBoatMoveCoroutine != null) 
        {
            StopCoroutine(stoppingBoatMoveCoroutine);
            stoppingBoatMoveCoroutingIsRunning = false;
        }
    }
    
    private IEnumerator DoBoatStop(float moveDirection) 
    {
        //Debug.Log("Coroutine started");
        while (stoppingBoatMoveCoroutingIsRunning) 
        {
            MovingController.currentSpeed -= stoppingBoatSpeedReduction;
            if (MovingController.currentSpeed <= stoppingBoatSpeedReduction) 
            {
                MovingController.currentSpeed = 0f;
                stoppingBoatMoveCoroutingIsRunning = false;
                //deactivate Water displacement VFX behind boat after driving-fadeout stopped
                //SetWaterDisplacementVfxWhileDriving(false);
                yield return null;
            }
            MovingController.MoveBoat(moveDirection);
            yield return new WaitForFixedUpdate();
        } 
        //Debug.Log("Coroutine stop over time");
    }
    
    private void SetWaterDisplacementVfxWhileDriving(bool newState)
    {
        //if the effect is already emitting and gets activated again, do nothing
        if (newState && BoatTrailMist.isEmitting) return;
        
        if (newState) {
            //activate Emission of particles
            BoatTrailMist.Play(true);
            BoatTrailWake.Play(true);
        }
        else 
        {
            //deactivate Emission of Water displacement VFX behind boat
            BoatTrailMist.Stop();
            BoatTrailWake.Stop();
        }
        
    }
    
}