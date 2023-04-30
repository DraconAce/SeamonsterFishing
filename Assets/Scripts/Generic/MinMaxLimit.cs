using System;
using UnityEngine;

[Serializable]
public struct MinMaxLimit
{
    public float MinLimit;
    public float MaxLimit;
}

[Serializable]
public struct RotationLimit
{
    public Vector3 MinLimit;
    public Vector3 MaxLimit;

    public Vector3 GetMinWithFactorModifier(Vector3 factorModifier) => ReturnLimitWithFactorModifier(factorModifier, MinLimit);

    private Vector3 ReturnLimitWithFactorModifier(Vector3 factorModifier, Vector3 limit)
    {
        var modifiedMin = limit;
        for (var i = 0; i < 3; i++)
        {
            modifiedMin[i] *= factorModifier[i];
        }

        return modifiedMin;
    }

    public Vector3 GetMaxWithFactorModifier(Vector3 factorModifier) => ReturnLimitWithFactorModifier(factorModifier, MaxLimit);
}