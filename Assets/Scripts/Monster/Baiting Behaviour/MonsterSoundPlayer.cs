using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField] private float maxWaitBetweenSounds;
    [SerializeField] private List<SoundGroupForDistance> monsterSoundsList;
    
    [SerializeField] private StudioEventEmitter approachSoundEmitter;
    [SerializeField] private StudioEventEmitter attackSoundEmitter;
    [SerializeField] private StudioEventEmitter repelledSoundEmitter;
    [SerializeField] private StudioEventEmitter killSoundEmitter;
    private Transform playerTrans;
    private Coroutine soundsRoutine;

    private void Start()
    {
        AssignPlayerTransformIfNotAssigned();

        monsterSoundsList.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        StartMonsterApproachSounds();
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
            var distanceToPlayer = (playerTrans.position - transform.position).magnitude;

            var soundGroupToPlay = GetSoundGroupOfDistance(distanceToPlayer);

            var randomSound = soundGroupToPlay.monsterSoundRefList[Random.Range(0, soundGroupToPlay.NumberOfSounds)];
            
            PlayMonsterSound(randomSound.eventRef);

            yield return new WaitForSeconds(Random.Range(randomSound.GetSoundLength(), maxWaitBetweenSounds));
        }
    }

    private SoundGroupForDistance GetSoundGroupOfDistance(float distanceToPlayer)
    {
        return monsterSoundsList.Aggregate(monsterSoundsList[0], (currentSoundRange, nextSoundRange) =>
            currentSoundRange.distance < distanceToPlayer && distanceToPlayer <= nextSoundRange.distance
                ? nextSoundRange
                : currentSoundRange);
    }

    private void PlayMonsterSound(EventReference eventRef)
    {
        approachSoundEmitter.EventReference = eventRef;
        
        approachSoundEmitter.Play();
    }

    public void StopMonsterApproachSounds()
    {
        if (soundsRoutine == null) return;
        
        StopCoroutine(soundsRoutine);
    }

    public void PlayAttackSound() => attackSoundEmitter.Play();

    public void PlayKillSound() => killSoundEmitter.Play();
    
    public void PlayRepelledSound() => repelledSoundEmitter.Play();
}