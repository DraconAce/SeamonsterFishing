using System;
using DG.Tweening;
using UnityEngine;

public class BoatRotationMaintainer : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    private float yRotationToKeep;
    
    private void Start()
    {
        yRotationToKeep = transform.localEulerAngles.y;
        
        DOVirtual.Float(0, 1, rotationSpeed, DynamicYRotationLock)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetSpeedBased(true);
    }

    private void DynamicYRotationLock(float interpolationValue)
    {
        var currentLocalRotation = transform.localEulerAngles;
        
        var targetRotation = currentLocalRotation;
        targetRotation.y = yRotationToKeep;

        transform.localEulerAngles = Vector3.Lerp(currentLocalRotation, targetRotation, interpolationValue);
    }
}