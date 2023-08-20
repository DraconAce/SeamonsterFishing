using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

[Serializable]
public class SoundEventRep
{
    public EventReference eventRef;
    
    private EventInstance soundInstance;
    public EventInstance SoundInstance => soundInstance;
        
    private float cachedLength;

    private bool lengthWasCached;

    public void CreateInstanceForSound(GameObject attachOb = null) 
    {
        soundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(eventRef, attachOb);
    }

    public void StartInstance()
    {
        if (!soundInstance.isValid()) return;
        
        soundInstance.start();
    }

    public void StopInstance(STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
    {
        if (!soundInstance.isValid()) return;

        soundInstance.stop(stopMode);
    }
    
    public void StopAndReleaseInstance(STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
    {
        if (!soundInstance.isValid()) return;

        SoundHelper.StopAndReleaseInstance(soundInstance, stopMode);
    }

    public float GetSoundLength()
    {
        if (lengthWasCached) return cachedLength;

        cachedLength = SoundHelper.GetSoundLength(eventRef);
        lengthWasCached = true;

        return cachedLength;
    }
    
    public void SetVolume(float volume)
    {
        if (!soundInstance.isValid()) return;
        
        SoundHelper.SetEventVolume(soundInstance, volume);
    }
}