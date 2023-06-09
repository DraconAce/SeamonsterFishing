using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;

public class SpotFlash : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string[] actionsToSubscribeTo = { "ActivateSpot" };
    [SerializeField] private float coolDownTimer = 10;
    [SerializeField] private UnityEvent onFlashReady;

    [Header("Flash On")] 
    [SerializeField] private float targetOnIntensity;
    [SerializeField] private float stayOnDuration = 0.75f;
    [SerializeField] private TweenSettings flashOnSettings;
    
    [Header("Flash Off")]
    [SerializeField] private float targetOffIntensity;
    [SerializeField] private TweenSettings flashOffSettings;
    

    public bool FlashIsReady { get; private set; }= true;
    private Sequence flashSequence;
    
    private Light spotLight;
    private InputManager inputManager;
    private FightMonsterSingleton monsterSingleton;

    public EventReference flashSound;

    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    private void Start()
    {
        inputManager = InputManager.instance;
        monsterSingleton = FightMonsterSingleton.instance;
        
        inputManager.SubscribeToActions(this);
        TryGetComponent(out spotLight);
    }

    public void InputPerformed(InputAction.CallbackContext callContext) => CheckIfFlashReadyAndActivate();

    private void CheckIfFlashReadyAndActivate()
    {
        if (!FlashIsReady) return;
        
        flashSequence?.Kill();
        
        monsterSingleton.FlashWasUsed();
        
        ActivateFlash();
    }

    private void ActivateFlash()
    {
        FlashIsReady = false;

        flashSequence = DOTween.Sequence();

        flashSequence.Append(FlashTween(targetOnIntensity, flashOnSettings));
        flashSequence.AppendInterval(stayOnDuration);
        
        flashSequence.Append(FlashTween(targetOffIntensity, flashOffSettings));
        flashSequence.AppendInterval(coolDownTimer);
        //create Instance of the flash sound for this activation
        FMOD.Studio.EventInstance flashSoundInstance = FMODUnity.RuntimeManager.CreateInstance(flashSound);
        //TODO create Instance of the sound to signal that flash is reloaded

        //start playing flash sound
        flashSoundInstance.start();

        flashSequence.OnComplete(() =>
        {
            //stop playing flash sound onComplete
            flashSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            flashSoundInstance.release();
            //TODO play sound to signal that flash is reloaded

            FlashIsReady = true;
            onFlashReady?.Invoke();
        });
    }

    private Tween FlashTween(float targetIntensity, TweenSettings targetSettings)
    {
        return spotLight.DOIntensity(targetIntensity, targetSettings.Duration)
            .SetEase(targetSettings.TweenEase)
            .OnStart(() => targetSettings.OnStartAction?.Invoke())
            .OnComplete(() => targetSettings.OnCompleteAction?.Invoke());
    }

    private void OnDestroy()
    {
        UnsubscribeOnDestroy();
        
        flashSequence?.Kill();
    }

    public void UnsubscribeOnDestroy()
    {
        if (inputManager == null) return;
        
        inputManager.UnsubscribeFromActions(this);
    }
}
