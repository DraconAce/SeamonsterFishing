using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class FightFlash : SpotFlash
{
    [SerializeField] private GameObject Lamp;
    [SerializeField] private Color LampOffEmissionColor;
    [SerializeField] private float LampEmissionStrength = 8f;
    private Material lampMat;
    
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
        
        //get Lamp Material
        lampMat = Lamp.GetComponent<Renderer>().material;
        lampMat.SetColor("_EmissionColor", LampOffEmissionColor*LampEmissionStrength);
    }

    protected override void FlashActivatedImpl()
    {
        //start playing flash sound
        flashSoundInstance.start();

        monsterSingleton.FlashWasUsed();
        
        //turn on lamp cooldown glow
        lampMat.EnableKeyword("_EMISSION");
    }

    protected override void FlashIsRecharged()
    {
        //stop playing flash sound onComplete
        flashSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        //play sound to signal that flash is reloaded
        reloadFlashSoundInstance.start();
        
        //turn off lamp cooldown glow
        lampMat.DisableKeyword("_EMISSION");
        
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