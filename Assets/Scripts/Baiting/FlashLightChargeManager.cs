using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashLightChargeManager : MonoBehaviour, IFloatInfoProvider, IInputEventSubscriber
{
    [SerializeField] private float drainPerSecond = 0.01f;
    [SerializeField] private BaitingFlash flashlight;
    [SerializeField] private string[] actionsToSubscribeTo;

    private bool isCharging;
    private bool flashCompletelyDrained;
    private float currentChargePercentage = 1f;

    private WaitForSeconds waitSec;
    private WaitUntil waitForChargeStop;
    private Coroutine drainRoutine;

    private InputManager inputManager;
    
    public event Action InfoChanged;

    public float Info
    {
        get => currentChargePercentage;
        set => currentChargePercentage = value;
    }

    private void Start()
    {
        waitSec = new(1f);
        waitForChargeStop = new WaitUntil(() => !isCharging);
        
        flashlight.onFlashUsed += OnFlashUsed;

        drainRoutine = StartCoroutine(DrainBatteryRoutine());

        inputManager = InputManager.instance;
        inputManager.SubscribeToActions(this);
    }

    #region Draining
    private void OnFlashUsed() => currentChargePercentage = 0f;

    private IEnumerator DrainBatteryRoutine()
    {
        var loop = true;

        while (loop)
        {
            if (isCharging)
                yield return waitForChargeStop;

            currentChargePercentage -= drainPerSecond * Time.deltaTime;
            InfoChanged?.Invoke();

            if (currentChargePercentage <= 0 && !flashCompletelyDrained)
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

        if (chargingRoutine != null)
            timeBetweenInputs = 0;
        else
            chargingRoutine = StartCoroutine(ChargingRoutine());
    }

    private IEnumerator ChargingRoutine()
    {
        while (isCharging)
        {
            yield return waitForEndFrame;

            if(inputReceivedFlag)
            {
                currentChargePercentage += chargePerInput;
                InfoChanged?.Invoke();
                
                if(currentChargePercentage >= 1f)
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
        flashlight.onFlashUsed -= OnFlashUsed;
        
        UnsubscribeOnDestroy();

        if (drainRoutine == null) return;
        
        StopCoroutine(drainRoutine);
    }
}