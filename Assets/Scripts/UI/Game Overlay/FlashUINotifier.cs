using System;
using UnityEngine;

public class FlashUINotifier : MonoBehaviour, IBoolInfoProvider
{
    [SerializeField] private SpotFlash flash;
    
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

    private void Start() => CheckFlashState();

    public void CheckFlashState() => Info = flash.FlashIsReady;
}