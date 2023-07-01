using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReelingStation_ReelInControl : AbstractStationSegment, IManualUpdateSubscriber
{
    [SerializeField] private int numberOfRotationsNeeded = 20;
    [SerializeField] private float maxTimeToReel = 7;
    [SerializeField] private float progressDecreasePercentagePerSecond = 0.01f;

    private float timer;

    private InputAction reelAction;

    private ReelingStation reelingStation => (ReelingStation) ControllerStation;
    public bool PlayerInputEnabled { get; set; }

    private void Start()
    {
        GetAndEnableReelAction();

        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
    }

    private void GetAndEnableReelAction()
    {
        reelAction = reelingStation.CustomPlayerInputs.Fight_Reeling.Reel;
        reelAction.Enable();
    }

    private void OnReelingStarted()
    {
        reelingStation.GameStateManager.BlockGameStateChange = true;
        
        ControllerStation.UpdateManager.SubscribeToManualUpdate(this);
        
        PlayerInputEnabled = true;
    }

    private void OnReelingCompleted()
    {
        ControllerStation.UpdateManager.UnsubscribeFromManualUpdate(this);
        
        Debug.LogFormat("Timer: {0}", timer);

        DOVirtual.DelayedCall(reelingStation.DelayForSubStationsOnReelingCompleted, ResetReeling);
    }

    private void ResetReeling()
    {
        PlayerInputEnabled = false;
        
        currentNumberOfRotations = 0;
        timer = 0;
        
        reelingStation.Progress = 0;
        neutralInputTimer = 0;

        ResetCachedAngles();
    }

    public void ManualUpdate()
    {
        if (!PlayerInputEnabled) return;

        timer += Time.deltaTime;
        
        DoReeling();
        
        ApplyProgressDecrease();
        
        UpdateReelingProgress();
        
        CheckForReelingCompletion();
        
        CheckForReelingFailed();
    }

    private void UpdateReelingProgress() => reelingStation.Progress = currentNumberOfRotations / numberOfRotationsNeeded;

    #region Reeling Input

    [Header("Reeling Input")] 
    [SerializeField] private float inputBuffer = 0.2f;


    private float currentNumberOfRotations;
    private float lastAngleToOrigin = -1;
    private bool passedRotationFinishedDeg;
    private bool passedRotationStartedDeg;

    private Vector2 startReelDirection;

    private bool doneOnce;
    private float neutralInputTimer;
    private readonly float neutralInputTimerMax = 0.5f;

    private void DoReeling()
    {
        var reelInput = reelAction.ReadValue<Vector2>();

        if (!IsOneInputAxisInBuffer(reelInput))
        {
            if(reelInput == Vector2.zero)
            {
                neutralInputTimer += Time.deltaTime;
                
                if(neutralInputTimer >= neutralInputTimerMax) ResetCachedAngles();
            }
            else
                ResetCachedAngles();

            return;
        }
        
        neutralInputTimer = 0;

        if (AngleWasReset())
        {
            startReelDirection = reelInput;
            lastAngleToOrigin = 0;
            
            return;
        }
        
        var currentInputAngle = CalculateAngleOfInput(reelInput);

        if (!PlayerIsRotatingCounterClockwise(currentInputAngle))
        {
            passedRotationFinishedDeg = false;
            passedRotationStartedDeg = false;
            
            return;
        }

        DetermineWhetherInputAngleMarksWerePassed(currentInputAngle);

        var wasFullRotationCompleted = FullRotationWasCompleted(currentInputAngle);
        lastAngleToOrigin = currentInputAngle;

        if (!wasFullRotationCompleted) return;
        
        (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);
        currentNumberOfRotations++;
    }

    private void DetermineWhetherInputAngleMarksWerePassed(float currentInputAngle)
    {
        if (currentInputAngle is > 0 and <= 15)
            passedRotationFinishedDeg = true;
        else if (currentInputAngle is >= 345 and < 360)
            passedRotationStartedDeg = true;
    }

    private bool IsOneInputAxisInBuffer(Vector2 input) => Mathf.Abs(input.x) >= 1 - inputBuffer || Mathf.Abs(input.y) >= 1 - inputBuffer;

    private void ResetCachedAngles()
    {
        lastAngleToOrigin = -1;

        (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);
    }

    private bool AngleWasReset() => lastAngleToOrigin < 0;

    private float CalculateAngleOfInput(Vector2 input)
    {
        var signedAngle = Vector2.SignedAngle(input, startReelDirection);

        return Converter.GetAngleFrom0To360(signedAngle);
    }

    private bool PlayerIsRotatingCounterClockwise(float currentInputAngle)
    {
        if (AngleJumpOccured(currentInputAngle))
        {
            return currentInputAngle > lastAngleToOrigin;
        }

        return currentInputAngle <= lastAngleToOrigin;
    }

    private bool AngleJumpOccured(float currentInputAngle)
    {
        return Mathf.Abs(currentInputAngle - lastAngleToOrigin) > 90f;
    }

    private bool FullRotationWasCompleted(float currentInputAngle)
    {
        var passedOrigin = AngleJumpOccured(currentInputAngle);
        
        if (!passedRotationFinishedDeg || !passedRotationStartedDeg)
        {
            if(passedOrigin)
                (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);

            return false;
        }

        return passedOrigin;
    }

    #endregion

    private void ApplyProgressDecrease() => currentNumberOfRotations -= numberOfRotationsNeeded 
                                                                        * progressDecreasePercentagePerSecond * Time.deltaTime;

    private void CheckForReelingCompletion()
    {
        if (currentNumberOfRotations < numberOfRotationsNeeded) return;
        
        reelingStation.GameStateManager.BlockGameStateChange = false;

        reelingStation.TriggerReelingCompletedEvent();

        PlayerInputEnabled = false;
    }

    private void CheckForReelingFailed()
    {
        if (timer < maxTimeToReel) return;
        
        reelingStation.GameStateManager.BlockGameStateChange = false;

        PlayerInputEnabled = false;
        
        ControllerStation.GameStateManager.ChangeGameState(GameState.Dead);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        reelAction.Disable();
    }
}