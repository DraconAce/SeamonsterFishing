using System;
using UnityEngine;
using UnityEngine.Animations;

public class LookAtPlayer : MonoBehaviour
{
    private bool sourceSet;
    private LookAtConstraint lookConstraint;

    private void Awake() => TryGetComponent(out lookConstraint);

    private void Start()
    {
        SetPlayerAsLookSource();

        lookConstraint.constraintActive = true;
    }

    private void SetPlayerAsLookSource()
    {
        var source = new ConstraintSource { sourceTransform = PlayerSingleton.instance.PlayerTransform, weight = 1 };
        lookConstraint.AddSource(source);

        sourceSet = true;
    }

    private void OnEnable()
    {
        if (!sourceSet) return;
        
        lookConstraint.constraintActive = true;
    }

    private void OnDisable()
    {
        if (!sourceSet) return;

        lookConstraint.constraintActive = false;
    }
}