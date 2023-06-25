using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MonsterSpawner_PositionGenerator))]

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MinMaxLimit timeBetweenSpawningChecks = new (){MinLimit = 3.0f, MaxLimit = 10.0f};
    [SerializeField] private GameObject[] monsterPrefabs;

    private int currentNumberMonsters;

    private MonsterSpawner_PositionGenerator positionGenerator;
    private WaitForSeconds waitBetweenSpawning;
    private Coroutine spawningRoutine;
    private GameStateManager gameStateManager;
    
    private readonly Dictionary<int, PrefabPool> monsterTypePools = new();

    public int MaxNumberMonsters { get; set; } = 1;
    public Transform SpawnCenter => positionGenerator.SpawnCircleCenter;

    private void Start()
    {
        gameObject.isStatic = true;
        SetupPrefabPoolsForEachMonsterPrefab();

        gameStateManager = GameStateManager.instance;
        spawningRoutine = StartCoroutine(MonsterSpawnRoutine());

        TryGetComponent(out positionGenerator);
    }

    private void SetupPrefabPoolsForEachMonsterPrefab()
    {
        for(var i = 0; i < monsterPrefabs.Length; i++) 
        {
            var poolObject = CreatePoolGameObject();

            var monsterPrefab = monsterPrefabs[i];

            var monsterPool =
                PrefabPoolFactory.instance.RequestNewPool(poolObject, monsterPrefab, poolObject.transform);
            
            monsterTypePools.Add(i, monsterPool);
        }
    }

    private GameObject CreatePoolGameObject()
    {
        return new GameObject
        {
            isStatic = true,
            transform =
            {
                parent = transform
            }
        };
    }

    private IEnumerator MonsterSpawnRoutine()
    {
        var loop = true;

        CreateWaitForNextSpawning();

        while (loop)
        {
            if (gameStateManager.CurrentGameState != GameState.Baiting) break;

            yield return waitBetweenSpawning;

            CreateWaitForNextSpawning();
            
            if (MonsterLimitIsReached()) continue;
            
            SpawnMonster();
        }
    }

    private void CreateWaitForNextSpawning()
    {
        waitBetweenSpawning = new(Random.Range(timeBetweenSpawningChecks.MinLimit, timeBetweenSpawningChecks.MaxLimit));
    }

    private bool MonsterLimitIsReached() => currentNumberMonsters >= MaxNumberMonsters;

    private void SpawnMonster()
    {
        var monsterPool = monsterTypePools[GetRandomMonsterPoolIndex()];

        monsterPool.RequestInstance(positionGenerator.GenerateMonsterSpawnPosition(), monsterPool.transform);

        currentNumberMonsters++;
    }

    private int GetRandomMonsterPoolIndex() => Random.Range(0, monsterTypePools.Count);

    public void MonsterDespawned()
    {
        currentNumberMonsters--;

        if (currentNumberMonsters < 0) currentNumberMonsters = 0;
    }

    private void OnDestroy()
    {
        if (spawningRoutine == null) return;
        
        StopCoroutine(spawningRoutine);
    }
}