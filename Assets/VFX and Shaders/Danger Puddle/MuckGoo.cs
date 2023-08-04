using System;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

public class MuckGoo : MonoBehaviour, IPoolObject
{
    [SerializeField] private float backupMuckDisappearTime = 30f;
    [SerializeField] private EventReference muckExplosionSound;
    [SerializeField] private ParticleSystem muckExplosion;
    [SerializeField] private Collider muckCollider;
    
    [Header("Goo")]
    [SerializeField] private ParticleSystem gooParticles;
    [SerializeField] private StudioEventEmitter gooSoundEmitter;
    
    [Header("Fire")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private StudioEventEmitter fireSoundEmitter;

    private Transform gooTransform;
    private Transform fireTransform;
    private Sequence fireEndSequence;

    public PoolObjectContainer ContainerOfObject { get; set; }
    public bool IsGooOnFire { get; private set; }

    private Tween emitterSizeTween;
    private Tween backupMuckDisappearTween;

    private const float emitterSizeTweenDuration = 0.5f;
    private const float startEmitterDelay = 0.75f;

    private void Start()
    {
        gooTransform = gooParticles.transform;
        fireTransform = fireParticles.transform;
    }

    public void ResetInstance()
    {
        DOVirtual.DelayedCall(0.5f, PlayMuckExplosion);
        
        DOVirtual.DelayedCall(startEmitterDelay, PlayGooParticles);

        backupMuckDisappearTween = DOVirtual.DelayedCall(backupMuckDisappearTime, () => ContainerOfObject.ReturnToPool());
    }

    private void PlayGooParticles()
    {
        gooParticles.Play();
        gooSoundEmitter.Play();
        
        emitterSizeTween = gooTransform.DOScale(Vector3.one, emitterSizeTweenDuration);
    }

    private void PlayMuckExplosion()
    {
        muckExplosion.Play();
        RuntimeManager.PlayOneShot(muckExplosionSound, transform.position);
    }

    public void StartMuckFire()
    {
        gooSoundEmitter.Stop();
        
        emitterSizeTween?.Kill(true);
        
        emitterSizeTween = fireTransform.DOScale(Vector3.one, emitterSizeTweenDuration);
        
        DOVirtual.DelayedCall(startEmitterDelay, () =>
        {
            gooParticles.Stop();
            fireParticles.Play();
            fireSoundEmitter.Play();
            
            IsGooOnFire = true;
            ToggleMuckCollider();
        });

        StartFireEndSequence();
    }

    private void ToggleMuckCollider()
    {
        muckCollider.enabled = false;

        DOVirtual.DelayedCall(0.1f, () => muckCollider.enabled = true);
    }

    private void StartFireEndSequence()
    {
        fireEndSequence = DOTween.Sequence();
        
        fireEndSequence.AppendInterval(5);
        
        fireEndSequence.AppendCallback(StopFireParticleEmission);
        
        fireEndSequence.AppendInterval(3);
        
        fireEndSequence.AppendCallback(() => ContainerOfObject.ReturnToPool());
    }

    private void StopFireParticleEmission()
    {
        var fireEmission = fireParticles.emission;
        fireEmission.rateOverTimeMultiplier = 0;
    }

    public void OnReturnInstance()
    {
        IsGooOnFire = false;
        
        gooTransform.localScale = Vector3.zero;
        fireTransform.localScale = Vector3.zero;
        
        backupMuckDisappearTween?.Kill();
        
        StopParticles();
    }

    private void StopParticles()
    {
        muckExplosion.Stop();
        gooParticles.Stop();
        fireParticles.Stop();
        
        fireEndSequence?.Kill();
        emitterSizeTween?.Kill();
    }

    private void OnDestroy() => StopParticles();
}