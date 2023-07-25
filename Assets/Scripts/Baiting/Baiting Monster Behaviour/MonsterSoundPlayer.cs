using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MonsterPositionFaker))]
public class MonsterSoundPlayer : MonoBehaviour
{
    [Serializable]
    private struct SoundEventRep
    {
        public EventReference eventRef;
        
        private float cachedLength;

        private bool lengthWasCached;

        public float GetSoundLength()
        {
            if (lengthWasCached) return cachedLength;

            cachedLength = SoundHelper.GetSoundLength(eventRef);
            lengthWasCached = true;

            return cachedLength;
        }
    }
    
    [Serializable]
    private struct SoundGroupForDistance
    {
        public MonsterRange MonsterRange;
        public List<SoundEventRep> monsterSoundRefList;

        public int NumberOfSounds => monsterSoundRefList.Count;
    }

    [SerializeField] private MinMaxLimit waitBetweenSoundsLimit;
    [SerializeField] private List<SoundGroupForDistance> monsterSoundsList;

    [Header("Sound Emitters")]
    [SerializeField] private StudioEventEmitter approachSoundEmitter;

    [SerializeField] private StudioEventEmitter lurkSoundEmitter;
    [SerializeField] private StudioEventEmitter repelledSoundEmitter;
    [SerializeField] private StudioEventEmitter killSoundEmitter;

    private Vector3 spawnLeft;

    private Transform spawnOrigin;
    private Transform playerTrans;
    private Transform fakeMonsterTrans;

    private Coroutine soundsRoutine;

    private DifficultyProgressionManager difficultyManager;
    private PauseManager pauseManager;
    private BaitingMonsterSingleton monsterSingleton;

    private WaitUntil waitForGameUnpaused;
    private readonly WaitForSeconds waitSecond = new (1f);

    private readonly MinMaxLimit[] soundProhibitedAngles = new MinMaxLimit[2]
    {
        new (35f, 60f),
        new (120f, 155f)
    };

    private void Start()
    {
        difficultyManager = DifficultyProgressionManager.instance;
        pauseManager = PauseManager.instance;
        monsterSingleton = BaitingMonsterSingleton.instance;
        
        waitForGameUnpaused = new WaitUntil(() => !pauseManager.GameIsPaused);
        
        spawnOrigin = BaitingMonsterSingleton.instance.Spawner.SpawnCenter;
        spawnLeft = spawnOrigin.right;

        fakeMonsterTrans = GetComponent<MonsterPositionFaker>().MonsterProxy;
        
        AssignPlayerTransformIfNotAssigned();

        monsterSoundsList.Sort((a, b) => a.MonsterRange.CompareTo(b.MonsterRange));
    }

    private void AssignPlayerTransformIfNotAssigned() => playerTrans = PlayerSingleton.instance.PlayerTransform;

    public void StartMonsterApproachSounds()
    {
        AssignPlayerTransformIfNotAssigned();
        
        soundsRoutine = StartCoroutine(MonsterSoundsRoutine());
    }

    private IEnumerator MonsterSoundsRoutine()
    {
        var loop = true;

        while (loop)
        {
            if (IsMonsterInProhibitedAngle())
            {
                yield return waitSecond;
                continue;
            }

            if (pauseManager.GameIsPaused) 
                yield return waitForGameUnpaused;

            var distanceToPlayer = GetDistanceToPlayer();

            var soundGroupToPlay = GetSoundGroupOfDistance(distanceToPlayer);

            var randomSound = soundGroupToPlay.monsterSoundRefList[Random.Range(0, soundGroupToPlay.NumberOfSounds)];
            
            PlayMonsterSound(randomSound.eventRef);

            var waitNextSoundTime = randomSound.GetSoundLength() + GenerateAdditionalWaitTimeForNextSound();
            
            yield return new WaitForSeconds(Random.Range(randomSound.GetSoundLength(), waitNextSoundTime));
        }
    }

    private float GetDistanceToPlayer()
    {
        return (playerTrans.position - fakeMonsterTrans.position).magnitude;
    }

    private bool IsMonsterInProhibitedAngle()
    {
        var angleMonsterAndSpawnOrigin = Vector3.Angle(spawnLeft, transform.position - spawnOrigin.position);

        foreach (var prohibitedZone in soundProhibitedAngles)
            if (prohibitedZone.IsValueBetweenLimits(angleMonsterAndSpawnOrigin))
                return true;

        return false;
    }

    private SoundGroupForDistance GetSoundGroupOfDistance(float distanceToPlayer)
    {
        return monsterSoundsList.Aggregate(monsterSoundsList[0], (currentSoundRange, nextSoundRange) =>
        {
            var categoryDistance = monsterSingleton.MonsterRangesDict[currentSoundRange.MonsterRange];
            
            if(distanceToPlayer <= categoryDistance) 
                return currentSoundRange;

            return nextSoundRange;
        });
    }

    private void PlayMonsterSound(EventReference eventRef)
    {
        approachSoundEmitter.OverrideAttenuation = true;
        
        approachSoundEmitter.OverrideMinDistance = 1f;
        
        var currentDistanceToPlayer = GetDistanceToPlayer();
        approachSoundEmitter.OverrideMaxDistance = currentDistanceToPlayer + 10f;

        approachSoundEmitter.EventReference = eventRef;
        
        approachSoundEmitter.Play();
    }

    private float GenerateAdditionalWaitTimeForNextSound()
    {
        return Random.Range(waitBetweenSoundsLimit.MinLimit, 
            waitBetweenSoundsLimit.MaxLimit * difficultyManager.DifficultyFraction);
    }

    public void StopMonsterApproachSounds()
    {
        if (soundsRoutine == null) return;
        
        StopCoroutine(soundsRoutine);
    }

    public void PlayLurkSound() => lurkSoundEmitter.Play();

    public void PlayKillSound() => killSoundEmitter.Play();
    
    public void PlayRepelledSound() => repelledSoundEmitter.Play();
    
    public float GetLurkSoundLength() => SoundHelper.GetSoundLength(lurkSoundEmitter.EventReference);
}