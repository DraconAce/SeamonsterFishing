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
    
    private bool disableMovementControls;

    public bool DisableMovementControls
    {
        get => disableMovementControls;
        set
        {
            var formerValue = disableMovementControls;
            
            disableMovementControls = value;
            
            if(formerValue == disableMovementControls) return;
            
            MovementEnabledStateChanged?.Invoke(disableMovementControls);
        }
    }

    public DriveStation DriveStation { get; set; }
    
    public event Action<bool> MovementEnabledStateChanged;
}