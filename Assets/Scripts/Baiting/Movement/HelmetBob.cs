using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class HelmetBob : MonoBehaviour
{
    [FormerlySerializedAs("moveDistance")] [SerializeField] private float verticalMoveDistance = 0.05f;
    [SerializeField] private float horizontalMoveDistance = 0.05f;
    [SerializeField] private float helmetBobDuration = 0.5f;
    [SerializeField] private Ease helmetBobEase = Ease.InOutSine;
    
    private void Start() => StartHelmetBobAnimation();

    private void StartHelmetBobAnimation()
    {
        var position = transform.position;
        
        var upPosition = position + new Vector3(0, verticalMoveDistance, 0);
        var downPosition = position + new Vector3(0, -verticalMoveDistance, 0);
        var leftPosition = position + new Vector3(-horizontalMoveDistance, 0, 0);
        var rightPosition = position + new Vector3(horizontalMoveDistance, 0, 0);
        
        var path = new [] {upPosition, rightPosition, downPosition, leftPosition};

        transform.DOMove(leftPosition, helmetBobDuration/4)
            .SetEase(helmetBobEase)
            .OnComplete(() =>
            {
                transform.DOPath(path, helmetBobDuration, PathType.CatmullRom)
                    .SetEase(helmetBobEase)
                    .SetLoops(-1, LoopType.Restart);
            });
    }
}