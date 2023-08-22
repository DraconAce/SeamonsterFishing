using System;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class MainSoundThemeManager : MonoBehaviour
{
    [SerializeField] private List<SoundEventRep> mainThemeParts;
    
    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private Ease fadeInEase = Ease.Linear;
    
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Ease fadeOutEase = Ease.Linear;

    private int themePartIndex;
    
    private FightMonsterSingleton monsterSingleton;
    private FightMonsterState fightMonsterState;
    
    private readonly List<Tween> fadeTweens = new(3);

    private void Start()
    {
        CreateInstanceForThemePartsAndSetVolume0();
        
        monsterSingleton = FightMonsterSingleton.instance;
        fightMonsterState = monsterSingleton.FightState;
        
        monsterSingleton.MonsterWeakpointWasHitEvent += OnWeakPointWasHit;
        fightMonsterState.MonsterStateChangedEvent += OnMonsterStateChanged;
        
        StartNextThemePart();
    }
    
    private void CreateInstanceForThemePartsAndSetVolume0()
    {
        foreach (var themePart in mainThemeParts)
        {
            themePart.CreateInstanceForSound(gameObject);
            themePart.SetVolume(0f);
        }
    }

    private void OnWeakPointWasHit()
    {
        if(themePartIndex >= mainThemeParts.Count) return;
        
        StartNextThemePart();
    }

    private void StartNextThemePart() => StartThemeFadeInTween(themePartIndex++);

    private void StartThemeFadeInTween(int themeIndex)
    {
        var themeToStart = mainThemeParts[themeIndex];
        
        themeToStart.StartInstance();
        
        var fadeTween = DOVirtual.Float(0f, 1f, fadeInTime, themeToStart.SetVolume)
            .SetEase(fadeInEase);
        
        fadeTweens.Add(fadeTween);
    }
    
    private void OnMonsterStateChanged(MonsterState newState)
    {
        if (newState != MonsterState.Dead) return;
        
        StartFadeOutAllParts();
    }

    private void StartFadeOutAllParts()
    {
        for(var i = 0; i < mainThemeParts.Count; i++)
        {
            var themePart = mainThemeParts[i];
            
            fadeTweens[i].Kill();
            
            fadeTweens[i] = StartThemeFadeOutTween(themePart);;
        }
    }
    
    private Tween StartThemeFadeOutTween(SoundEventRep themePart)
    {
        var fadeTween = DOVirtual.Float(1f, 0f, fadeOutTime, themePart.SetVolume)
            .SetEase(fadeOutEase);

        return fadeTween;
    }

    private void OnDestroy()
    {
        foreach (var themePart in mainThemeParts) 
            themePart.StopAndReleaseInstance(STOP_MODE.IMMEDIATE);
        
        if(fightMonsterState != null)
            fightMonsterState.MonsterStateChangedEvent -= OnMonsterStateChanged;

        if(monsterSingleton == null) return;
        
        monsterSingleton.MonsterWeakpointWasHitEvent -= OnWeakPointWasHit;
    }
}