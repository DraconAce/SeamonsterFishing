using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CannonStation_Aiming), typeof(CannonStation_Shooting), typeof(CannonStation_Reload))]
public class CannonStation : AbstractStation
{
    [SerializeField] private Transform cannonPivot;
    public Transform CannonPivot => cannonPivot;
}