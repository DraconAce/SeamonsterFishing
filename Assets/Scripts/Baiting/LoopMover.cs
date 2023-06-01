using System;
using UnityEngine;

public class LoopMover : MonoBehaviour, IPoolObject, IManualUpdateSubscriber
{
    [SerializeField] private GameObject movePerSecondProviderOb;
    [SerializeField] private WallLoopManager wallLoopManager;

    private bool canMove;
    private Vector3 movementVector;
    private IMovePerSecondProvider movePerSecondProvider;

    private UpdateManager updateManager;
    
    public PoolObjectContainer ContainerOfObject { get; set; }

    private void Start()
    {
        movePerSecondProviderOb.TryGetComponent(out movePerSecondProvider);

        movementVector = Vector3.zero;
        movementVector[(int)wallLoopManager.MovementAxis] = 1;

        updateManager = UpdateManager.instance;
        
        updateManager.SubscribeToManualUpdate(this);
    }

    public void OnInitialisation(PoolObjectContainer container)
    {
        ContainerOfObject = container;

        canMove = true;
    }

    public void OnReturnInstance() => canMove = false;

    public void ResetInstance() => canMove = true;

    public void ManualUpdate()
    {
        if (!canMove) return;

        if (IsTargetPositionReached())
        {
            TargetPositionReached();
            return;
        }
        
        MoveObject();
    }

    private void MoveObject()
    {
        var trans = transform;
        var currentPos = trans.position;

        var targetPos = currentPos + movementVector * (Time.deltaTime * movePerSecondProvider.MovePerSecond * wallLoopManager.MoveDirection);

        trans.position = targetPos;
    }

    private bool IsTargetPositionReached() => transform.localPosition.y >= wallLoopManager.DespawnLocalTargetY;

    private void TargetPositionReached()
    {
        wallLoopManager.SpawnNewLoopElement();
        
        ((IPoolObject)this).ReturnInstanceToPool();
    }

    private void OnDestroy() => updateManager.UnsubscribeFromManualUpdate(this);
}