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
}