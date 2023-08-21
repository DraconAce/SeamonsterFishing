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
    private PlayerEnteredMuckTracker playerEnteredMuckTracker;

    private Tween emitterSizeTween;
    private Tween backupMuckDisappearTween;
    private Sequence endFireSequence;

    private const float EmitterSizeTweenDuration = 0.5f;
    private const float StartEmitterDelay = 0.75f;
    
    public bool IsGooOnFire { get; private set; }
    public PoolObjectContainer ContainerOfObject { get; set; }
    
    private void Start()
    {
        gooTransform = gooParticles.transform;
        fireTransform = fireParticles.transform;

        playerEnteredMuckTracker = GetComponentInChildren<PlayerEnteredMuckTracker>();
    }

    public void ResetInstance()
    {
        muckCollider.enabled = true;
        
        DOVirtual.DelayedCall(0.5f, PlayMuckExplosion);
        
        DOVirtual.DelayedCall(StartEmitterDelay, PlayGooParticles);

        backupMuckDisappearTween = DOVirtual.DelayedCall(backupMuckDisappearTime, () => ContainerOfObject.ReturnToPool());
    }

    private void PlayGooParticles()
    {
        gooParticles.Play();
        gooSoundEmitter.Play();
        
        emitterSizeTween = StartScaleToOneTween(gooTransform);
    }

    private Tween StartScaleToOneTween(Transform target) => target.DOScale(Vector3.one, EmitterSizeTweenDuration);

    private void PlayMuckExplosion()
    {
        muckExplosion.Play();
        RuntimeManager.PlayOneShot(muckExplosionSound, transform.position);
    }

    public void StartMuckFire()
    {
        emitterSizeTween?.Kill(true);
        emitterSizeTween = StartScaleToOneTween(fireTransform);
        
        DOVirtual.DelayedCall(StartEmitterDelay, () =>
        {
            StopParticlesAndSound(gooSoundEmitter, gooParticles);

            StartGooFireParticles();
            ToggleMuckCollider();
        });

        StartFireEndSequence();
    }
    
    private void StopParticlesAndSound(StudioEventEmitter sound, ParticleSystem particles)
    {
        sound.Stop();
        particles.Stop();
    }

    private void StartGooFireParticles()
    {
        fireParticles.Play();
        fireSoundEmitter.Play();

        IsGooOnFire = true;
    }

    private void ToggleMuckCollider()
    {
        muckCollider.enabled = false;

        DOVirtual.DelayedCall(0.1f, () => muckCollider.enabled = true);
    }

    private void StartFireEndSequence()
    {
        endFireSequence = DOTween.Sequence();
        
        endFireSequence.AppendInterval(5);
        
        endFireSequence.AppendCallback(StopFireParticleEmission);
        
        endFireSequence.AppendInterval(3);
        
        endFireSequence.AppendCallback(() => ContainerOfObject.ReturnToPool());
    }

    private void StopFireParticleEmission()
    {
        muckCollider.enabled = false;
        
        playerEnteredMuckTracker.InterruptMuckEffects();
        
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
        
        playerEnteredMuckTracker.InterruptMuckEffects();
    }

    private void StopParticles()
    {
        endFireSequence?.Kill();
        emitterSizeTween?.Kill();
        
        StopParticlesAndSound(gooSoundEmitter, gooParticles);
        StopParticlesAndSound(fireSoundEmitter, fireParticles);

        muckExplosion.Stop();
    }

    private void OnDestroy() => StopParticles();
}