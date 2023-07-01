using System;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;

public class SpotFlash : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string[] actionsToSubscribeTo = { "ActivateSpot" };
    [SerializeField] private float coolDownTimer = 10;
    [SerializeField] private bool autoChargeAfterUse = true;
    [SerializeField] private UnityEvent onFlashReady;

    [Header("Flash On")] 
    [SerializeField] private float targetOnIntensity;
    [SerializeField] private float stayOnDuration = 0.75f;
    [SerializeField] private TweenSettings flashOnSettings;
    
    [Header("Flash Off")]
    [SerializeField] private float targetOffIntensity;
    [SerializeField] private TweenSettings flashOffSettings;
    
    public bool FlashIsReady { get; protected set; }= true;
    private Sequence flashSequence;
    
    private Light spotLight;
    public Light SpotLight => spotLight;

    private InputManager inputManager;

    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    private void Awake() => TryGetComponent(out spotLight);

    protected virtual void Start()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToActions(this);
    }

    public void InputPerformed(InputAction.CallbackContext callContext) => CheckIfFlashReadyAndActivate();

    private void CheckIfFlashReadyAndActivate()
    {
        if (!FlashIsReady) return;
        
        flashSequence?.Kill();
        
        FlashActivatedImpl();
        
        ActivateFlash();
    }
    
    protected virtual void FlashActivatedImpl(){}

    private void ActivateFlash()
    {
        FlashIsReady = false;
        
        flashSequence = DOTween.Sequence();

        flashSequence.Append(FlashTween(targetOnIntensity, flashOnSettings));
        flashSequence.AppendInterval(stayOnDuration);

        flashSequence.Append(FlashTween(targetOffIntensity, flashOffSettings));

        if (!autoChargeAfterUse) return;

        flashSequence.AppendInterval(coolDownTimer);

        flashSequence.OnComplete(FlashIsRecharged);
    }

    protected virtual void FlashIsRecharged() => SetFlashToReady();

    protected void SetFlashToReady()
    {
        FlashIsReady = true;
        onFlashReady?.Invoke();
    }

    private Tween FlashTween(float targetIntensity, TweenSettings targetSettings)
    {
        return spotLight.DOIntensity(targetIntensity, targetSettings.Duration)
            .SetEase(targetSettings.TweenEase)
            .OnStart(() => targetSettings.OnStartAction?.Invoke())
            .OnComplete(() => targetSettings.OnCompleteAction?.Invoke());
    }

    protected virtual void OnDestroy()
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
