using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSingleton : Singleton<PlayerSingleton>
{
    [SerializeField] private Transform playerTransform;
    [FormerlySerializedAs("playerRepresentation")] [SerializeField] private Transform physicalPlayerRepresentation;

    public override bool AddToDontDestroy => false;

    public Transform PlayerTransform => playerTransform;

    public Transform PhysicalPlayerRepresentation => physicalPlayerRepresentation;
    
    public bool DisableMovementControls { get; set; }
    
    public DriveStation DriveStation { get; set; }
}