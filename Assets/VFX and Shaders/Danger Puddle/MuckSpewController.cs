using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Animations;

public class MuckSpewController : MonoBehaviour
{
    [SerializeField] private MuckAttack muckAttack;

    [SerializeField] private float delayForMuckBuildUp = 1.5f;
    [SerializeField] private float delayForSpewReady = 3.5f;
    [SerializeField] private float delayForSpewEnd = 1f;
    [SerializeField] private float delayForFireSpew = 6f;
    [SerializeField] private float delayForMuckFire = 0.2f;
    [SerializeField] private float delayForMuckEnd = 1.5f;

    [SerializeField] private GameObject Muck_Spew_Gameobject;
    [SerializeField] private GameObject muckExplosionPrefab;
     
    [SerializeField] private EventReference MuckFireBreathSound;
    [SerializeField] private EventReference MuckSpewSound;
    
    [Header("Animation Triggers")]
    [SerializeField] private string buildUpTrigger = "MuckBuildUp";
    [SerializeField] private string spewTrigger = "SpewMuck";
    [SerializeField] private string endSpewTrigger = "EndSpewMuck";

    private Vector3 savedPosition_Player;
    private EventInstance muckFireBreathSoundInstance;
    private EventInstance muckSpewSoundInstance;

    private Transform playerTransform;
    private Transform spewTransform;

    private Material mat_MuckSpew;
    private ParticleSystem Muck_Spew;

    private PrefabPool muckExplosionPool;
    private MuckGoo currentGoo;

    private WaitForSeconds waitForMuckBuildUp;
    private WaitForSeconds waitForSpewReady;
    private WaitForSeconds waitForSpewEnd;
    private WaitForSeconds waitForFireSpew;
    private WaitForSeconds waitForMuckOnFire;
    private WaitForSeconds waitForEndMuck;

    private Coroutine muckRoutine;
    
    private MonsterAnimationController monsterAnimationController;

    private void Start()
    {
        monsterAnimationController = FightMonsterSingleton.instance.MonsterAnimationController;
        playerTransform = PlayerSingleton.instance.PhysicalPlayerRepresentation;
        spewTransform = Muck_Spew_Gameobject.transform;
        
        muckExplosionPool = PrefabPoolFactory.instance.RequestNewPool(gameObject, muckExplosionPrefab);

        muckFireBreathSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MuckFireBreathSound, Muck_Spew_Gameobject);
        muckSpewSoundInstance = SoundHelper.CreateSoundInstanceAndAttachToTransform(MuckSpewSound, Muck_Spew_Gameobject);

        Muck_Spew = Muck_Spew_Gameobject.GetComponent<ParticleSystem>();
        mat_MuckSpew = Muck_Spew_Gameobject.GetComponent<Renderer>().material;

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
        monsterAnimationController.SetTrigger(buildUpTrigger);
        yield return waitForMuckBuildUp;
        
        monsterAnimationController.SetTrigger(spewTrigger);
        yield return waitForSpewReady;
        
        savedPosition_Player = playerTransform.position;
        Debug.Log(savedPosition_Player);
        StartSpew();

        currentGoo = RequestNewMuckExplosionAndPlace(savedPosition_Player);

        yield return waitForSpewEnd;
        monsterAnimationController.SetTrigger(endSpewTrigger);

        yield return waitForFireSpew;
        monsterAnimationController.SetTrigger(spewTrigger);

        yield return waitForSpewReady;
        
        StartFireBreath();
        
        yield return waitForMuckOnFire;
        SetMuckOnFire();

        yield return waitForEndMuck;
        
        TriggerMuckEnd();
    }

    private void StartSpew()
    {
        muckSpewSoundInstance.start();
        
        AdjustMuckSpewStrength();
    }

    private void AdjustMuckSpewStrength()
    {
        var spewPos = spewTransform.position;
        
        //Adjust the Muck Spew initial Speed -> speed up the spew if the player is further away
        var distance = savedPosition_Player - spewPos;
        
        //Debug.Log("Spew distance: " + distance);
        var bonusSpeed = distance[0] - distance[1] - distance[2]; //positive x, negative y and z = further away
        var main = Muck_Spew.main;
        main.startSpeed = bonusSpeed;
        
        Muck_Spew.Play();
    }

    private MuckGoo RequestNewMuckExplosionAndPlace(Vector3 muckPosition)
    {
        var newGoo = muckExplosionPool.RequestInstance(muckPosition);
        newGoo.TryGetCachedComponent<MuckGoo>(out var muckGoo);

        return muckGoo;
    }

    private void StartFireBreath()
    {
        //Muck Spews towards old player position -> the Muck that is supposed to burn
        //also automatically adjusts Strength
        StartSpew();
        
        mat_MuckSpew.EnableKeyword("_EMISSION");
        Muck_Spew.Play();
        muckFireBreathSoundInstance.start();
    }

    private void SetMuckOnFire()
    {
        if (currentGoo == null) return;
        
        currentGoo.StartMuckFire();
    }

    private void TriggerMuckEnd()
    {
        muckAttack.TriggerMuckEnd();
        monsterAnimationController.SetTrigger(endSpewTrigger);
    }

    public void StopMuckRoutine()
    {
        if (muckRoutine == null) return;
            
        StopCoroutine(muckRoutine);
    }

    private void OnDestroy()
    {
        if (muckRoutine == null) return;
        
        StopCoroutine(muckRoutine);
    }
}
