using System;
using DG.Tweening;
using UnityEngine;

public class DynamicControlsTutorial : MonoBehaviour
{
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float timeBetweenControlTexts = 10f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease fadeEase;
    [SerializeField] private ControlText[] controlTexts;

    private void Start() => HideAllControlsTexts();

    private void HideAllControlsTexts()
    {
        foreach (var controlText in controlTexts) 
            controlText.CanvasGroup.alpha = 0f;
    }

    public void StartShowingControlTextsSequence()
    {
        var sequence = DOTween.Sequence();
        
        sequence.AppendInterval(initialDelay);
        
        foreach (var controlsText in controlTexts)
        {
            sequence.Append(CreateShowControlTextTween(1f, controlsText));
            sequence.AppendInterval(timeBetweenControlTexts);
            sequence.Append(CreateShowControlTextTween(0f, controlsText));
        }
        
        sequence.Play();
    }

    private Tween CreateShowControlTextTween(float fadeTarget, ControlText controlText)
    {
        return controlText.CanvasGroup.DOFade(fadeTarget, fadeDuration)
            .SetEase(fadeEase)
            .OnStart(controlText.UpdateControlsTextBasedOnUsedControls);
    }
}
