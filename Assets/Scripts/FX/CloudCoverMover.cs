using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMover : MonoBehaviour
{
    [Range(0, 50)] 
    public float rotationSpeed = 1f;    
    
    [SerializeField] private Transform rotateAroundTransform;

    private void FixedUpdate()
    {
        Vector3 position = rotateAroundTransform.position;
        Vector3 axis =  new Vector3(0,1,0);
        transform.RotateAround(position, axis, Time.deltaTime * rotationSpeed);
    }
}
