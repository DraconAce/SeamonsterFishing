using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReelingStation_ReelInControl : AbstractStationSegment, IManualUpdateSubscriber
{
    [SerializeField] private int numberOfRotationsNeeded = 20;
    [SerializeField] private float progressDecreasePercentagePerSecond = 0.01f;

    private InputAction reelActionMouse;
    private InputAction reelActionGamepad;
    private InputManager inputManager;
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;
    public bool PlayerInputEnabled { get; set; }

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        GetAndEnableReelAction();
        
        inputManager = InputManager.instance;

        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
    }

    private void GetAndEnableReelAction()
    {
        var customPlayerInput = reelingStation.GetPlayerInputs();
        
        reelActionMouse = customPlayerInput.Fight_Reeling.Reel_Mouse;
        reelActionGamepad = customPlayerInput.Fight_Reeling.Reel_Gamepad;
    }

    private void OnReelingStarted()
    {
        reelingStation.GameStateManager.BlockGameStateChangeWithExceptions = true;
        
        ControllerStation.UpdateManager.SubscribeToManualUpdate(this);
        
        reelActionMouse.Enable();
        reelActionGamepad.Enable();

        PlayerInputEnabled = true;
    }

    private void OnReelingCompleted()
    {
        ControllerStation.UpdateManager.UnsubscribeFromManualUpdate(this);

        DOVirtual.DelayedCall(reelingStation.DelayForSubStationsOnReelingCompleted, ResetReeling, false);
        
        reelActionMouse.Disable();
        reelActionGamepad.Disable();
    }

    private void ResetReeling()
    {
        PlayerInputEnabled = false;
        
        currentNumberOfRotations = 0;
        reelingStation.ReelingTimer = 0;
        
        reelingStation.Progress = 0;
        neutralInputTimer = 0;
        lastAngleToOrigin = -1;
        lastCursorPos = Vector2.negativeInfinity;

        ResetCachedAngles();
    }

    public void ManualUpdate()
    {
        if (!PlayerInputEnabled) return;

        reelingStation.ReelingTimer += Time.deltaTime;
        
        DoReeling();
        
        ApplyProgressDecrease();
        
        UpdateReelingProgress();
        
        CheckForReelingCompletion();
        
        CheckForReelingFailed();
    }

    private void UpdateReelingProgress() => reelingStation.Progress = currentNumberOfRotations / numberOfRotationsNeeded;

    #region Reeling Input

    [Header("Reeling Input")]
    [SerializeField] private float minDistanceToCenter = 150f;
    
    private float currentNumberOfRotations;
    private float lastAngleToOrigin = -1;
    private bool passedRotationFinishedDeg;
    private bool passedRotationStartedDeg;

    private Vector2 startCursorDirection;
    private Vector2 lastCursorPos = Vector2.negativeInfinity;

    private bool doneOnce;
    private float neutralInputTimer;
    private readonly float neutralInputTimerMax = 1f;

    private void DoReeling()
    {
        var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        var cursorPosition = GetInputBasedOnInputDevice();
        
        var cursorDirection = cursorPosition - screenCenter;
        
        SetLastCursorToCurrentIfUninitialized(cursorPosition);

        if (!IsCursorFarEnoughFromCenter(cursorPosition, screenCenter))
        {
            ResetCachedAngles();
            return;
        }
        
        if(!DidPlayerMoveCursor(cursorDirection))
        {
            IncreaseAndCheckInactiveTimer();
            return;
        }
        
        neutralInputTimer = 0;
        lastCursorPos = cursorPosition;
        
        var cursorDirectionNormalized = cursorDirection.normalized;

        if (AngleWasReset())
        {
            SetReelingVariablesToNeutral(cursorDirectionNormalized);
            return;
        }

        var currentInputAngle = CalculateAngleOfInput(cursorDirectionNormalized);

        if (!PlayerIsRotatingCounterClockwise(currentInputAngle))
        {
            (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);
            return;
        }

        DetermineWhetherInputAngleMarksWerePassed(currentInputAngle);

        var wasFullRotationCompleted = FullRotationWasCompleted(currentInputAngle);
        lastAngleToOrigin = currentInputAngle;

        if (!wasFullRotationCompleted) return;
        
        (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);
        currentNumberOfRotations++;
    }

    private Vector2 GetInputBasedOnInputDevice()
    {
        var currentInputDevice = inputManager.LatestDevice;

        if (currentInputDevice == PlayerDevice.KeyboardMouse)
            return reelActionMouse.ReadValue<Vector2>();

        var gamepadInputValue = reelActionGamepad.ReadValue<Vector2>();
        
        var gamepadCursorPos = ConvertGamepadInputToScreenPosition(gamepadInputValue);

        return gamepadCursorPos;
    }

    private static Vector2 ConvertGamepadInputToScreenPosition(Vector2 gamepadInputValue)
    {
        var halfScreenWidth = Screen.width / 2f;
        var halfScreenHeight = Screen.height / 2f;
        var screenCenter = new Vector2(halfScreenWidth, halfScreenHeight);

        var gamepadCursorPos = screenCenter + gamepadInputValue * new Vector2(halfScreenWidth, halfScreenHeight);
        return gamepadCursorPos;
    }

    private void SetLastCursorToCurrentIfUninitialized(Vector2 cursorPosition)
    {
        if (lastCursorPos == Vector2.negativeInfinity) lastCursorPos = cursorPosition;
    }

    private bool IsCursorFarEnoughFromCenter(Vector2 cursorPos, Vector2 screenCenter) => (cursorPos - screenCenter).magnitude > minDistanceToCenter;

    private void ResetCachedAngles()
    {
        lastAngleToOrigin = -1;

        (passedRotationFinishedDeg, passedRotationStartedDeg) = (false, false);
    }

    private bool DidPlayerMoveCursor(Vector2 cursorDirection)
    {
        return !Mathf.Approximately((cursorDirection - lastCursorPos).magnitude, 0);
    }

    private void IncreaseAndCheckInactiveTimer()
    {
        neutralInputTimer += Time.deltaTime;

        if (neutralInputTimer >= neutralInputTimerMax) ResetCachedAngles();
    }

    private bool AngleWasReset() => lastAngleToOrigin < 0;

    private void SetReelingVariablesToNeutral(Vector2 cursorDirectionNormalized)
    {
        startCursorDirection = cursorDirectionNormalized;
        lastAngleToOrigin = 0;
    }

    private float CalculateAngleOfInput(Vector2 cursorDirection)
    {
        var signedAngle = Vector2.SignedAngle(cursorDirection, startCursorDirection);

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

    private void DetermineWhetherInputAngleMarksWerePassed(float currentInputAngle)
    {
        if (currentInputAngle is > 0 and <= 45)
            passedRotationFinishedDeg = true;
        else if (currentInputAngle is >= 315 and < 360)
            passedRotationStartedDeg = true;
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
        
        reelingStation.GameStateManager.BlockGameStateChangeWithExceptions = false;

        reelingStation.TriggerReelingCompletedEvent();

        PlayerInputEnabled = false;
    }

    private void CheckForReelingFailed()
    {
        if (!reelingStation.IsReelingTimeUp) return;
        
        reelingStation.GameStateManager.BlockGameStateChangeWithExceptions = false;

        PlayerInputEnabled = false;
        
        ControllerStation.GameStateManager.ChangeGameState(GameState.Dead);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        reelActionMouse.Disable();
        reelActionGamepad.Disable();
    }
}