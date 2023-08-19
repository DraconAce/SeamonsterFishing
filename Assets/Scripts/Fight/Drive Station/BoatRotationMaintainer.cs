using System;
using DG.Tweening;
using UnityEngine;

public class BoatRotationMaintainer : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    
    private void Start()
    {
        var currentRotation = transform.localEulerAngles;
        
        transform.DOLocalRotate(currentRotation, rotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }
}