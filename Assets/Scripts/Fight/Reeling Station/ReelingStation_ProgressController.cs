using DG.Tweening;
using UnityEngine;

public class ReelingStation_ProgressController : AbstractStationSegment
{
    [SerializeField] private GameObject reelingUI;
    
    private ReelingStation reelingStation => (ReelingStation) ControllerStation;
    
    private void Start()
    {
        reelingUI.SetActive(false);
        
        reelingStation.OnReelingStartedEvent += OnReelingStarted;
        reelingStation.OnReelingCompletedEvent += OnReelingCompleted;
    }
    
    private void OnReelingStarted() => reelingUI.SetActive(true);
    private void OnReelingCompleted()
    {
        DOVirtual.DelayedCall(reelingStation.DelayForSubStationsOnReelingCompleted, 
            () => reelingUI.SetActive(false));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if(reelingStation == null) return;
        
        reelingStation.OnReelingStartedEvent -= OnReelingStarted;
        reelingStation.OnReelingCompletedEvent -= OnReelingCompleted;
    }
}