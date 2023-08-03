using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMuck : MonoBehaviour
{
    
    [SerializeField] private GameObject Muck_Spew_Gameobject;
    private ParticleSystem Muck_Spew;
    private Material mat_MuckSpew;
    private Vector3 pos_MuckSpew;
    [SerializeField] private GameObject Muck_Explosion_Gameobject;
    private ParticleSystem Muck_Explosion;
    private BoxCollider MuckCollider;
    [SerializeField] private GameObject GooParticles_Gameobject;
    [SerializeField] private GameObject FireParticles_Gameobject;
    private Transform playerTransform;
    
    private Vector3 savedPosition_Player;
    
    [SerializeField] private FMODUnity.EventReference MuckExplosionSound;
    [SerializeField] private FMODUnity.EventReference MuckFireBreathSound;
    
    void Start()
    {
        Muck_Explosion = Muck_Explosion_Gameobject.GetComponent<ParticleSystem>();
        Muck_Spew = Muck_Spew_Gameobject.GetComponent<ParticleSystem>();
        mat_MuckSpew = Muck_Spew_Gameobject.GetComponent<Renderer>().material;
        pos_MuckSpew = Muck_Spew_Gameobject.transform.position;
        
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();
        MuckCollider = Muck_Explosion_Gameobject.transform.GetChild(0).GetComponent<BoxCollider>();
        
        StartCoroutine(MuckTimeTracker());
    }
    
    IEnumerator MuckTimeTracker()
    {
        savedPosition_Player = playerTransform.position;
        RotateMuckSpewTowardsSavedPlayerPos();
        
        //set Transform of Muck_Explosion (and Puddle) to current player position
        Muck_Explosion_Gameobject.transform.position = savedPosition_Player;
        
        //wait for play MuckExplosion
        yield return new WaitForSeconds(0.9f);
        Muck_Explosion.Play();
        FMODUnity.RuntimeManager.PlayOneShot(MuckExplosionSound, Muck_Explosion_Gameobject.transform.position);
        
        //short wait for activate Muck_Puddle
        yield return new WaitForSeconds(0.1f);
        GooParticles_Gameobject.SetActive(true);
        
        //Testing fire after 5s
        yield return new WaitForSeconds(5f);
        
        //after Monster creates MuckOrigin: save a reference to MuckOrigin 
        //and call StartFireBreath with its (new) origin(Vector3), then a short delay into StartFireMuck when the goo should burn
        StartFireBreathFromPosition(Muck_Spew_Gameobject.transform.position);
        yield return new WaitForSeconds(0.2f);
        StartFireMuck();
    }
    
    private void RotateMuckSpewTowardsSavedPlayerPos()
    {
        //rotate Muck_Spew towards where the player used to be
        var direction = (savedPosition_Player - pos_MuckSpew).normalized;
        var targetRotation = Quaternion.LookRotation(direction, Vector3.forward);
        Muck_Spew_Gameobject.transform.rotation = targetRotation;
        
        AdjustMuckSpewStrength();
    }
    
    private void AdjustMuckSpewStrength()
    {
        //Adjust the Muck Spew initial Speed -> speed up the spew if the player is further away
        var distance = savedPosition_Player - pos_MuckSpew;
        //Debug.Log("Spew distance: " + distance);
        float bonusSpeed = distance[0] - distance[1] - distance[2]; //positive x, negative y and z = further away
        var main = Muck_Spew.main;
        main.startSpeed = bonusSpeed;
    }
    
    public void StartFireBreathFromPosition(Vector3 fireOrigin)
    {
        //set firespew origin
        pos_MuckSpew = fireOrigin;
        Muck_Spew_Gameobject.transform.position = fireOrigin;
        //Muck Spews towards old player position -> the Muck that is supposed to burn
        //also automatically adjusts Strength
        RotateMuckSpewTowardsSavedPlayerPos();
        
        mat_MuckSpew.EnableKeyword("_EMISSION");
        Muck_Spew.Play();
        FMODUnity.RuntimeManager.PlayOneShot(MuckFireBreathSound, pos_MuckSpew);
    }
    
    public void StartFireMuck()
    {
        FireParticles_Gameobject.SetActive(true);
        //Reset Trigger to cause "OnTriggerEnter" to hurt player if they are already inside the goo
        MuckCollider.enabled = false;
        MuckCollider.enabled = true;
    }
}
