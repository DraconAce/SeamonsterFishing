using System;
using DG.Tweening;
using UnityEngine;

public class PlayerRepMovement : MonoBehaviour
{
    [SerializeField] private float movementDuration;
    [SerializeField] private Ease movementEase;

    private Transform playerRepTransform;
    private Tween movementTween;

    private void Start() => playerRepTransform = PlayerSingleton.instance.PhysicalPlayerRepresentation;

    public void MovePlayerToTargetPos(Transform targetPosition)
    {
        movementTween.Kill();

        playerRepTransform.DOMove(targetPosition.position, movementDuration)
            .SetEase(movementEase);
    }

    public void MovePlayerToTargetLocalPos(Transform targetPosition)
    {
        movementTween?.Kill();

        var targetInLocalSpace = playerRepTransform.parent.InverseTransformPoint(targetPosition.position);

        playerRepTransform.DOLocalMove(targetInLocalSpace, movementDuration)
            .SetEase(movementEase);
    }

    private void OnDestroy() => movementTween?.Kill();
}