using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class Lightning_Manager : MonoBehaviour
{
    
    [SerializeField] private GameObject Monster;
    private Outline outlineComponent;
    [SerializeField] private float outlineWidth;
    [SerializeField] private float AppearOutlineDuration = 0.5f;
    [SerializeField] private float DisappearOutlineDuration = 0.2f;

    [SerializeField] private Light spotLight;
    
    [SerializeField] private EventReference LightningSound;
    
    [SerializeField] private ParticleSystem LightningParticleSystem;

    [SerializeField] private int chanceMultipleLightning = 50;
    [SerializeField] private float delayMultipleLightning = 0.5f;
    [SerializeField] private int maxConsecutiveLightnings = 4;

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

        outlineComponent = Monster.GetComponent<Outline>();
        outlineComponent.OutlineWidth = outlineWidth;
    }

    private IEnumerator DoLightning() 
    { 
        while (lightningIsActive)
        {
            float waitTime = Random.Range(minCooldown, maxCooldown);
            //Debug.Log("Lightning wait:" + waitTime);
            yield return new WaitForSeconds(waitTime);
            createLightning();
            for (int i = 1; i < maxConsecutiveLightnings; i++)
            {
                int randomMoreLightning = Random.Range(0, 100);
                if (randomMoreLightning < chanceMultipleLightning) 
                {
                    yield return new WaitForSeconds(delayMultipleLightning);
                    createLightning();
                }
            }

        }
    }

    void createLightning()
    {
        lightningSequence = DOTween.Sequence();
        lightningSequence.Append(LightningTweenOne(targetIntensity, lightningSettings));
        lightningSequence.AppendInterval(keepLightningOnDuration);
        lightningSequence.Append(LightningTweenTwo(0f, lightningSettings));

        //play Lightning particle at random z-Location
        float z = Random.Range(-150f, 150f);
        Vector3 lightningPositionVector = new Vector3(-125f,30f,z);
        LightningParticleSystem.transform.position = lightningPositionVector;
        LightningParticleSystem.Play();
        RuntimeManager.PlayOneShot(LightningSound, lightningPositionVector);
        //put Spotlight at Lightning location
        spotLight.transform.position = new Vector3(-125f,80f,z);
    }

    private Tween LightningTweenOne(float targetIntensity, TweenSettings targetSettings)
    {
        return spotLight.DOIntensity(targetIntensity, targetSettings.Duration)
            .SetEase(targetSettings.TweenEase)
            .OnStart(() =>
            {
                targetSettings.OnStartAction?.Invoke();
                //outlineComponent.OutlineWidth = 3;
                DOVirtual.Color(new Color(1,1,1,0), new Color(1,1,1,1), AppearOutlineDuration, (colorValue) =>
                {
                    outlineComponent.OutlineColor = colorValue;
                });
            });
    }

    private Tween LightningTweenTwo(float targetIntensity, TweenSettings targetSettings)
    {
        return spotLight.DOIntensity(targetIntensity, targetSettings.Duration)
            .SetEase(targetSettings.TweenEase)
            .OnComplete(() =>
            { 
                targetSettings.OnCompleteAction?.Invoke();
                //outlineComponent.OutlineWidth = 0;
                DOVirtual.Color(new Color(1,1,1,1), new Color(1,1,1,0), DisappearOutlineDuration, (colorValue) =>
                {
                    outlineComponent.OutlineColor = colorValue;
                });
            });
    }

    private void OnDestroy()
    {
        lightningSequence?.Kill();
    }

}