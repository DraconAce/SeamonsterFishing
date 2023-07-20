using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
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

            var eventDescription = eventRef.Guid == default ? default : RuntimeManager.GetEventDescription(eventRef);

            if (!eventDescription.isValid()) return 2f;

            eventDescription.getLength(out var timeInMillis);
            return timeInMillis / 1000f;
        }
    }
    
    [Serializable]
    private struct SoundGroupForDistance
    {
        public float distance;
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

    private float initialDistanceToPlayer;
    private Vector3 spawnLeft;

    private Transform spawnOrigin;
    private Transform playerTrans;
    private Transform fakeMonsterTrans;

    private Coroutine soundsRoutine;

    private DifficultyProgressionManager difficultyManager;

    private readonly WaitForSeconds waitSecond = new (1f);

    private readonly MinMaxLimit[] soundProhibitedAngles = new MinMaxLimit[2]
    {
        new (35f, 60f),
        new (120f, 155f)
    };

    private void Start()
    {
        difficultyManager = DifficultyProgressionManager.instance;
        spawnOrigin = BaitingMonsterSingleton.instance.Spawner.SpawnCenter;
        spawnLeft = spawnOrigin.right;

        fakeMonsterTrans = GetComponent<MonsterPositionFaker>().MonsterProxy;
        
        AssignPlayerTransformIfNotAssigned();

        monsterSoundsList.Sort((a, b) => a.distance.CompareTo(b.distance));
    }

    private void AssignPlayerTransformIfNotAssigned() => playerTrans = PlayerSingleton.instance.PlayerTransform;

    public void StartMonsterApproachSounds()
    {
        AssignPlayerTransformIfNotAssigned();
        
        initialDistanceToPlayer = (playerTrans.position - fakeMonsterTrans.position).magnitude;
        
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
            
            var distanceToPlayer = (playerTrans.position - fakeMonsterTrans.position).magnitude;

            var soundGroupToPlay = GetSoundGroupOfDistance(distanceToPlayer);

            var randomSound = soundGroupToPlay.monsterSoundRefList[Random.Range(0, soundGroupToPlay.NumberOfSounds)];
            
            PlayMonsterSound(randomSound.eventRef);

            var waitNextSoundTime = randomSound.GetSoundLength() + GenerateAdditionalWaitTimeForNextSound();
            
            yield return new WaitForSeconds(Random.Range(randomSound.GetSoundLength(), waitNextSoundTime));
        }
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
            if(distanceToPlayer <= currentSoundRange.distance) 
                return currentSoundRange;

            return nextSoundRange;
        });
    }

    private void PlayMonsterSound(EventReference eventRef)
    {
        approachSoundEmitter.OverrideAttenuation = true;
        
        approachSoundEmitter.OverrideMinDistance = 1f;
        approachSoundEmitter.OverrideMaxDistance = initialDistanceToPlayer - 5f;

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
}