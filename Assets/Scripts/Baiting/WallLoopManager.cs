using System;
using UnityEngine;

public class WallLoopManager : LoopManager
{
    [Header("Spawning")]
    [SerializeField] private float spawnDistance = 10f;
    [SerializeField] private int numberOfLoopObsToSpawn = 5;
    
    [Header("Movement")]
    [SerializeField] private float moveDirection = -1;
    [SerializeField] private Axis movementAxis = Axis.YAXIS;

    private bool initialSpawnNumberIsEven;
    
    public float MoveDirection => moveDirection;
    public Axis MovementAxis => movementAxis;
    public float DespawnLocalTargetY { get; private set; }

    protected override void Start()
    {
        initialSpawnNumberIsEven = numberOfLoopObsToSpawn % 2 == 0;
        
        base.Start();
    }

    protected override void SpawnLoopElementImpl()
    {
        var spawnIndex = numberOfLoopObsToSpawn - (initialSpawnNumberIsEven ? 1 : 2);

        var spawnPos = CalculateLoopObPosition(spawnIndex);

        RequestLoopElementInstance(spawnPos);
    }

    protected override void SetupLoop()
    {
        DespawnLocalTargetY = spawnDistance * Mathf.Round(numberOfLoopObsToSpawn / 2f);
        
        SpawnInitialWall();
    }

    private void SpawnInitialWall()
    {
        for (var i = 0; i < numberOfLoopObsToSpawn; i++)
        {
            var spawnPos = CalculateLoopObPosition(i);
            
            RequestLoopElementInstance(spawnPos);
        }
    }

    private Vector3 CalculateLoopObPosition(int spawnIndex)
    {
        var neutralPos = transform.position;

        var spawnDirection = spawnIndex % 2 == 0 ? 1 : -1;
        var distanceToNeutralFactor = (float) Math.Round(spawnIndex / 2f, MidpointRounding.AwayFromZero);

        var spawnPos = neutralPos;
        spawnPos.y += distanceToNeutralFactor * spawnDirection * spawnDistance;

        return spawnPos;
    }
}