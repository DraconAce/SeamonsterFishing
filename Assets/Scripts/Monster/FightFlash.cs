using FMOD.Studio;
using FMODUnity;

public class FightFlash : SpotFlash
{
    public EventReference flashSound;
    public EventReference reloadFlashSound;

    private EventInstance flashSoundInstance;
    private EventInstance reloadFlashSoundInstance;
    
    private FightMonsterSingleton monsterSingleton;

    protected override void Start()
    {
        monsterSingleton = FightMonsterSingleton.instance;
        
        flashSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(flashSound, gameObject);
        reloadFlashSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(reloadFlashSound, gameObject);
        
        base.Start();
    }

    protected override void FlashActivatedImpl()
    {
        //start playing flash sound
        flashSoundInstance.start();

        monsterSingleton.FlashWasUsed();
    }

    protected override void FlashIsRecharged()
    {
        //stop playing flash sound onComplete
        flashSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        //play sound to signal that flash is reloaded
        reloadFlashSoundInstance.start();
        
        base.FlashIsRecharged();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        flashSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        reloadFlashSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        flashSoundInstance.release();
        reloadFlashSoundInstance.release();
    }
}