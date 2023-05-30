using System;
using UnityEngine;

public class CannonReloadUINotifier : MonoBehaviour, IThreeStageStateInfoProvider
{
    [SerializeField] private CannonStation_Shooting shootingSegment;
    [SerializeField] private CannonStation_Reload reloadSegment;
    
    public event Action InfoChanged;

    private ThreeStageProcessState info = ThreeStageProcessState.Ready;

    public ThreeStageProcessState Info
    {
        get => info;
        set
        {
            var formerInfo = info;

            info = value;

            if (info == formerInfo) return;
            InfoChanged?.Invoke();
        }
    }

    private void Start()
    {
        shootingSegment.SegmentStateChanged += CheckReloadState;
        reloadSegment.SegmentStateChanged += CheckReloadState;
        
        CheckReloadState();
    }

    private void CheckReloadState()
    {
        if (reloadSegment.IsLoaded)
            Info = ThreeStageProcessState.Ready;
        else if (reloadSegment.IsReloading)
            Info = ThreeStageProcessState.Preparing;
        else
            Info = ThreeStageProcessState.NotReady;
    }

    private void OnDestroy()
    {
        shootingSegment.SegmentStateChanged -= CheckReloadState;
        reloadSegment.SegmentStateChanged -= CheckReloadState;
    }
}
