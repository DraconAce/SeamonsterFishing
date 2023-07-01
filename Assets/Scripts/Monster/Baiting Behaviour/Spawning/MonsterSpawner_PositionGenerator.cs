using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner_PositionGenerator : MonoBehaviour
{
    [SerializeField] private float spawnCircleRadius = 3f;
    
    [SerializeField] private Transform spawnCircleCenter;
    public Transform SpawnCircleCenter => spawnCircleCenter;

    private Vector3 spawnCenter;
    private Vector3 spawnCircleStart;
    
    private MinMaxLimit[] spawnSectors;

    private readonly float[] spawnSectionLimits = {0, 35, 60, 90, 90, 120, 155, 180 };

    private void Awake()
    {
        spawnCenter = spawnCircleCenter.position;
        
        spawnCircleStart = spawnCenter + -spawnCircleCenter.right * spawnCircleRadius;

        SetupSpawnCircles();
    }

    private void SetupSpawnCircles()
    {
        spawnSectors = new MinMaxLimit[4];

        for (var sectionIndex = 0; sectionIndex < spawnSectionLimits.Length; sectionIndex += 2)
        {
            spawnSectors[sectionIndex / 2] = new MinMaxLimit(spawnSectionLimits[sectionIndex], 
                spawnSectionLimits[sectionIndex + 1]);
        }
    }

    public Vector3 GenerateMonsterSpawnPosition() => GetPointOnSpawnCircle();

    private Vector3 GetPointOnSpawnCircle()
    {
        var spawnSector = GetRandomSpawnSector();
        
        var pointOnCircle = GenerateSectionPoint(spawnSector);
        
        return pointOnCircle;
    }

    private MinMaxLimit GetRandomSpawnSector() => spawnSectors[Random.Range(0, spawnSectors.Length)];

    private Vector3 GenerateSectionPoint(MinMaxLimit spawnSector)
    {
        var rotationVector = Vector3.up * spawnSector.GetRandomBetweenLimits();
        
        return TransformationHelper.RotateAroundPivot(spawnCenter, spawnCircleStart,
            rotationVector);
    }
}