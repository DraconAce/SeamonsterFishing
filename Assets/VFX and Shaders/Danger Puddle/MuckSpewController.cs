using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public class MuckSpewController : MonoBehaviour
{
    [SerializeField] private MuckAttack muckAttack;

    [FormerlySerializedAs("Muck_Spew_Gameobject")] [SerializeField] private GameObject muckSpewOb;
    [SerializeField] private GameObject muckExplosionPrefab;
     
    [Header("Sound")]
    [SerializeField] private EventReference MuckFireBreathSound;
    [SerializeField] private EventReference MuckSpewSound;
    private EventInstance muckFireBreathSoundInstance;
    private EventInstance muckSpewSoundInstance;
    
    [Header("Routine delays for animation sync")]
    [SerializeField] private float delayForMuckBuildUp = 1.5f;
    [SerializeField] private float delayForSpewReady = 3.5f;
    [SerializeField] private float delayForSpewEnd = 1f;
    [SerializeField] private float delayForFireSpew = 6f;
    [SerializeField] private float delayForMuckFire = 0.2f;
    [SerializeField] private float delayForMuckEnd = 1.5f;
    
    private WaitForSeconds waitForMuckBuildUp;
    private WaitForSeconds waitForSpewReady;
    private WaitForSeconds waitForSpewEnd;
    private WaitForSeconds waitForFireSpew;
    private WaitForSeconds waitForMuckOnFire;
    private WaitForSeconds waitForEndMuck;

    [Header("Animation Triggers")]
    [SerializeField] private string buildUpTrigger = "MuckBuildUp";
    [SerializeField] private string spewTrigger = "SpewMuck";
    [SerializeField] private string endSpewTrigger = "EndSpewMuck";

    private Vector3 cachedPlayerPos;
    
    private Transform playerTransform;
    private Transform spewTransform;

    private Material muckSpewMat;
    private ParticleSystem muckSpewParticles;

    private PrefabPool muckExplosionPool;
    private MuckGoo currentGoo;
    private MonsterAnimationController monsterAnimationController;

    private Coroutine muckRoutine;

    private void Start()
    {
        playerTransform = PlayerSingleton.instance.PhysicalPlayerRepresentation;
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;

        muckExplosionPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, muckExplosionPrefab);

        CreateMuckSoundInstances();

        SetupMuckSpewVariables();

        CreateYieldInstructionsForAnimationDelays();
    }

    private void CreateMuckSoundInstances()
    {
        muckFireBreathSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MuckFireBreathSound, muckSpewOb);
        muckSpewSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MuckSpewSound, muckSpewOb);
    }

    private void SetupMuckSpewVariables()
    {
        spewTransform = muckSpewOb.transform;
        muckSpewMat = muckSpewOb.GetComponent<Renderer>().material;
        muckSpewParticles = muckSpewOb.GetComponent<ParticleSystem>();
    }

    private void CreateYieldInstructionsForAnimationDelays()
    {
        waitForMuckBuildUp = new WaitForSeconds(delayForMuckBuildUp);
        waitForSpewReady = new WaitForSeconds(delayForSpewReady);
        waitForSpewEnd = new WaitForSeconds(delayForSpewEnd);
        waitForFireSpew = new WaitForSeconds(delayForFireSpew);
        waitForMuckOnFire = new WaitForSeconds(delayForMuckFire);
        waitForEndMuck = new WaitForSeconds(delayForMuckEnd);
    }

    public void StartMuckRoutine() => muckRoutine = StartCoroutine(MuckTimeTracker());

    private IEnumerator MuckTimeTracker()
    {
        monsterAnimationController.SetTrigger(buildUpTrigger); //build up
        
        yield return waitForMuckBuildUp;
        monsterAnimationController.SetTrigger(spewTrigger); //get ready for spew
        
        yield return waitForSpewReady;
        cachedPlayerPos = playerTransform.position;
        SpewAndPlaceNewGoo();

        yield return waitForSpewEnd;
        monsterAnimationController.SetTrigger(endSpewTrigger); //get ready for fire spew

        yield return waitForFireSpew;
        monsterAnimationController.SetTrigger(spewTrigger); //get ready for fire spew

        yield return waitForSpewReady;
        StartFireBreath();
        
        yield return waitForMuckOnFire;
        SetMuckOnFire();

        yield return waitForEndMuck;
        TriggerAttackEndAndEndAnimation();
    }

    private void SpewAndPlaceNewGoo()
    {
        StartSpewAndAdjustStrength();

        currentGoo = RequestNewMuckExplosionAndPlace(cachedPlayerPos);
    }

    private void StartSpewAndAdjustStrength()
    {
        AdjustMuckSpewStrength();

        muckSpewSoundInstance.start();
        muckSpewParticles.Play();
    }

    private void AdjustMuckSpewStrength()
    {
        var spewPos = spewTransform.position;
        
        //Adjust the Muck Spew initial Speed -> speed up the spew if the player is further away
        var distance = cachedPlayerPos - spewPos;
        
        //Debug.Log("Spew distance: " + distance);
        var bonusSpeed = distance[0] - distance[1] - distance[2]; //positive x, negative y and z = further away
        var main = muckSpewParticles.main;
        main.startSpeed = bonusSpeed;
    }

    private MuckGoo RequestNewMuckExplosionAndPlace(Vector3 muckPosition)
    {
        var newGoo = muckExplosionPool.RequestInstance(muckPosition);
        newGoo.TryGetCachedComponent<MuckGoo>(out var muckGoo);

        return muckGoo;
    }

    private void StartFireBreath()
    {
        StartSpewAndAdjustStrength();
        
        muckSpewMat.EnableKeyword("_EMISSION");
        muckSpewParticles.Play();
        muckFireBreathSoundInstance.start();
    }

    private void SetMuckOnFire()
    {
        if (currentGoo == null) return;
        
        currentGoo.StartMuckFire();
    }

    private void TriggerAttackEndAndEndAnimation()
    {
        muckAttack.TriggerMuckAttackEnd();
        monsterAnimationController.SetTrigger(endSpewTrigger);
    }

    public void StopMuckRoutine()
    {
        if (muckRoutine == null) return;
            
        StopCoroutine(muckRoutine);
    }

    private void OnDestroy()
    {
        SoundHelper.StopAndReleaseInstance(muckFireBreathSoundInstance);
        SoundHelper.StopAndReleaseInstance(muckSpewSoundInstance);

        if (muckRoutine == null) return;
        
        StopCoroutine(muckRoutine);
    }
}
