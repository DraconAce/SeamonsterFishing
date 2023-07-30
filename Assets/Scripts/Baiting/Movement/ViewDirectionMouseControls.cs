using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewDirectionMouseControls : AbstractViewDirectionControls, IManualUpdateSubscriber
{
    [SerializeField] private float triggerTurnEdgePercentage = 0.05f;
    [SerializeField] private float timePlayerNeedsToBeStationary = 1f;

    private bool userIsStillInEdgeZoneAfterViewChange;
    private bool viewCanBeChanged;
    
    private MinMaxLimit screenEdgeLimits;
    private Vector2 mousePositionOnTimerStart;

    private Tween timerTween;
    
    private PlayerInputs playerInputs;
    private InputAction mousePositionAction;
    
    private UpdateManager updateManager;

    public override PlayerDevice ControlDevice => PlayerDevice.KeyboardMouse;

    protected override void Start()
    {
        updateManager = UpdateManager.instance;
        
        playerInputs = new();
        mousePositionAction = playerInputs.Baiting.MousePosition;
        
        DetermineScreenEdgeLimits();
        
        base.Start();
    }

    private void DetermineScreenEdgeLimits()
    {
        var screenWidth = Screen.width;
        
        var triggerTurnWidth = screenWidth * triggerTurnEdgePercentage;

        var leftLimit = triggerTurnWidth;
        var rightLimit = screenWidth - triggerTurnWidth;
        
        screenEdgeLimits = new MinMaxLimit(leftLimit, rightLimit);
    }

    protected override void ToggleTrackInputForView(bool trackInput)
    {
        if (trackInput)
            EnableLookInput();
        else
            DisableLookInput();
    }

    private void EnableLookInput()
    {
        mousePositionAction.Enable();
        updateManager.SubscribeToManualUpdate(this);
    }

    private void DisableLookInput()
    {
        mousePositionAction.Disable();
        updateManager.UnsubscribeFromManualUpdate(this);
    }

    public void ManualUpdate() => CheckViewInput("");

    protected override void ChangeViewBasedOnInput(string actionName)
    {
        var mousePos = mousePositionAction.ReadValue<Vector2>();
        var mouseXPosition = mousePos.x;

        if (screenEdgeLimits.IsValueBetweenLimits(mouseXPosition))
        {
            ResetTimer();
            userIsStillInEdgeZoneAfterViewChange = false;
            return;
        }

        if (userIsStillInEdgeZoneAfterViewChange) return;
        
        if (DoesTimerNeedToBeStarted(mousePos))
        {
            StartTimer(mousePos);
            return;
        }

        if (!viewCanBeChanged) return;

        RequestViewChangeBasedOnInput(mouseXPosition);
    }

    private void ResetTimer()
    {
        timerTween?.Kill();
        timerTween = null;
    }

    private bool DoesTimerNeedToBeStarted(Vector2 currentMousePos)
    {
        return !timerTween.IsActive() || !timerTween.IsPlaying() && !timerTween.IsComplete() || currentMousePos != mousePositionOnTimerStart;
    }

    private void StartTimer(Vector2 mousePos)
    {
        ResetTimer();
        
        mousePositionOnTimerStart = mousePos;
        
        timerTween = DOVirtual.DelayedCall(timePlayerNeedsToBeStationary, () => viewCanBeChanged = true);
    }

    private void RequestViewChangeBasedOnInput(float mouseXPosition)
    {
        (userIsStillInEdgeZoneAfterViewChange, viewCanBeChanged) = (true, false);
        
        ResetTimer();

        var requestedDirection = GetRequestedViewDirection(mouseXPosition);

        ViewDirectionHandler.RotateView(requestedDirection);
    }

    private ViewDirectionHandler.RotationDirection GetRequestedViewDirection(float mouseX)
    {
        var isLeftLimitCloser = DetermineIfMouseCloserToLeftLimit(mouseX);
        
        return isLeftLimitCloser ? ViewDirectionHandler.RotationDirection.Left 
            : ViewDirectionHandler.RotationDirection.Right;
    }

    private bool DetermineIfMouseCloserToLeftLimit(float mouseX)
    {
        var distanceFromLeftLimit = DetermineDistanceOfValues(mouseX, screenEdgeLimits.MinLimit);
        var distanceFromRightLimit = DetermineDistanceOfValues(mouseX, screenEdgeLimits.MaxLimit);
        
        return distanceFromLeftLimit <= distanceFromRightLimit;
    }
    
    private float DetermineDistanceOfValues(float value1, float value2)
    {
        if(value1 > value2)
            return value1 - value2;
        
        return value2 - value1;
    }
}
