using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class MuckPlayerCollider : MonoBehaviour
{
    [SerializeField] private GameObject DriveStation;
    private DriveStation_Moving DriveScript;
    [SerializeField] private FMODUnity.EventReference BoatFireHitSound;
    [SerializeField] private FMODUnity.EventReference BoatBurnDeathSound;
    
    //Boat burning particles
    [SerializeField] private GameObject FireParticles;
    private GameObject FirstBoatBurn;
    private GameObject SecondBoatBurn;
    private GameObject ThirdBoatBurn;
    [SerializeField] private float fireHeight = 0.5f;
    [SerializeField] private EventReference boatBurningSound;
    private EventInstance boatBurningSoundInstance;
    
    private IEnumerator boatBurningCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        DriveScript = DriveStation.GetComponent<DriveStation_Moving>();
        boatBurningSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(boatBurningSound, this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var collidedOb = other.gameObject;
        if (!collidedOb.CompareTag("MuckPuddle")) return;
        
        //slow down player
        DriveScript.SetBoatInMuck(true);

        var muck = collidedOb.GetComponentInParent<MuckGoo>();
        //damage player?
        if (muck == null || !muck.IsGooOnFire) return;

        //remember Puddle to destroy when GameOver
        RuntimeManager.PlayOneShot(BoatFireHitSound, other.ClosestPoint(transform.position));
        boatBurningCoroutine = DoBoatBurning();
        StartCoroutine(boatBurningCoroutine);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("MuckPuddle")) return;
        
        InterruptMuckEffects();
    }

    public void InterruptMuckEffects()
    {
        //stop slowing down player
        DriveScript.SetBoatInMuck(false);

        StopBoatBurn();
    }

    private void StopBoatBurn()
    {
        if (boatBurningCoroutine != null) StopCoroutine(boatBurningCoroutine);

        if(FirstBoatBurn != null) Destroy(FirstBoatBurn);
        if(SecondBoatBurn != null) Destroy(SecondBoatBurn);
        if(ThirdBoatBurn != null) Destroy(ThirdBoatBurn);

        //stop sound
        boatBurningSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private IEnumerator DoBoatBurning() 
    { 
        //Debug.Log("Boat is burning");
        
        //instant fire
        FirstBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, fireHeight, 0), Quaternion.identity);
        FirstBoatBurn.transform.parent = transform;
        
        boatBurningSoundInstance.setParameterByName("BurnState", (int) 1);
        boatBurningSoundInstance.start();
        
        yield return new WaitForSeconds(0.6f);
        
        //more fire
        //Debug.Log("Burning State 2");
        SecondBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, 2*fireHeight, 0), Quaternion.identity);
        SecondBoatBurn.transform.parent = transform;
        //make fire louder
        boatBurningSoundInstance.setParameterByName("BurnState", (int) 2);
        
        yield return new WaitForSeconds(0.6f);
        
        //more fire
        //Debug.Log("Burning State 3");
        ThirdBoatBurn = (GameObject)Instantiate(FireParticles, transform.position + new Vector3(0, 3*fireHeight, 0), Quaternion.identity);
        ThirdBoatBurn.transform.parent = transform;
        //more fire
        boatBurningSoundInstance.setParameterByName("BurnState", (int) 3);
        
        yield return new WaitForSeconds(0.8f);
        //Debug.Log("Boat is DEAD");
        
        GameStateManager.instance.ChangeGameState(GameState.Dead);
        
        //Play Death Sound at player location
        RuntimeManager.PlayOneShot(BoatBurnDeathSound, this.transform.position);
        
        boatBurningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        boatBurningSoundInstance.release();
        //Destroy Muck_Puddle Sound to destroy looping sounds after Game-Over
        //Alternatively Stop the sound-loops directly
        //remember_Muck_Puddle.gameObject.transform.GetChild(2).gameObject.GetComponent<StudioEventEmitter>().Stop();
        //remember_Muck_Puddle.gameObject.transform.GetChild(3).gameObject.GetComponent<StudioEventEmitter>().Stop();
    }
    
    void OnDestroy() {
        boatBurningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        boatBurningSoundInstance.release();
    }
}
