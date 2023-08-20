using System;
using TMPro;
using UnityEngine;

public class ControlText : MonoBehaviour
{
    [SerializeField] private string GeneralText;
    [SerializeField] private string ControllerInsertText;
    [SerializeField] private string KeyboardInsertText;

    private TextMeshProUGUI textOb;
    private InputManager inputManager;
    public CanvasGroup CanvasGroup { get; private set; }

    private void Awake()
    {
        textOb = GetComponent<TextMeshProUGUI>();
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start() => inputManager = InputManager.instance;

    public void UpdateControlsTextBasedOnUsedControls()
    {
        var textToInsert = inputManager.IsPlayerUsingController ? ControllerInsertText : KeyboardInsertText;
        
        textOb.text = string.Format(GeneralText, textToInsert);
    }
}