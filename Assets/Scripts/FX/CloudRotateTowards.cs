using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudRotateTowards : MonoBehaviour
{
    [Range(0, 50)] 
    public float rotationSpeed = 1f;    
    
    [SerializeField] private Transform target;

    private void FixedUpdate()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = target.position - transform.position;

        // The step size is equal to rotationSpeed times frame time.
        float singleStep = rotationSpeed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
