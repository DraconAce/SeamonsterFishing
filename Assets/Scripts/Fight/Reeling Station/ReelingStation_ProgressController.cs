using DG.Tweening;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class ReelingStation_ProgressController : AbstractStationSegment
{
    [SerializeField] private GameObject reelingUI;
    [SerializeField] private EventReference ReelingSound;
    private EventInstance reelingSoundInstance;
    
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;
    
    private void Start()
    {
        reelingUI.SetActive(false);
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
        
        reelingSoundInstance = RuntimeManager.CreateInstance(ReelingSound);
    }
    
    private void OnReelingStarted() 
    {
        reelingUI.SetActive(true);
        reelingSoundInstance.start();
    } 
    private void OnReelingCompleted()
    {
        reelingSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        DOVirtual.DelayedCall(reelingStation.DelayForSubStationsOnReelingCompleted, 
            () => reelingUI.SetActive(false));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        reelingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        reelingSoundInstance.release();
        
        if(reelingStation == null) return;
        
        reelingStation.OnReelingStartedEvent -= OnReelingStarted;
        reelingStation.OnReelingCompletedEvent -= OnReelingCompleted;
    }
}