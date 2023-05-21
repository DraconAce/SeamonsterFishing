using System;
using FMODUnity;
using UnityEngine;

public class CannonBallExplosion : MonoBehaviour, IPoolObject
{
    private ParticleSystem explosionParticle;
    private StudioEventEmitter explosionSound;
    
    public PoolObjectContainer ContainerOfObject { get; set; }

    private void Awake()
    {
        TryGetComponent(out explosionParticle);
        TryGetComponent(out explosionSound);
    }

    public void OnReturnInstance()
    {
        explosionParticle.Stop();
        
        explosionSound.Stop();
    }

    public void ResetInstance()
    {
        explosionParticle.Play();
        
        explosionSound.Play();
    }
}