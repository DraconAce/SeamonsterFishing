using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateFlash : MonoBehaviour
{
    [SerializeField] private GameObject lightOne;
    [SerializeField] private GameObject lightTwo;
    [SerializeField] private bool activateLights;

    void OnEnable()
    {
        if (activateLights)
        {
            lightOne.SetActive(true);
            lightTwo.SetActive(true);
        }
        else
        {
            lightOne.SetActive(false);
            lightTwo.SetActive(false);
        }
    }
}
