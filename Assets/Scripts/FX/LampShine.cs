using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]

public class LampShine : MonoBehaviour
{

    private Light light;

    private float flashTime;
    [SerializeField] private float flashEndTime = 0.6f;

    [SerializeField] private AnimationCurve intensityCurve;

    // Start is called before the first frame update
    void Awake()
    {
        light = GetComponent<Light>();
    }

    void OnEnable()
    {
        //When the gameObject gets enabled reset Timer and light intensity
        flashTime = 0f;
        light.intensity = intensityCurve.Evaluate(flashTime);
    }

    void HandleFlash()
    {
        flashTime += Time.deltaTime;
        light.intensity = intensityCurve.Evaluate(flashTime);
        if (flashTime > flashEndTime)
        {
            //disable the gameObject this script is attached to
            gameObject.SetActive(false);
            //disable this script only
            //this.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        HandleFlash();
    }
}

