using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public static class SoundHelper
{
    public static EventInstance CreateSoundInstanceAndAttachToTransform(EventReference sound, GameObject attachOb)
    {
        var soundInstance = RuntimeManager.CreateInstance(sound);
        soundInstance.set3DAttributes(attachOb.To3DAttributes());
        
        RuntimeManager.AttachInstanceToGameObject(soundInstance, attachOb.transform);

        return soundInstance;
    }

    public static float GetSoundLength(EventReference eventRef)
    {
        var eventDescription = eventRef.Guid == default ? default : RuntimeManager.GetEventDescription(eventRef);

        if (!eventDescription.isValid()) return 2f;

        eventDescription.getLength(out var timeInMillis);

        return timeInMillis / 1000f;
    }
    
    public static void StopAndReleaseInstance(EventInstance instance, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
    {
        instance.stop(stopMode);
        instance.release();
    }
    
    public static void SetEventVolume(EventInstance instance, float volume)
    {
        instance.setVolume(volume);
    }
}