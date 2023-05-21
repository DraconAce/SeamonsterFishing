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
    
    public static Quaternion ClampGivenRotationToLimits(RotationLimit limits, Vector3 rotationToClamp, Vector3 additionModifier, Vector3 factorModifier)
    {
        var eulerToClamp = IncludeNegativeInRotation(rotationToClamp);

        var modifiedMinLimit = limits.GetMinWithFactorModifier(factorModifier) + additionModifier;
        var modifiedMaxLimit = limits.GetMaxWithFactorModifier(factorModifier) + additionModifier;

        eulerToClamp.x = Mathf.Clamp(eulerToClamp.x, Mathf.Min(modifiedMinLimit.x, modifiedMaxLimit.x), Mathf.Max(modifiedMinLimit.x, modifiedMaxLimit.x));
        eulerToClamp.y = Mathf.Clamp(eulerToClamp.y, Mathf.Min(modifiedMinLimit.y, modifiedMaxLimit.y), Mathf.Max(modifiedMinLimit.y, modifiedMaxLimit.y));
        eulerToClamp.z = Mathf.Clamp(eulerToClamp.z, Mathf.Min(modifiedMinLimit.z, modifiedMaxLimit.z), Mathf.Max(modifiedMinLimit.z, modifiedMaxLimit.z));
        
        return Quaternion.Euler(eulerToClamp);
    }

    private static Vector3 IncludeNegativeInRotation(Vector3 rotationToConvert)
    {
        rotationToConvert.x = Converter.AdjustRotationToNegative(rotationToConvert.x);
        rotationToConvert.y = Converter.AdjustRotationToNegative(rotationToConvert.y);
        rotationToConvert.z = Converter.AdjustRotationToNegative(rotationToConvert.z);

        return rotationToConvert;
    }
}