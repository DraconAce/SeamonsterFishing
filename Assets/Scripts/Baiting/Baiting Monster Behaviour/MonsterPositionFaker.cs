using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MonsterAttackManager))]

public class MonsterPositionFaker : MonoBehaviour
{
    [SerializeField] private Transform monsterProxy;
    public Transform MonsterProxy => monsterProxy;

    [SerializeField] private MinMaxLimit numberOfPointsLimits;
    [SerializeField] private MinMaxLimit distanceStepLimits;
    [SerializeField] private MinMaxLimit rotationAroundSpawnerLimits;

    [Header("Animation")] 
    [SerializeField] private float waitAtWaypointChance = 0.15f;
    [SerializeField] private MinMaxLimit waitLimits;
    [SerializeField] private MinMaxLimit durationLimits;
    
    private float minDistanceToPlayer;
    private float currentDistance;

    private Vector3 finalPositionMonster;
    private Vector3 spawnerAsPivot;

    private Transform spawnerTrans;
    private MonsterAttackManager attackManager;
    private MonsterSoundPlayer soundPlayer;
    private DifficultyProgressionManager difficultyManager;
    private PauseManager pauseManager;
    
    private WaitForSeconds waitSecond = new (1f);
    private Coroutine fakePosRoutine;
    private Tween movementTween;

    private readonly List<Vector3> fakePositionsList = new();


    private void Awake()
    {
        TryGetComponent(out attackManager);
        TryGetComponent(out soundPlayer);
    }

    private void Start()
    {
        difficultyManager = DifficultyProgressionManager.instance;
        pauseManager = PauseManager.instance;
        
        pauseManager.GamePausedStateChangedEvent += OnGamePaused;
        
        AssignSpawnerIfNotAssigned();
    }

    private void AssignSpawnerIfNotAssigned()
    {
        if (spawnerTrans != null) return;
        
        spawnerTrans = BaitingMonsterSingleton.instance.Spawner.transform;
        spawnerAsPivot = spawnerTrans.position;
    }

    private void OnGamePaused(bool isPaused)
    {
        if(isPaused) movementTween?.Pause();
        else movementTween?.Play();
    }

    public void StartPositionVariationRoutine() => StartCoroutine(DeferredPositionVariation());

    private IEnumerator DeferredPositionVariation()
    {
        yield return null;
        
        AssignSpawnerIfNotAssigned();
        
        finalPositionMonster = transform.position;
        fakePositionsList.Clear();

        minDistanceToPlayer = (spawnerAsPivot - finalPositionMonster).magnitude;
        currentDistance = minDistanceToPlayer;

        GeneratePositions();

        monsterProxy.position = fakePositionsList[0];
        fakePositionsList.RemoveAt(0);

        yield return waitSecond;
        
        StartMoveProxyTween();
        
        soundPlayer.StartMonsterApproachSounds();
    }

    private void GeneratePositions()
    {
        fakePositionsList.Add(finalPositionMonster);
        
        var numberOfPointsToGenerate =
            numberOfPointsLimits.GetRandomBetweenLimits(1f, difficultyManager.DifficultyFraction);

        for (var i = 0; i < numberOfPointsToGenerate; i++)
        {
            var newPosition = GenerateRandomPositionWithinLimits();

            fakePositionsList.Add(newPosition);
        }
        
        fakePositionsList.Reverse(); //because we started from the end position
    }

    private Vector3 GenerateRandomPositionWithinLimits()
    {
        var distanceStep = distanceStepLimits.GetRandomBetweenLimits();

        currentDistance += distanceStep;
        
        var newPosition = spawnerTrans.position + spawnerTrans.forward * currentDistance;

        var newRotation = new Vector3(0, rotationAroundSpawnerLimits.GetRandomBetweenLimits(), 0);
        newPosition = TransformationHelper.RotateAroundPivot(spawnerAsPivot, newPosition, newRotation);

        return newPosition;
    }

    private void StartMoveProxyTween()
    {
        var duration = durationLimits.GetRandomBetweenLimits(1f, difficultyManager.DifficultyFraction);

        movementTween = monsterProxy.DOPath(fakePositionsList.ToArray(), duration)
            .OnWaypointChange((wayPointIndex) =>
            {
                if (wayPointIndex < fakePositionsList.Count - 1
                    && Random.Range(0f, 1f) > waitAtWaypointChance) return;

                var waitDuration = waitLimits.GetRandomBetweenLimits(1f, difficultyManager.DifficultyFraction);
                DOVirtual.DelayedCall(waitDuration, () => movementTween.TogglePause(), false);

                movementTween.TogglePause();
            })
            .OnComplete(() =>
            {
                soundPlayer.StopMonsterApproachSounds();
                attackManager.StartAttack();
            });
    }

    private void OnDestroy()
    {
        if(pauseManager != null) 
            pauseManager.GamePausedStateChangedEvent -= OnGamePaused;
        
        if (fakePosRoutine == null) return;
        
        StopCoroutine(fakePosRoutine);
    }
}