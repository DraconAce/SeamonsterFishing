using DG.Tweening;
using UnityEngine;
using System.Collections;

public class CannonStation_Reload : AbstractStationSegment
{
    [SerializeField] private float autoReloadDelay = 2f;
    [SerializeField] private GameObject fuse;
    private Material fuseMat;
    private IEnumerator burnFuseCoroutine;
    private float shootDelayForFuse;
    
    public bool IsLoaded { get; set; } = true;
    public bool IsReloading { get; private set; }
    public bool CannonIsPrepared => !IsReloading && IsLoaded;
    
    private Tween reloadingTween;
    private Tween autoReloadDelayTween;

    [SerializeField] private SoundEventRep reloadSound;

    private float reloadSoundLength;

    private CannonStation cannonStation => (CannonStation)ControllerStation;

    private void Start() 
    {
        //Fetch the Material from the Renderer of the fuse-GameObject
        fuseMat = fuse.GetComponent<Renderer>().sharedMaterial;
        shootDelayForFuse /= 100; // set fuse burntime
    } 

    protected override void OnControllerSetup()
    {
        base.OnControllerSetup();
        
        reloadSound.CreateInstanceForSound(cannonStation.CannonPivot.gameObject);
        reloadSoundLength = reloadSound.GetSoundLength();
    }

    public void AutoReload()
    {
        autoReloadDelayTween?.Kill();
        autoReloadDelayTween = DOVirtual.DelayedCall(autoReloadDelay, TryToReload);
    }
    
    private void TryToReload()
    {
        if (IsLoaded || IsReloading) return;

        IsReloading = true;
        
        //play sound
        reloadSound.StartInstance();

        reloadingTween = DOVirtual.DelayedCall(reloadSoundLength, Reload, false);
        
        InvokeSegmentStateChangedEvent();
    }

    private void Reload()
    {
        IsLoaded = true;
        IsReloading = false;
        
        //stop sound
        reloadSound.StopInstance();

        //turn fuse back on
        resetFuse();
        
        InvokeSegmentStateChangedEvent();
    }
    
    private void resetFuse()
    {
        fuseMat.SetFloat("_isFuseActive", 0f);
        fuseMat.SetFloat("_Transparancy", 1f);
    }
    
    public void burnFuse() 
    {
        burnFuseCoroutine = DoFuseBurn();
        StartCoroutine(burnFuseCoroutine);
    }
    
    public void SetShootDelayforFuse(float delay)
    {
        shootDelayForFuse = delay/100;
    }
    
    private IEnumerator DoFuseBurn()
    {
        fuseMat.SetFloat("_isFuseActive", 1f);
        shootDelayForFuse -= 0.003f; //slightly reduce the delay (Coroutine and Tween seem to run on different times)
        //fuseTime in %
        for (float fuseTime = 100f; fuseTime >= 0f; fuseTime--)
        {
            fuseMat.SetFloat("_Transparancy", fuseTime/100f);
            yield return new WaitForSeconds(shootDelayForFuse); //100*0.03f = 3 seconds of shoot delay
        }
        fuseMat.SetFloat("_isFuseActive", 0f);
        yield return null;
        //StopCoroutine(burnFuseCoroutine);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        //StopCoroutine(burnFuseCoroutine);
        resetFuse();
        
        reloadingTween?.Kill();
        autoReloadDelayTween?.Kill();
        
        reloadSound.StopAndReleaseInstance();
    }
}