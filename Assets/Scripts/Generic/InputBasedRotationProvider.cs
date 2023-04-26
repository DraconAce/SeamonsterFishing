using UnityEngine;

public static class InputBasedRotationProvider
{
    public static Vector3 CalculateRotationBasedOnInput(Vector2 input, float speed = 1f)
    {
        var rotationVector = new Vector3(-input.y, input.x, 0);
        var rotationToAdd = Time.deltaTime * speed * rotationVector;

        return rotationToAdd;
    }

    public static Quaternion ClampGivenRotationToLimits(RotationLimit limits, Vector3 rotationToClamp)
    {
        var eulerToClamp = IncludeNegativeInRotation(rotationToClamp);

        eulerToClamp.x = Mathf.Clamp(eulerToClamp.x, limits.MinLimit.x, limits.MaxLimit.x);
        eulerToClamp.y = Mathf.Clamp(eulerToClamp.y, limits.MinLimit.y, limits.MaxLimit.y);
        eulerToClamp.z = Mathf.Clamp(eulerToClamp.z, limits.MinLimit.z, limits.MaxLimit.z);
        
        return Quaternion.Euler(eulerToClamp);
    }

    private static Vector3 IncludeNegativeInRotation(Vector3 rotationToConvert)
    {
        rotationToConvert.x = AdjustRotationToNegative(rotationToConvert.x);
        rotationToConvert.y = AdjustRotationToNegative(rotationToConvert.y);
        rotationToConvert.z = AdjustRotationToNegative(rotationToConvert.z);

        return rotationToConvert;
    }

    private static float AdjustRotationToNegative(float rotationAxisValue)
    {
        return rotationAxisValue switch
        {
            > 180 => rotationAxisValue - 360,
            < -180 => rotationAxisValue + 360,
            _ => rotationAxisValue
        };
    }
}