using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;
using FMOD.Studio;
using FMODUnity;

public class RushAttack : AbstractAttackNode
{
    [Header("Rush Implementation")]
    [SerializeField] private Transform[] rushWindupPath;
    [SerializeField] private Transform rushTarget;
    [SerializeField] private Transform underWaterPosition;
    
    [Header("Rush Sound")]
    [SerializeField] private EventReference BiteRushSound;
    [SerializeField] private EventReference InitBiteRushSound;
    [SerializeField] private GameObject SoundOrigin;
    private EventInstance BiteRushSoundInstance;
    private EventInstance InitBiteRushSoundInstance;
    
    #region Animation Variables
    [Header("Animation")]
    [Header("Rush Windup")]
    [SerializeField] private string rushStartTrigger;
    [SerializeField] private float delayForRushAnimation;
    [SerializeField] private float rushWindupDuration = 3f;
    [SerializeField] private Ease rushWindupEase = Ease.InOutSine;

    [Header("Rush Charge")]
    [SerializeField] private float rushDuration = 2f;
    [SerializeField] private Ease rushEase = Ease.InSine;

    [Header("Rush Underwater")]
    [SerializeField] private float underWaterTime = 2f;

    [Header("Rush Surface")]
    [SerializeField] private float surfaceDuration = 1.5f;
    [SerializeField] private Ease surfaceEase = Ease.InOutSine;

    [Header("Rush Interrupted")]
    [SerializeField] private float delayForRushEndAnimation;
    [SerializeField] private float diveUnderDuration = 1f;
    [SerializeField] private Ease diveUnderEase = Ease.InOutSine;
    #endregion

    private Vector3 positionBeforeRush;
    private Vector3[] windupPositions;
    
    private Transform monsterPivot;
    private Sequence rushSequence;
    private Sequence rushToPlayerSequence;

    public override MonsterAttackType AttackType => MonsterAttackType.ShortRange;

    protected override void Start()
    {
        base.Start();
        
        InitBiteRushSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(InitBiteRushSound, SoundOrigin);
        BiteRushSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(BiteRushSound, SoundOrigin);
        
        monsterPivot = FightMonsterSingleton.instance.MonsterTransform;

        boatMovingController = FindFirstObjectByType<DriveStation_Moving>();
        
        CreateWindupPathPositionsList();
        SetupPositionConstraintForRushTarget();
    }

    private void CreateWindupPathPositionsList()
    {
        windupPositions = new Vector3[rushWindupPath.Length + 1];

        for (var i = 1; i < windupPositions.Length; i++) 
            windupPositions[i] = rushWindupPath[i - 1].position;
    }

    private void SetupPositionConstraintForRushTarget()
    {
        var playerTransform = PlayerSingleton.instance.PlayerTransform;
        
        var constrainSource = new ConstraintSource{ sourceTransform = playerTransform, weight = 1};

        var rushTargetConstraint = rushTarget.GetComponent<PositionConstraint>();

        rushTargetConstraint.AddSource(constrainSource);
        
        var offsetToPlayer = rushTarget.position - playerTransform.position;
        rushTargetConstraint.translationOffset = offsetToPlayer;

        rushTargetConstraint.locked = true;
        rushTargetConstraint.constraintActive = true;
    }

    protected override IEnumerator BehaviourRoutineImpl()
    {
        positionBeforeRush = monsterPivot.position;
        windupPositions[0] = positionBeforeRush;
        
        //Debug.Log("Rush Sequence1");
        InitBiteRushSoundInstance.start(); //immediate start of the sound
        
        rushSequence?.Kill();
        
        rushSequence = DOTween.Sequence();

        rushSequence.Append(CreateWindupPathTween());
        rushSequence.Join(DOVirtual.DelayedCall(delayForRushAnimation, () => MonsterAnimationController.SetTrigger(rushStartTrigger)));

        rushSequence.Append(DOVirtual.DelayedCall(0.01f, CreateRushToPlayerSequence));
        
        //start sound before actual rush
        rushSequence.AppendCallback(() => { BiteRushSoundInstance.start(); });
        
        rushSequence.AppendInterval(rushDuration);
        
        //stop sound after Monster is behind you
        //rushSequence.AppendCallback(() => { BiteRushSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); });
        
        rushSequence.AppendInterval(underWaterTime);
        
        rushSequence.Append(CreateSurfaceToOriginalPositionTween());
        rushSequence.Join(DOVirtual.DelayedCall(delayForRushEndAnimation, StartIdleAnimation));

        rushSequence.AppendInterval(1.5f);

        yield return rushSequence.WaitForCompletion();
    }

