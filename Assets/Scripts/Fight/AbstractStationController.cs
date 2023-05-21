using UnityEngine;

public abstract class AbstractStationController : MonoBehaviour
{
    protected AbstractStation ControllerStation { get; private set; }

    public void SetupController(AbstractStation station)
    {
        ControllerStation = station;
        
        OnControllerSetup();
    }
    
    protected virtual void OnControllerSetup(){}
}