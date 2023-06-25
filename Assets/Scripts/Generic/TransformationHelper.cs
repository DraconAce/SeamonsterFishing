using UnityEngine;

public static class TransformationHelper
{
    public static Vector3 RotateAroundPivot(Vector3 pivotPoint, Vector3 pointToRotate, Vector3 angles) 
        => Quaternion.Euler(angles) * (pointToRotate - pivotPoint) + pivotPoint;
}