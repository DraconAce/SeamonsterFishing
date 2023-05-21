using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpotFlash : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] private string[] actionsToSubscribeTo = { "ActivateSpot" };
    [SerializeField] private float coolDownTimer = 10;

    [Header("Flash On")] 
    [SerializeField] private float targetOnIntensity;
    [SerializeField] private float stayOnDuration = 0.75f;
    [SerializeField] private TweenSettings flashOnSettings;
    
    [Header("Flash Off")]
    [SerializeField] private float targetOffIntensity;
    [SerializeField] private TweenSettings flashOffSettings;
    

    private bool flashIsReady = true;
    private Sequence flashSequence;
    
    private Light spotLight;
    private InputManager inputManager;
    
    public string[] ActionsToSubscribeTo => actionsToSubscribeTo;
    
    private void Start()
    {
        inputManager = InputManager.instance;
        
        inputManager.SubscribeToActions(this);
        TryGetComponent(out spotLight);
    }

    public void InputPerformed(InputAction.CallbackContext callContext) => CheckIfFlashReadyAndActivate();

    private void CheckIfFlashReadyAndActivate()
    {
        if (!flashIsReady) return;
        
        flashSequence?.Kill();
        ActivateFlash();
    }

    private void ActivateFlash()
    {
        flashIsReady = false;
        flashSequence = DOTween.Sequence();

        flashSequence.Append(FlashTween(targetOnIntensity, flashOnSettings));
        flashSequence.AppendInterval(stayOnDuration);
        
        flashSequence.Append(FlashTween(targetOffIntensity, flashOffSettings));
        flashSequence.AppendInterval(coolDownTimer);

        flashSequence.OnComplete(() => flashIsReady = true);
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