using System;
using UnityEngine;

[RequireComponent(typeof(ReelingStation_ReelInControl), typeof(ReelingStation_ProgressController),
    typeof(ReelingStation_Animation))]
public class ReelingStation : AbstractStation, IFloatInfoProvider
{
    [SerializeField] private float maxTimeToReel = 7;
    public float MaxTimeToReel => maxTimeToReel;
    [SerializeField] private float delayForSubStationsOnReelingCompleted = 1f;
    public float DelayForSubStationsOnReelingCompleted => delayForSubStationsOnReelingCompleted;

    public float Info
    {
        get => Progress;
        set => Progress = value;
    }
    
    public event Action OnReelingStartedEvent;
    public event Action OnReelingCompletedEvent;
    public event Action InfoChanged;

    private float progress;

    public float Progress 
    {
        get => progress;
        set
        {
            var formerProgress = progress;
            progress = value;
            
            if(Mathf.Approximately(formerProgress, progress)) return;
            
            InfoChanged?.Invoke();
        }
    }
    
    public float ReelingTimer { get; set; }
    
    public bool IsReelingTimeUp => ReelingTimer >= MaxTimeToReel;

    protected override void GameStateMatches()
    {
        base.GameStateMatches();
        
        OnReelingStartedEvent?.Invoke();
    }

    public void TriggerReelingCompletedEvent() => OnReelingCompletedEvent?.Invoke();
}