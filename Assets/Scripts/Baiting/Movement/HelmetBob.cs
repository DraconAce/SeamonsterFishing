using System;
using DG.Tweening;
using UnityEngine;

public class HelmetBob : MonoBehaviour
{
    [SerializeField] private float moveDistance = 0.05f;
    [SerializeField] private float helmetBobDuration = 0.5f;
    [SerializeField] private Ease helmetBobEase = Ease.InOutSine;
    
    private void Start() => StartHelmetBobAnimation();

    private void StartHelmetBobAnimation()
    {
        transform.DOMove(new Vector3(0,moveDistance, 0), helmetBobDuration)
            .SetRelative(true)
            .SetEase(helmetBobEase)
            .SetLoops(-1, LoopType.Yoyo);
    }
}