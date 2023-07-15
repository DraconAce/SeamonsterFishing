using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookController : MonoBehaviour, IManualUpdateSubscriber
{
    [Serializable]
    private struct RotationLimits
    {
        public float HorizontalLimit;
        public float VerticalLimit;
    }

    [SerializeField] private RotationLimits lookLimit;
    [SerializeField] private float lookSpeed = 10f;
    
    private UpdateManager updateManager;
    private PlayerSingleton playerSingleton;

    private PlayerInputs customPlayerInputs;
    private InputAction lookAction;

    private Vector3 targetLookAngle;

    private WaitForSeconds deferredSubscriptionTime = new(0.25f);

    private void Awake()
    {
        customPlayerInputs = new();
        lookAction = customPlayerInputs.Baiting.Look;
        lookAction.Enable();
    }

    private void Start() => StartCoroutine(DeferredUpdateSubscription());

    private IEnumerator DeferredUpdateSubscription()
    {
        yield return deferredSubscriptionTime;
        
        playerSingleton = PlayerSingleton.instance;
        
        updateManager = UpdateManager.instance;
        updateManager.SubscribeToManualUpdate(this);
    }

    public void ManualUpdate()
    {
        if(playerSingleton.DisableMovementControls) return;

        UpdateLookRotation();
    }

    private void UpdateLookRotation()
    {
        var newLocalRotation = CalculateNewLocalRotation();
        var newLocalRotationQuaternion = ClampRotation(newLocalRotation);

        transform.localRotation = newLocalRotationQuaternion ;
    }

    private Vector3 CalculateNewLocalRotation()
    {
        var lookInput = lookAction.ReadValue<Vector2>();

        var lookVector = new Vector3(-lookInput.y, lookInput.x, 0);
        var rotationToAdd = Time.deltaTime * lookSpeed * lookVector;

        targetLookAngle += rotationToAdd;
        
        return targetLookAngle;
    }

    private Quaternion ClampRotation(Vector3 rotationToClamp)
    {
        var eulerOfClampRotation = ConvertToRotationWithinNegativeAngles(rotationToClamp);

        eulerOfClampRotation.x = Mathf.Clamp(eulerOfClampRotation.x, -lookLimit.VerticalLimit, lookLimit.VerticalLimit);
        eulerOfClampRotation.y = Mathf.Clamp(eulerOfClampRotation.y, -lookLimit.HorizontalLimit, lookLimit.HorizontalLimit);

        targetLookAngle = eulerOfClampRotation;
        
        return Quaternion.Euler(eulerOfClampRotation);
    }

    private Vector3 ConvertToRotationWithinNegativeAngles(Vector3 rotationToConvert)
    {
        rotationToConvert.x = AdjustRotationToNegative(rotationToConvert.x);
        rotationToConvert.y = AdjustRotationToNegative(rotationToConvert.y);
        rotationToConvert.z = 0;

        return rotationToConvert;
    }

    private float AdjustRotationToNegative(float rotationToAdjust)
    {
        return rotationToAdjust switch
        {
            > 180 => rotationToAdjust - 360,
            < -180 => rotationToAdjust + 360,
            _ => rotationToAdjust
        };
    }

    private void OnDestroy()
    {
        if (updateManager == null) return;
        updateManager.UnsubscribeFromManualUpdate(this);
    }
}