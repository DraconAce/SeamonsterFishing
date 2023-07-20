using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class HitWithArmsAttack : AbstractAttackNode
{
    [Header("Horizontal Rotation")]
    
    [SerializeField] private Transform leftArmYPivot;
    [SerializeField] private Transform rightArmYPivot;

    [SerializeField] private float horizontalRotationDuration = 1;
    [SerializeField] private MinMaxLimit rotationLimitYLeft;
    [SerializeField] private Ease horizontalEase = Ease.InSine;

    [Header("Vertical Rotation")]
    
    [SerializeField] private Transform leftArmZPivot;

    [SerializeField] private Transform rightArmZPivot;

    [FormerlySerializedAs("swingLimit")] [SerializeField] private MinMaxLimit swingLimitZ;

    [SerializeField] private int numberOfSwings = 5;
    
    [SerializeField] private float swingUpTime;
    [SerializeField] private float swingDownTime;
    
    [SerializeField] private Ease swingUpEase;
    [SerializeField] private Ease swingDownEase;

    public override MonsterAttackType AttackType => MonsterAttackType.MidRange;

    private Sequence attackSequence;
    
    public override float GetExecutability()
    { 
        return 100f;
    }

    
    protected override IEnumerator BehaviourRoutineImpl()
    {
        yield return StartCoroutine(SwingAttackRoutine());
    }

    private IEnumerator SwingAttackRoutine()
    {
        CreateArmAttackSequence();

        yield return attackSequence.WaitForCompletion();
        
        yield return ReturnToOriginalPositionTween();
    }

    private void CreateArmAttackSequence()
    {
        attackSequence = DOTween.Sequence();
        
        attackSequence.Append(RotateArmUp(leftArmZPivot));
        attackSequence.Join(RotateArmDown(rightArmZPivot));

        attackSequence.Join(RotateArmInwards(leftArmYPivot, false));
        attackSequence.Join(RotateArmInwards(rightArmYPivot, true));

        for(var i = 0; i < numberOfSwings; i++)
            AppendOneHitCycle();
    }

    private Tween RotateArmInwards(Transform armPivot, bool isRightArm) =>
        RotateArmHorizontally(armPivot, rotationLimitYLeft.MaxLimit, isRightArm);
    
    private Tween RotateArmOutwards(Transform armPivot, bool isRightArm) =>
        RotateArmHorizontally(armPivot, rotationLimitYLeft.MinLimit, isRightArm);

    private Tween RotateArmHorizontally(Transform armPivot, float rotationValue, bool mirrorRotation)
    {
        var targetRotation = new Vector3(0, rotationValue * (mirrorRotation ? -1 : 1), 0);

        return armPivot.DOLocalRotate(targetRotation, horizontalRotationDuration)
            .SetEase(horizontalEase);
    }

    private void AppendOneHitCycle()
    {
        attackSequence.Append(RotateArmUp(rightArmZPivot));
        attackSequence.Join(RotateArmDown(leftArmZPivot));
        
        attackSequence.Append(RotateArmUp(leftArmZPivot));
        attackSequence.Join(RotateArmDown(rightArmZPivot));
    }

    private Tween RotateArmUp(Transform armPivot) => RotateArmVertically(armPivot, swingLimitZ.MaxLimit, true);
    
    private Tween RotateArmDown(Transform armPivot) => RotateArmVertically(armPivot, swingLimitZ.MinLimit, false);

    private Tween RotateArmVertically(Transform armPivot, float rotationValue, bool swingUp)
    {
        var targetRotation = new Vector3(0,0,rotationValue);

        var swingDuration = swingUp ? swingUpTime : swingDownTime;
        var swingEase = swingUp ? swingUpEase : swingDownEase;

        return armPivot.DOLocalRotate(targetRotation, swingDuration)
            .SetEase(swingEase);
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        attackSequence?.Kill();
        
        yield return ReturnToOriginalPositionTween();
    }

    private IEnumerator ReturnToOriginalPositionTween()
    {
         var returnLeftRot = RotateArmDown(leftArmZPivot);
         var returnRightRot = RotateArmDown(rightArmZPivot);
        
        var returnOuterLeft = RotateArmOutwards(leftArmYPivot, false);
        var returnOuterRight= RotateArmOutwards(rightArmYPivot, true);

        yield return new WaitUntil(() =>
            TweenCompletedOrIsDead(returnLeftRot) && TweenCompletedOrIsDead(returnRightRot) 
            && TweenCompletedOrIsDead(returnOuterLeft) && TweenCompletedOrIsDead(returnOuterRight));
    }
    
    private bool TweenCompletedOrIsDead(Tween tween) => tween.IsComplete() || !tween.IsActive();
}