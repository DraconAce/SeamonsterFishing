using System.Text;

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

    public static float GetAngleFrom0To360(float signedAngle)
    {
        if (signedAngle < 0)
        {
            signedAngle = 360 + signedAngle;
        }

        return signedAngle;
    }
}