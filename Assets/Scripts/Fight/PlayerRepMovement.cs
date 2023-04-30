using DG.Tweening;
using UnityEngine;

public class PlayerRepMovement : MonoBehaviour
{
    [SerializeField] private float movementDuration;
    [SerializeField] private Ease movementEase;

    private Tween movementTween;

    public void MovePlayerToTargetPos(Transform targetPosition)
    {
        movementTween.Kill();

        var newPlayerPos = targetPosition.position;
        newPlayerPos.y = transform.position.y;
        
        transform.DOMove(newPlayerPos, movementDuration)
            .SetEase(movementEase);
    }

    private void OnDestroy() => movementTween?.Kill();
}