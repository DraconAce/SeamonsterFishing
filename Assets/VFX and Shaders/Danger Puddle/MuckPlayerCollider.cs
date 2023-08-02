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
            //damage player?
            if (other.transform.GetChild(3).gameObject.activeSelf) //child3 is FireParticles
            {
                Debug.Log("Destroy Player because of Fire");
            }
            
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
