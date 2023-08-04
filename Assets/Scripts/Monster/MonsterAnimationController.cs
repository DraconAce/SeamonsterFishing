using System;
using UnityEngine;

public class MonsterAnimationController : MonoBehaviour
{
    public Animator MonsterAnimator { get; private set; }
    
    public event Action<string> AnimationFinishedEvent;

    public const float AnimationEndBuffer = 0.01f;

    private void Start()
    {
        MonsterAnimator = GetComponent<Animator>();
        
        AttachAnimationFinishedEventToAllAnimationClips();
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

    public void OnAnimationFinished(string animationName) => AnimationFinishedEvent?.Invoke(animationName);
}