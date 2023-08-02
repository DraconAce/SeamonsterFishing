using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuckPlayerCollider : MonoBehaviour
{
    
    [SerializeField] private GameObject DriveStation;
    private DriveStation_Moving DriveScript;
    
    // Start is called before the first frame update
    void Start()
    {
        DriveScript = DriveStation.GetComponent<DriveStation_Moving>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "MuckPuddle")
        {
            //slow down player
            DriveScript.SetBoatInMuck(true);
            
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "MuckPuddle")
        {
            //slow down player
            DriveScript.SetBoatInMuck(false);
        }
    }
}
