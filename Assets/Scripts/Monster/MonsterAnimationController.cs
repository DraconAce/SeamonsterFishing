using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimationController : MonoBehaviour
{
    [Serializable]
    private class AnimationTrigger
    {
        public string TriggerName;
        public int TriggerID;
    }
    
    [SerializeField] private List<string> animationTriggerNames;

    private Dictionary<string, int> animationTriggerIDLookup = new();

    public Animator MonsterAnimator { get; private set; }
    
    public event Action<string> AnimationFinishedEvent;

    public const float AnimationEndBuffer = 0.01f;

    private void Start()
    {
        MonsterAnimator = GetComponent<Animator>();
        
        CreateAnimationTriggerIDLookup();
        
        AttachAnimationFinishedEventToAllAnimationClips();
    }

    private void CreateAnimationTriggerIDLookup()
    {
        foreach (var triggerName in animationTriggerNames)
            animationTriggerIDLookup.Add(triggerName, Animator.StringToHash(triggerName));
    }
    
    private void AttachAnimationFinishedEventToAllAnimationClips()
    {
        var animationClips = MonsterAnimator.runtimeAnimatorController.animationClips;
        
        foreach (var animationClip in animationClips)
        {
            AnimationEvent animationEvent = new()
            {
                functionName = "OnAnimationFinished",
                time = animationClip.length - AnimationEndBuffer,
                stringParameter = animationClip.name
            };
            
            animationClip.AddEvent(animationEvent);
        }
    }

    public void SetTrigger(int triggerID) => MonsterAnimator.SetTrigger(triggerID);
    public void SetTrigger(string triggerName)
    {
        if (!animationTriggerIDLookup.TryGetValue(triggerName, out var triggerID))
        {
            var newTriggerID = Animator.StringToHash(triggerName);
            animationTriggerIDLookup.Add(triggerName, newTriggerID);
            
            MonsterAnimator.SetTrigger(newTriggerID);
            
            return;
        }
        
        MonsterAnimator.SetTrigger(triggerID);
    }

    public void OnAnimationFinished(string animationName) => AnimationFinishedEvent?.Invoke(animationName);
}