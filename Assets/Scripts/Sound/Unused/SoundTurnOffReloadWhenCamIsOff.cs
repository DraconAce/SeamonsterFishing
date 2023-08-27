using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SoundTurnOffReloadWhenCamIsOff : MonoBehaviour
{   
    private CannonStation_Reload reloadScript;
    
    void Start()
    {
        reloadScript = this.transform.parent.GetComponent<CannonStation_Reload>();
    }
    void OnDisable()
    {
        //turn off reload sound if it is valid
        //reloadScript.turnOffReloadSound();
    }
}
