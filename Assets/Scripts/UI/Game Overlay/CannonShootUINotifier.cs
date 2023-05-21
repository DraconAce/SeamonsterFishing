using System;
using UnityEngine;

public class CannonShootUINotifier : MonoBehaviour, IBoolInfoProvider
{
    [SerializeField] private CannonStation_Shooting shootingSegment;
    
    public event Action InfoChanged;

    private bool info = true;

    public bool Info
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
        shootingSegment.SegmentStateChanged += CheckShootState;
        
        CheckShootState();
    }

    private void CheckShootState() => Info = shootingSegment.ShootIsScheduled;

    private void OnDestroy() => shootingSegment.SegmentStateChanged -= CheckShootState;
}