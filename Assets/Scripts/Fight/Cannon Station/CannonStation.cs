using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CannonStation_Aiming), typeof(CannonStation_Shooting))]
public class CannonStation : AbstractStation
{
    private CannonStation_Aiming aimingController;
    private CannonStation_Shooting shootingController;

    protected override void Start()
    {
        base.Start();
        
        SetupControllers();
    }

    private void SetupControllers()
    {
        TryGetComponent(out aimingController);
        TryGetComponent(out shootingController);
        
        aimingController.SetupController(this);
        shootingController.SetupController(this);
    }
}