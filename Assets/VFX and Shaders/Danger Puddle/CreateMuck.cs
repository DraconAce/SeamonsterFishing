using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMuck : MonoBehaviour
{
    
    [SerializeField] private GameObject Muck_Spew_Gameobject;
    private ParticleSystem Muck_Spew;
    private Material mat_MuckSpew;
    [SerializeField] private GameObject Muck_Explosion_Gameobject;
    private ParticleSystem Muck_Explosion;
    private BoxCollider MuckCollider;
    [SerializeField] private GameObject GooParticles_Gameobject;
    [SerializeField] private GameObject FireParticles_Gameobject;
    private Transform playerTransform;
    
    [SerializeField] private FMODUnity.EventReference MuckExplosionSound;
    
    void Start()
    {
        Muck_Explosion = Muck_Explosion_Gameobject.GetComponent<ParticleSystem>();
        Muck_Spew = Muck_Spew_Gameobject.GetComponent<ParticleSystem>();
        mat_MuckSpew = Muck_Spew_Gameobject.GetComponent<Renderer>().material;
        
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();
        MuckCollider = Muck_Explosion_Gameobject.transform.GetChild(0).GetComponent<BoxCollider>();
        
        StartCoroutine(MuckTimeTracker());
    }
    
    IEnumerator MuckTimeTracker()
    {
        RotateMuckSpewTowardsPlayer();
        
        //set Transform of Muck_Explosion (and Puddle) to current player position
        Muck_Explosion_Gameobject.transform.position = playerTransform.position;
        
        //wait for play MuckExplosion
        yield return new WaitForSeconds(0.9f);
        Muck_Explosion.Play();
        FMODUnity.RuntimeManager.PlayOneShot(MuckExplosionSound, Muck_Explosion_Gameobject.transform.position);
        
        //short wait for activate Muck_Puddle
        yield return new WaitForSeconds(0.1f);
        GooParticles_Gameobject.SetActive(true);
        
        //Testing fire after 3s
        yield return new WaitForSeconds(3f);
        
        //after Monster creates MuckOrigin: save a reference to MuckOrigin 
        //and call StartFireBreath, then a short delay into StartFireMuck when the goo should burn
        StartFireBreath();
        yield return new WaitForSeconds(0.2f);
        StartFireMuck();
    }
    
    private void RotateMuckSpewTowardsPlayer()
    {
        //rotate Muck_Spew towards player
        var direction = (playerTransform.position - Muck_Spew_Gameobject.transform.position).normalized;
        var targetRotation = Quaternion.LookRotation(direction, Vector3.forward);
        Muck_Spew_Gameobject.transform.rotation = targetRotation;
    }
    
    public void StartFireBreath()
    {
        //Muck Spews towards old player position -> the Muck that is supposed to burn
        mat_MuckSpew.EnableKeyword("_EMISSION");
        Muck_Spew.Play();
    }
    
    public void StartFireMuck()
    {
        FireParticles_Gameobject.SetActive(true);
        //Reset Trigger to cause "OnTriggerEnter" to hurt player if they are already inside the goo
        MuckCollider.enabled = false;
        MuckCollider.enabled = true;
    }
}
