using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMuck : MonoBehaviour
{

    [SerializeField] private GameObject Muck_Explosion_Gameobject;
    private ParticleSystem Muck_Explosion;
    [SerializeField] private GameObject GooParticles_Gameobject;
    [SerializeField] private GameObject FireParticles_Gameobject;
    
    void Start()
    {
        Muck_Explosion = Muck_Explosion_Gameobject.GetComponent<ParticleSystem>();
        StartCoroutine(MuckTimeTracker());
    }
    
    IEnumerator MuckTimeTracker()
    {
        //wait for play MuckExplosion
        yield return new WaitForSeconds(0.9f);
        Muck_Explosion.Play();
        
        //short wait for activate Muck_Puddle
        yield return new WaitForSeconds(0.1f);
        GooParticles_Gameobject.SetActive(true);
        
        //Testing fire after 2s
        yield return new WaitForSeconds(2f);
        FireParticles_Gameobject.SetActive(true);
    }
    
}
