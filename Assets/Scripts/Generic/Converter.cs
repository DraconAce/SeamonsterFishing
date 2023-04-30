public static class Converter
{
    public static float AdjustRotationToNegative(float rotationAxisValue)
    {
        return rotationAxisValue switch
        {
            > 180 => rotationAxisValue - 360,
            < -180 => rotationAxisValue + 360,
            _ => rotationAxisValue
        };
    }
}