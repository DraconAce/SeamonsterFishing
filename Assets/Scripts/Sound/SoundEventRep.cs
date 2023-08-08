using System;
using FMODUnity;

[Serializable]
public struct SoundEventRep
{
    public EventReference eventRef;
        
    private float cachedLength;

    private bool lengthWasCached;

    public float GetSoundLength()
    {
        if (lengthWasCached) return cachedLength;

        cachedLength = SoundHelper.GetSoundLength(eventRef);
        lengthWasCached = true;

        return cachedLength;
    }
}