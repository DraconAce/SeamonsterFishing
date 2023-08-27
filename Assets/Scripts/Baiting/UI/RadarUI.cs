using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarUI : MonoBehaviour
{
    private struct MonsterRadarInfo
    {
        public Vector3 MonsterPosition;

        public Vector2 NormalizedVectorToMonster;
        public float RelativeDistanceToFarthestDistance;
        
        public Vector2 RadarPosition;
        
        public void DetermineRelativeDistanceAndDirection(Vector3 playerPos, float farDistance)
        {
            playerPos.y = 0;
            
            var groundedMonsterPos = MonsterPosition;
            groundedMonsterPos.y = 0;
            
            var vectorPlayerToMonster = groundedMonsterPos - playerPos;

            var monsterDistanceToPlayer = vectorPlayerToMonster.magnitude;
            
            var normalizedPlayerMonsterVector = vectorPlayerToMonster.normalized;

            NormalizedVectorToMonster = new(normalizedPlayerMonsterVector.x, normalizedPlayerMonsterVector.z);
            NormalizedVectorToMonster = NormalizedVectorToMonster.normalized;

            RelativeDistanceToFarthestDistance = monsterDistanceToPlayer / farDistance;
        }
        
        public void DetermineRadarPosition(float radarRadius)
        {
            var radarPos = NormalizedVectorToMonster * (radarRadius * RelativeDistanceToFarthestDistance);
            
            RadarPosition = new(radarPos.x, radarPos.y);
        }
    }

    [SerializeField] private float radarCheckInterval;
    [SerializeField] private RectTransform radarDotParent;
    [SerializeField] private GameObject radarDotPrefab;
    [SerializeField] private SoundEventRep radarSound;

    private float farDistance;
    private float radarRadius;
    
    private Transform playerTrans;
    private MonsterSpawner monsterSpawner;
    private readonly List<MonsterRadarInfo> activeMonsterPositions = new();
    
    private WaitForSeconds waitBetweenRadarChecks;
    private Coroutine radarCoroutine;
    
    private PrefabPool radarDotPool;

    private void Start()
    {
        radarSound.CreateInstanceForSound(PlayerSingleton.instance.PlayerTransform.gameObject);
        
        var monsterSingleton = BaitingMonsterSingleton.instance;

        monsterSpawner = monsterSingleton.Spawner;

        playerTrans = PlayerSingleton.instance.PlayerTransform;

        SetRadarVariables(monsterSingleton);

        waitBetweenRadarChecks = new WaitForSeconds(radarCheckInterval);
        
        radarCoroutine = StartCoroutine(StartRadarRoutine());
    }

    private void SetRadarVariables(BaitingMonsterSingleton monsterSingleton)
    {
        DetermineFarDistance(monsterSingleton);
        DetermineRadarRadius();

        radarDotPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, radarDotPrefab, radarDotParent);
    }

    private void DetermineFarDistance(BaitingMonsterSingleton monsterSingleton)
    {
        farDistance = monsterSingleton.MonsterRangesDict[MonsterRange.Far];
    }

    private void DetermineRadarRadius()
    {
        var radarTransform = radarDotParent;
        var radarRect = radarTransform.rect;
        
        radarRadius = radarRect.width / 2 * radarTransform.localScale.x;
    }

    private IEnumerator StartRadarRoutine()
    {
        while (true)
        {
            yield return waitBetweenRadarChecks;
            
            UpdateActiveMonsterRadarInfoList();
            
            CreateRadarDots();
            
            if(monsterSpawner.ActiveMonsterProxyTransforms.Count > 0)
                radarSound.StartInstance();
        }
    }

    private void UpdateActiveMonsterRadarInfoList()
    {
        var activeMonsters = monsterSpawner.ActiveMonsterProxyTransforms;
        
        activeMonsterPositions.Clear();
        
        foreach (var activeMonster in activeMonsters)
        {
            var newMonsterRadarInfo = CreateMonsterRadarInfo(activeMonster);

            activeMonsterPositions.Add(newMonsterRadarInfo);
        }
    }

    private MonsterRadarInfo CreateMonsterRadarInfo(Transform activeMonster)
    {
        var newMonsterRadarInfo = new MonsterRadarInfo();

        var monsterPos = activeMonster.position;

        newMonsterRadarInfo.MonsterPosition = monsterPos;
        newMonsterRadarInfo.DetermineRelativeDistanceAndDirection(playerTrans.position, farDistance);

        newMonsterRadarInfo.DetermineRadarPosition(radarRadius);
        return newMonsterRadarInfo;
    }
    
    private void CreateRadarDots()
    {
        foreach (var monsterRadarInfo in activeMonsterPositions)
            radarDotPool.RequestInstance(monsterRadarInfo.RadarPosition);
    }

    private void OnDestroy()
    {
        radarSound.StopAndReleaseInstance();
        
        if (radarCoroutine == null) return;
        
        StopCoroutine(radarCoroutine);
    }
}