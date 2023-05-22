using System;
using System.Collections;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] private Material hitMat;
    private Material normalMat;

    private MeshRenderer renderer;

    private readonly WaitForSeconds changeMatWait = new(1.5f);

    private readonly string cannonBallTag = "cannonBall";

    private void Start()
    {
        TryGetComponent(out renderer);

        normalMat = renderer.material;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag(cannonBallTag)) return;

        StartCoroutine(WeakPointHitRoutine());
    }

    private IEnumerator WeakPointHitRoutine()
    {
        ChangeMaterial(hitMat);
        
        yield return changeMatWait;
        
        ChangeMaterial(normalMat);
    }

    private void ChangeMaterial(Material newMat) => renderer.material = newMat;
}