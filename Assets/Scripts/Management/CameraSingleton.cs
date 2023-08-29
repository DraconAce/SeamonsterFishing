using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraSingleton : Singleton<CameraSingleton>
{
    [SerializeField] private CinemachineVirtualCamera mainMainPlayerCamera;
    private CinemachineBrain cameraBrain;

    public override bool AddToDontDestroy => false;
    public Camera OutputCamera => cameraBrain.OutputCamera;
    public CinemachineVirtualCamera MainPlayerCamera => mainMainPlayerCamera;

    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out cameraBrain);
    }
}