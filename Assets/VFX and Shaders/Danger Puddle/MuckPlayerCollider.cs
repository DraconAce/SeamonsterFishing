using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuckPlayerCollider : MonoBehaviour
{
    [SerializeField] private GameObject DriveStation;
    private DriveStation_Moving DriveScript;
    [SerializeField] private FMODUnity.EventReference BoatFireHitSound;
    
    //Boat burning particles
    [SerializeField] private GameObject FireParticles;
    private GameObject FirstBoatBurn;
    private GameObject SecondBoatBurn;
    private GameObject ThirdBoatBurn;
    [SerializeField] private float fireHeight = 0.5f;
    
    private IEnumerator boatBurningCoroutine;
    
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
                //Debug.Log("Destroy Player because of Fire");
                FMODUnity.RuntimeManager.PlayOneShot(BoatFireHitSound, other.ClosestPoint(transform.position));
                boatBurningCoroutine = DoBoatBurning();
                StartCoroutine(boatBurningCoroutine);
            }
            
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "MuckPuddle")
        {
            //stop slowing down player
            DriveScript.SetBoatInMuck(false);
            
            if (other.transform.GetChild(3).gameObject.activeSelf) //child3 is FireParticles
            {
                StopCoroutine(boatBurningCoroutine);
                Destroy(FirstBoatBurn);
                Destroy(SecondBoatBurn);
                Destroy(ThirdBoatBurn);
            }
        }
    }
    
    private IEnumerator DoBoatBurning() 
    { 
        //Debug.Log("Boat is burning");
        
        //instant fire
        FirstBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, fireHeight, 0), Quaternion.identity);
        FirstBoatBurn.transform.parent = transform;
        
        yield return new WaitForSeconds(0.6f);
        
        //more fire
        //Debug.Log("Burning State 2");
        SecondBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, 2*fireHeight, 0), Quaternion.identity);
        SecondBoatBurn.transform.parent = transform;
        
        yield return new WaitForSeconds(0.6f);
        
        //more fire
        //Debug.Log("Burning State 3");
        ThirdBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, 3*fireHeight, 0), Quaternion.identity);
        ThirdBoatBurn.transform.parent = transform;
        
        yield return new WaitForSeconds(0.8f);
        //Debug.Log("Boat is DEAD");
        GameStateManager.instance.ChangeGameState(GameState.Dead);
    }
    
}
