using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using DG.Tweening;

public class FightFlash : SpotFlash
{
    [SerializeField] private GameObject Lamp;
    [SerializeField] private Color LampOffEmissionColor;
    [SerializeField] private float LampEmissionStrength = 8f;
    [SerializeField] private Ease CooldownEase;
    private Material lampMat;
    private float lampCooldown;
    
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
        
        lampCooldown = base.GetCoolDownTimer();
        //get Lamp Material
        lampMat = Lamp.GetComponent<Renderer>().materials[0];
        lampMat.SetColor("_EmissionColor", LampOffEmissionColor*LampEmissionStrength);
    }

    protected override void FlashActivatedImpl()
    {
        //start playing flash sound
        flashSoundInstance.start();

        monsterSingleton.FlashWasUsed();
        
        //turn on lamp cooldown glow
        lampMat.EnableKeyword("_EMISSION");
        //start Tween to reduce Emission strength
        lampMat.DOColor(LampOffEmissionColor, "_EmissionColor", lampCooldown).SetEase(CooldownEase); //Set Emission to 0 over time
    }

    protected override void FlashIsRecharged()
    {
        //stop playing flash sound onComplete
        flashSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        //play sound to signal that flash is reloaded
        reloadFlashSoundInstance.start();
        
        //reset lamp cooldown glow 
        lampMat.DisableKeyword("_EMISSION");
        lampMat.SetColor("_EmissionColor", LampOffEmissionColor*LampEmissionStrength);
        
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