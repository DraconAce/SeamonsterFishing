using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lightning_Manager : MonoBehaviour
{
    
    [SerializeField] private Light spotLight;
    
    [SerializeField] private FMODUnity.EventReference LightningSound;
    
    [SerializeField] private ParticleSystem LightningParticleSystem;

    private Sequence lightningSequence;
    [Header("Flash Settings")] 
    [SerializeField] float minCooldown = 2f;
    [SerializeField] float maxCooldown = 5f;
    [SerializeField] private float targetIntensity = 100f;
    [SerializeField] private float keepLightningOnDuration = 0.2f;
    [SerializeField] private TweenSettings lightningSettings;

    private bool lightningIsActive;
    private IEnumerator lightningCoroutine;

    private void Start() 
    {
        lightningIsActive = true;
        lightningCoroutine = DoLightning();
        StartCoroutine(lightningCoroutine);
        LightningParticleSystem.Stop();
    }

    private IEnumerator DoLightning() 
    { 
        while (lightningIsActive)
        {
            float waitTime = Random.Range(minCooldown, maxCooldown);
            //Debug.Log("Lightning wait:" + waitTime);
            yield return new WaitForSeconds(waitTime);
            createLightning();
        }
    }

    void createLightning()
    {
        lightningSequence = DOTween.Sequence();
        lightningSequence.Append(LightningTween(targetIntensity, lightningSettings));
        lightningSequence.AppendInterval(keepLightningOnDuration);
        lightningSequence.Append(LightningTween(0f, lightningSettings));

        //play Lightning particle at random z-Location
        float z = Random.Range(-150f, 150f);
        LightningParticleSystem.transform.position = new Vector3(-125f,30f,z);
        LightningParticleSystem.Play();
        FMODUnity.RuntimeManager.PlayOneShot(LightningSound);
        //put Spotlight at Lightning location
        spotLight.transform.position = new Vector3(-125f,30f,z);
    }

    private Tween LightningTween(float targetIntensity, TweenSettings targetSettings)
    {
        return spotLight.DOIntensity(targetIntensity, targetSettings.Duration)
            .SetEase(targetSettings.TweenEase)
            .OnStart(() => targetSettings.OnStartAction?.Invoke())
            .OnComplete(() => targetSettings.OnCompleteAction?.Invoke());
    }

    private void OnDestroy()
    {
        lightningSequence?.Kill();
    }

}