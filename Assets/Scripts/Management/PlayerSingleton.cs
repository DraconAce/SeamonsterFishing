using System;
using Cinemachine;
using UnityEngine;

public class PlayerSingleton : Singleton<PlayerSingleton>
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerRepresentation;

    public override bool AddToDontDestroy => false;

    public Transform PlayerTransform => playerTransform;

    public Transform PlayerRepresentation => playerRepresentation;
    
    public bool DisableMovementControls { get; set; }
    
    public DriveStation DriveStation { get; set; }
}