using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]

public class AbstractOverlayTextElement : AbstractOverlayElement
{
    protected TextMeshProUGUI textElement;

    protected virtual void Awake() => TryGetComponent(out textElement);

    protected override void OnDisplayInfoUpdated() {}
}