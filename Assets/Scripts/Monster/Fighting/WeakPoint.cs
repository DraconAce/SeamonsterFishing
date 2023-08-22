using System;
using System.Collections;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] private Material hitMat;
    
    private MeshRenderer renderer;
    private Collider collider;

    private readonly string cannonBallTag = "cannonBall";

    private void Start()
    {
        TryGetComponent(out renderer);
        TryGetComponent(out collider);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag(cannonBallTag)) return;

        WeakPointWasHit();
    }

    private void WeakPointWasHit()
    {
        collider.enabled = false;
        ChangeMaterial(hitMat);
    }

    private void ChangeMaterial(Material newMat) => renderer.material = newMat;
}