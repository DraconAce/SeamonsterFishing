using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class SoundHelper
{
    public static EventInstance CreateSoundInstanceAndAttachToTransform(EventReference sound, GameObject attachOb)
    {
        var soundInstance = RuntimeManager.CreateInstance(sound);
        soundInstance.set3DAttributes(attachOb.To3DAttributes());
        
        RuntimeManager.AttachInstanceToGameObject(soundInstance, attachOb.transform);

        return soundInstance;
    }
}