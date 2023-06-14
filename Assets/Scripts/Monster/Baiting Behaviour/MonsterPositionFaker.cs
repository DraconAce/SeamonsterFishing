using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MonsterAttackManager))]

public class MonsterPositionFaker : MonoBehaviour
{
    [SerializeField] private Transform monsterProxy;

    [SerializeField] private MinMaxLimit numberOfPointsLimits;
    [SerializeField] private MinMaxLimit distanceStepLimits;
    [SerializeField] private MinMaxLimit rotationAroundSpawnerLimits;

    [Header("Animation")] 
    [SerializeField] private float waitAtWaypointChance = 0.15f;
    [SerializeField] private MinMaxLimit waitLimits;
    [SerializeField] private MinMaxLimit durationLimits;

    private MonsterAttackManager attackManager;
    private MonsterSoundPlayer soundPlayer;
    private Coroutine fakePosRoutine;
    private Tween movementTween;

    private float minDistanceToPlayer;
    private float currentDistance;

    private Vector3 finalPositionMonster;
    private Vector3 spawnerAsPivot;

    private Transform spawnerTrans;
    private readonly List<Vector3> fakePositionsList = new();


    private void Awake()
    {
        TryGetComponent(out attackManager);
        TryGetComponent(out soundPlayer);
    }

    private void Start() => AssignSpawnerIfNotAssigned();

    private void AssignSpawnerIfNotAssigned()
    {
        if (spawnerTrans != null) return;
        
        spawnerTrans = BaitingMonsterSingleton.instance.Spawner.transform;
        spawnerAsPivot = spawnerTrans.position;
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
        
        StartMoveProxyTween();
        
        soundPlayer.StartMonsterApproachSounds();
    }

    private void GeneratePositions()
    {
        fakePositionsList.Add(finalPositionMonster);
        
        //generate number of points to generate
        var numberOfPointsToGenerate = numberOfPointsLimits.GetRandomBetweenLimits();

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
        newPosition = RotateAroundPivot(spawnerAsPivot, newPosition, newRotation);
        return newPosition;
    }

    private Vector3 RotateAroundPivot(Vector3 pivotPoint, Vector3 pointToRotate, Vector3 angles) 
        => Quaternion.Euler(angles) * (pointToRotate - pivotPoint) + pivotPoint;

    private void StartMoveProxyTween()
    {
        var duration = durationLimits.GetRandomBetweenLimits();

        movementTween = monsterProxy.DOPath(fakePositionsList.ToArray(), duration, PathType.CatmullRom)
            .OnWaypointChange((wayPointIndex) =>
            {
                if (wayPointIndex < fakePositionsList.Count - 1
                    && Random.Range(0f, 1f) > waitAtWaypointChance) return;

                var waitDuration = waitLimits.GetRandomBetweenLimits();
                DOVirtual.DelayedCall(waitDuration, () => movementTween.TogglePause());

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
        if (fakePosRoutine == null) return;
        
        StopCoroutine(fakePosRoutine);
    }
}