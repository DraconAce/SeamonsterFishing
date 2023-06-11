using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner_PositionGenerator : MonoBehaviour
{
    [SerializeField] private float spawnCircleRadius = 3f;
    [SerializeField] private Transform spawnCircleCenter;
    [SerializeField] private MinMaxLimit xLimit;
    [SerializeField] private MinMaxLimit zLimit;

    private Vector3 circlePos;

    private void Awake() => circlePos = spawnCircleCenter.position;

    public Vector3 GenerateMonsterSpawnPosition() => GetPointOnSpawnCircle();

    private Vector3 GetPointOnSpawnCircle()
    {
        var xCoord = xLimit.GetRandomBetweenLimits() + circlePos.x;
        var zCoord = zLimit.GetRandomBetweenLimits() + circlePos.z;
        
        var randomPointInLimits = new Vector3(xCoord, circlePos.y, zCoord);

        var randomPointDirection = (randomPointInLimits - circlePos).normalized;

        var pointOnCircle = circlePos + randomPointDirection * spawnCircleRadius;
        return pointOnCircle;
    }
}