using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewDirectionHandler : MonoBehaviour, IInputEventSubscriber
{
    private enum RotationDirection
    {
        Left,
        Right,
        Down,
        Up
    }
    
    [SerializeField] private Transform viewLeftRightPivot;
    [SerializeField] private Transform viewUpDownPivot;
    [SerializeField] private string[] actionsToSubscribeTo = {"Change_View_Left", "Change_View_Right"};
    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    [SerializeField] private float rotationDuration;
    [SerializeField] private Ease rotationEase;
    
    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    private const float rightRotation = 90;
    private const float leftRotation = -90;
    private const float downRotation = 90;
    private const float upRotation = 0;

    private InputManager inputManager;
    private InputManager.SubscriberSettings inputSubscriberSettings;

    private Vector3 lastTargetRotation;
    private Tween leftRightTween; 
    private Tween upDownTween;

    private bool isLookingDown;

    private void Start()
    {
        inputManager = InputManager.instance;

        inputSubscriberSettings = new InputManager.SubscriberSettings
            { ActionsToSubscribeTo = ActionsToSubscribeTo, EventSubscriber = this };
        
        inputManager.SubscribeToActions(inputSubscriberSettings);
    }

    public void InputPerformed(InputAction.CallbackContext callContext) 
        => ChangeViewBasedOnInput(callContext.action.name);

    private void ChangeViewBasedOnInput(string actionName)
    {
        var rotationDirection = GetRequestedViewDirection(actionName);
        RotateView(rotationDirection);
    }

    private RotationDirection GetRequestedViewDirection(string actionName)
    {
        if (actionName.Contains(RotationDirection.Left.ToString()))
            return RotationDirection.Left;
        if (actionName.Contains(RotationDirection.Right.ToString()))
            return RotationDirection.Right;
        if (actionName.Contains(RotationDirection.Down.ToString()))
            return RotationDirection.Down;

        return RotationDirection.Up;
    }

    private void RotateView(RotationDirection direction)
    {
        if (direction is RotationDirection.Down or RotationDirection.Up)
        {
            if (IsLookingInTargetUpDownDirection(direction)) return;

            var lookDown = direction == RotationDirection.Down;
            LookUpDownTween(lookDown);
            
            return;
        }
        
        LookUpDownTween(false);

        var rotateToLeft = direction == RotationDirection.Left;
        HandleLeftRightRotation(rotateToLeft);
    }

    private bool IsLookingInTargetUpDownDirection(RotationDirection direction)
    {
        if (direction == RotationDirection.Down)
            return isLookingDown;

        return !isLookingDown;
    }

    private void LookUpDownTween(bool lookDown)
    {
        upDownTween?.Kill();

        var targetAngle = lookDown ? downRotation : upRotation;
        var targetRotation = new Vector3(targetAngle, 0, 0);

        upDownTween = viewUpDownPivot.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(rotationEase);

        isLookingDown = lookDown;
    }

    private void HandleLeftRightRotation(bool rotateToLeft)
    {
        var rotationAngle = rotateToLeft ? leftRotation : rightRotation;

        var targetRotation = CalculateTargetRotation(rotationAngle);

        leftRightTween?.Kill();

        leftRightTween = viewLeftRightPivot.DORotate(targetRotation, rotationDuration, RotateMode.FastBeyond360)
            .SetEase(rotationEase)
            .OnComplete(() => lastTargetRotation = Vector3.zero);
    }

    private Vector3 CalculateTargetRotation(float rotationAngle)
    {
        var baseRotation = RotationTweenIsPlaying() ? lastTargetRotation : viewLeftRightPivot.eulerAngles;

        var targetRotation = baseRotation + new Vector3(0,rotationAngle, 0);

        lastTargetRotation = targetRotation;

        return targetRotation;
    }

    private bool RotationTweenIsPlaying() => leftRightTween != null && leftRightTween.IsPlaying();

    private void OnDestroy()
    {
        upDownTween?.Kill();
        leftRightTween?.Kill();
        
        if (inputManager == null) return;
        
        inputManager.UnsubscribeFromActions(inputSubscriberSettings);
    }
}