    private Tween CreateWindupPathTween()
    {
        return monsterPivot.DOPath(windupPositions, rushWindupDuration, PathType.CatmullRom)
            .SetEase(rushWindupEase)
            .SetOptions(false, AxisConstraint.None,AxisConstraint.X | AxisConstraint.Z)
            .SetLookAt(0.01f);
    }

    private void CreateRushToPlayerSequence()
    {
        rushToPlayerSequence?.Kill();
        
        rushToPlayerSequence = DOTween.Sequence();
        
        rushToPlayerSequence.Append(CreateRushTween());
        rushToPlayerSequence.Join(CreateLookAtTargetTween());

        rushToPlayerSequence.OnComplete(SetMonsterToUnderwaterPosition);
    }

    private Tween CreateRushTween()
    {
        return monsterPivot.DOMove(rushTarget.position, rushDuration)
            .SetEase(rushEase);
    }

    private Tween CreateLookAtTargetTween()
    {
        return monsterPivot.DOLookAt(rushTarget.position, 1f)
            .SetEase(rushEase);
    }

    private void SetMonsterToUnderwaterPosition()
    {
        monsterPivot.position = underWaterPosition.position;
    }

    private Tween CreateSurfaceToOriginalPositionTween()
    {
        return monsterPivot.DOMove(positionBeforeRush, surfaceDuration)
            .SetEase(surfaceEase);
    }

    protected override IEnumerator StopBehaviourRoutineImpl()
    {
        rushSequence?.Kill();
        rushToPlayerSequence?.Kill();
        
        //stop sound when interrupted
        //BiteRushSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        
        rushSequence = DOTween.Sequence();
        
        rushSequence.Append(CreateInterruptingDiveUnderTween()
            .OnComplete(SetMonsterToUnderwaterPosition));

        rushSequence.Append(CreateSurfaceToOriginalPositionTween());
        rushSequence.Join(DOVirtual.DelayedCall(delayForRushEndAnimation, StartIdleAnimation));

        yield return rushSequence.WaitForCompletion();
    }

    private Tween CreateInterruptingDiveUnderTween()
    {
        var currentPos = monsterPivot.position;
        var monsterForward = monsterPivot.forward;

        var diagonalForwardDownPosition = currentPos + monsterForward * 5f;
        diagonalForwardDownPosition.y = underWaterPosition.position.y;
        
        return monsterPivot.DOMove(diagonalForwardDownPosition,diveUnderDuration)
            .SetEase(diveUnderEase);
    }

    protected override void ForceStopBehaviourImpl()
    {
        rushSequence?.Kill();
        rushToPlayerSequence?.Kill();

        MonsterAnimationController.UnsetTrigger(rushStartTrigger);
        
        StopSounds();
    }

    private void StopSounds()
    {
        BiteRushSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        BiteRushSoundInstance.release();
        InitBiteRushSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        InitBiteRushSoundInstance.release();
    }

    #region Executability Calculations

    private DriveStation_Moving boatMovingController;
    public override float GetExecutability()
    {
        var isBoatStationary = boatMovingController.PlayerIsStationary;
        
        return executability.GetRandomBetweenLimits() * (isBoatStationary ? 1f : 0f);
    }

    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        //Destroy sound after Reset
        StopSounds();
    }
}