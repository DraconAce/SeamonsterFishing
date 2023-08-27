using System;
using UnityEngine;

public class UIElementSelector : MonoBehaviour
{
    private InputManager inputManager;

    private void Start() => inputManager = InputManager.instance;

    public void SelectScreenObject(GameObject obToSelect)
    {
        inputManager.EventSystem.SetSelectedGameObject(null);
        inputManager.EventSystem.SetSelectedGameObject(obToSelect);
    }
}