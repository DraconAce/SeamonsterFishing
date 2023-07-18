using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashLightChargeManager : MonoBehaviour, IFloatInfoProvider, IInputEventSubscriber
{
    [SerializeField] private float completelyDrainedPercentage = 0.2f;
    [SerializeField] private MinMaxLimit spotIntensityLimits = new MinMaxLimit(0, 10);
    [SerializeField] private float drainPerSecond = 0.01f;
    [SerializeField] private BaitingFlash flashlight;
    [SerializeField] private EventReference chargeSound;
    [SerializeField] private string[] actionsToSubscribeTo;
    
    private bool isCharging;
    private bool flashCompletelyDrained;
    
    private float currentChargePercentage = 1f;

    private float CurrentChargePercentage
    {
        get => currentChargePercentage;
        set
        {
            var formerValue = currentChargePercentage;
            currentChargePercentage = Mathf.Clamp01(value);

            if (Mathf.Approximately(formerValue, currentChargePercentage)) return;

            flashlight.SpotLight.intensity 
                = Mathf.Lerp(spotIntensityLimits.MinLimit, spotIntensityLimits.MaxLimit, currentChargePercentage);
            InfoChanged?.Invoke();
        }
    }

    private WaitUntil waitForChargeStop;
    private Coroutine drainRoutine;
    private EventInstance chargeSoundInstance;

    private InputManager inputManager;
    
    public event Action InfoChanged;

    public float Info
    {
        get => CurrentChargePercentage;
        set => CurrentChargePercentage = value;
    }

    private void Start()
    {
        waitForChargeStop = new WaitUntil(() => !isCharging);

        chargeSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(chargeSound, gameObject);
        
        flashlight.onFlashUsed += OnFlashUsed;

        drainRoutine = StartCoroutine(DrainBatteryRoutine());

        inputManager = InputManager.instance;
        inputManager.SubscribeToActions(this);
    }

    #region Draining
    private void OnFlashUsed() => CurrentChargePercentage = 0f;

    private IEnumerator DrainBatteryRoutine()
    {
        var loop = true;

        while (loop)
        {
            if (isCharging)
                yield return waitForChargeStop;

            CurrentChargePercentage -= drainPerSecond * Time.deltaTime;

            if (CurrentChargePercentage <= completelyDrainedPercentage && !flashCompletelyDrained)
            {
                flashlight.ToggleFlashReady(false);
                flashCompletelyDrained = true;
            }

            yield return null;
        }
    }
    #endregion
    
    #region Charging

    [SerializeField] private float maxTimeBetweenChargeInput = 1f;
    [SerializeField] private float chargePerInput = 0.05f;

    private bool inputReceivedFlag;
    private float timeBetweenInputs;
    private Coroutine chargingRoutine;
    private readonly WaitForEndOfFrame waitForEndFrame = new();

    public bool SubscribedToPerformed => true;
    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;

    public void InputPerformed(InputAction.CallbackContext callContext) => ChargeInputReceived();

    private void ChargeInputReceived()
    {
        isCharging = true;
        inputReceivedFlag = true;
        
        timeBetweenInputs = 0;
        
        if (chargingRoutine != null) return;
            
        chargingRoutine = StartCoroutine(ChargingRoutine());
    }

    private IEnumerator ChargingRoutine()
    {
        chargeSoundInstance.start();
        
        while (isCharging)
        {
            yield return waitForEndFrame;

            if(inputReceivedFlag)
            {
                CurrentChargePercentage += chargePerInput;
                
                if(CurrentChargePercentage >= 1f)
                {
                    flashlight.ToggleFlashReady(true);
                    flashCompletelyDrained = false;
                }
            }
            else
            {
                timeBetweenInputs += Time.deltaTime;
                
                if (timeBetweenInputs >= maxTimeBetweenChargeInput)
                {
                    isCharging = false;

                    break;
                }
            }

            inputReceivedFlag = false;
        }

        chargeSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        chargingRoutine = null;
    }

    public void UnsubscribeOnDestroy()
    {
        if (inputManager == null) return;
        inputManager.UnsubscribeFromActions(this);
    }

    #endregion

    private void OnDestroy()
    {
        chargeSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        chargeSoundInstance.release();
        
        flashlight.onFlashUsed -= OnFlashUsed;
        
        UnsubscribeOnDestroy();

        if (drainRoutine == null) return;
        
        StopCoroutine(drainRoutine);
    }
}