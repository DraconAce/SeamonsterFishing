public class ViewDirectionGamepadControls : AbstractViewDirectionControls
{
    private string leftString;
    public override PlayerDevice ControlDevice => PlayerDevice.Gamepad;

    private void Start() => leftString = ViewDirectionHandler.RotationDirection.Left.ToString();

    protected override void ChangeViewBasedOnInput(string actionName)
    {
        var rotationDirection = GetRequestedViewDirection(actionName);
        
        ViewDirectionHandler.RotateView(rotationDirection);
    }

    private ViewDirectionHandler.RotationDirection GetRequestedViewDirection(string actionName)
    {
        return actionName.Contains(leftString) ? ViewDirectionHandler.RotationDirection.Left 
            : ViewDirectionHandler.RotationDirection.Right;
    }
}