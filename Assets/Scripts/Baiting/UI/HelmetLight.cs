using DG.Tweening;
using UnityEngine;

public class HelmetLight : MonoBehaviour
{
    [SerializeField] private bool playWarningSound;
    [SerializeField] private SoundEventRep warningSound;
    [SerializeField] private MeshRenderer lightRenderer;
    
    [SerializeField] private Material lightOnMaterial;
    [SerializeField] private Material lightOffMaterial;
    
    [SerializeField] private Light light;
    
    [SerializeField] private float onIntensity = 1f;
    [SerializeField] private float offIntensity = 0.0001f;
    
    [SerializeField] private float blinkDuration = 0.5f;
    [SerializeField] private Ease blinkEase = Ease.InOutSine;
    
    private Tween blinkingTween;
    private BaitingMonsterSingleton monsterSingleton;

    private void Start()
    {
        monsterSingleton = BaitingMonsterSingleton.instance;
        
        monsterSingleton.MonsterFinishedLurkingApproachEvent += OnMonsterStartedLurking;
        monsterSingleton.TryRepelMonsterEvent += OnTryRepelMonsterRepelled;
        
        if(playWarningSound) warningSound.CreateInstanceForSound(PlayerSingleton.instance.PlayerTransform.gameObject);
        
        ToggleLight(false);
    }

    private void OnMonsterStartedLurking() => TurnOnLightAndStartBlinking();

    private void TurnOnLightAndStartBlinking()
    {
        ToggleLight(true, false);
        StartBlinkingTween();
    }

    private void ToggleLight(bool activate, bool setLightIntensity = true)
    {
        lightRenderer.sharedMaterial = activate ? lightOnMaterial : lightOffMaterial;
        
        blinkingTween?.Kill();

        if (!setLightIntensity) return;
        light.intensity = activate ? onIntensity : offIntensity;
    }

    private void StartBlinkingTween()
    {
        blinkingTween = light.DOIntensity(onIntensity, blinkDuration)
            .SetEase(blinkEase)
            .OnStart(PlayWarningSound)
            .OnComplete(PlayWarningSound)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void PlayWarningSound()
    {
        if(!playWarningSound) return;
        warningSound.StartInstance();
    }

    private void OnTryRepelMonsterRepelled() => ToggleLight(false);

    private void OnDestroy()
    {
        warningSound.StopAndReleaseInstance();
        
        if(monsterSingleton == null) return;
        
        monsterSingleton.MonsterFinishedLurkingApproachEvent -= OnMonsterStartedLurking;
        monsterSingleton.TryRepelMonsterEvent -= OnTryRepelMonsterRepelled;
    }
}