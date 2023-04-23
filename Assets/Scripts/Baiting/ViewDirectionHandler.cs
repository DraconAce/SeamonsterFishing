using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewDirectionHandler : MonoBehaviour, IInputEventSubscriber
{
    private enum RotationDirection
    {
        Left = -1,
        Right = 1
    }
    
    [SerializeField] private Transform viewLeftRightPivot;
    [SerializeField] private string[] actionsToSubscribeTo = {"Change_View_Left", "Change_View_Right"};
    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    [SerializeField] private float rotationDuration;
    [SerializeField] private Ease rotationEase;
    
    public bool SubscribedToStarted => false;
    public bool SubscribedToPerformed => true;
    public bool SubscribedToCanceled => false;

    private const float rightRotation = 90;
    private const float leftRotation = -90;

    private InputManager inputManager;
    private InputManager.SubscriberSettings inputSubscriberSettings;

    private Vector3 lastTargetRotation;
    private Tween leftRightTween;
    private int lookDirection;

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
        return actionName.Contains(RotationDirection.Left.ToString()) ? RotationDirection.Left : RotationDirection.Right;
    }

    private void RotateView(RotationDirection direction)
    {
        if (!PlayerCanTurn(direction)) return;

        var rotateToLeft = direction == RotationDirection.Left;
        HandleLeftRightRotation(rotateToLeft);
    }

    private bool PlayerCanTurn(RotationDirection direction)
    {
        var nextLookDirection = lookDirection + (int)direction;

        if (Mathf.Abs(nextLookDirection) > 1) return false;

        lookDirection = nextLookDirection;
        return true;
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
        leftRightTween?.Kill();
        
        if (inputManager == null) return;
        
        inputManager.UnsubscribeFromActions(inputSubscriberSettings);
    }
}