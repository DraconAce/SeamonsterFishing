using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FlashLightChargeManager : MonoBehaviour, IFloatInfoProvider, IInputEventSubscriber
{
    [SerializeField] private float completelyDrainedPercentage = 0.2f;
    [SerializeField] private float drainPerSecond = 0.01f;
    [SerializeField] private MinMaxLimit spotIntensityLimits = new (0, 10);
    [SerializeField] private BaitingFlash flashlight;
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
    private InputManager inputManager;
    private GameStateManager gameStateManager;
    
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
        emptySoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(emptySound, gameObject);
        fullyChargedSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(fullyChargedSound, gameObject);
        
        flashlight.onFlashUsed += OnFlashUsed;

        drainRoutine = StartCoroutine(DrainBatteryRoutine());

        inputManager = InputManager.instance;
        inputManager.SubscribeToActions(this);
        
        gameStateManager = GameStateManager.instance;
        gameStateManager.GameStateChangedEvent += OnPlayerDeath;
    }

    private void OnPlayerDeath(GameState newGameState)
    {
        if(newGameState != GameState.Dead) return;

        StopFlashlightSounds();
    }

    private void StopFlashlightSounds()
    {
        chargeSoundInstance.stop(STOP_MODE.IMMEDIATE);
        emptySoundInstance.stop(STOP_MODE.IMMEDIATE);
    }

    #region Draining
    [Header("Draining")]
    [SerializeField] private EventReference emptySound;

    private EventInstance emptySoundInstance;

    private void OnFlashUsed() => CurrentChargePercentage = 0f;

    private IEnumerator DrainBatteryRoutine()
    {
        var loop = true;

        while (loop)
        {
            if (isCharging)
                yield return waitForChargeStop;

            CurrentChargePercentage -= drainPerSecond * Time.deltaTime;

            if (IsFlashlightDrained())
                SetFlashlightToDrained();

            yield return null;
        }
    }

    private bool IsFlashlightDrained()
    {
        return CurrentChargePercentage <= completelyDrainedPercentage && !flashCompletelyDrained;
    }

    private void SetFlashlightToDrained()
    {
        flashlight.ToggleFlashReady(false);
        flashCompletelyDrained = true;
        emptySoundInstance.start();
    }

    #endregion
    
    #region Charging

    [Header("Charging")]
    [SerializeField] private float maxTimeBetweenChargeInput = 1f;
    [SerializeField] private float chargePerInput = 0.05f;
    [SerializeField] private EventReference chargeSound;
    [SerializeField] private EventReference fullyChargedSound;

    private bool inputReceivedFlag;
    private float timeBetweenInputs;
    
    private EventInstance chargeSoundInstance;
    private EventInstance fullyChargedSoundInstance;
    
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
                    
                    if(flashCompletelyDrained)
                        fullyChargedSoundInstance.start();

                    flashCompletelyDrained = false;
                    emptySoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
        emptySoundInstance.stop(STOP_MODE.IMMEDIATE);
        emptySoundInstance.release();
        
        chargeSoundInstance.stop(STOP_MODE.IMMEDIATE);
        chargeSoundInstance.release();
        
        flashlight.onFlashUsed -= OnFlashUsed;
        
        UnsubscribeOnDestroy();

        if (drainRoutine == null) return;
        
        StopCoroutine(drainRoutine);
    }
}