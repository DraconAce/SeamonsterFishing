using System;
using System.Collections.Generic;
using UnityEngine;

public class TextOverlayThreeStageElement : AbstractOverlayTextElement
{
    [Serializable]
    private struct StateText
    {
        public ThreeStageProcessState Stage;
        public string StageText;
    }

    [SerializeField] private List<StateText> stageTextsList;
    private readonly Dictionary<ThreeStageProcessState, string> stageTextsDict = new();

    private IThreeStageStateInfoProvider infoProvider;

    protected override void Awake()
    {
        base.Awake();

        CreateStageTextDict();
        
        TryGetComponent(out textElement);

        infoProviderOb.TryGetComponent(out infoProvider);
        infoProvider.InfoChanged += OnDisplayInfoUpdated;
    }

    private void CreateStageTextDict()
    {
        foreach (var text in stageTextsList)
            stageTextsDict.Add(text.Stage, text.StageText);
    }

    private void Start() => AssignTextBasedOnStage();

    protected override void OnDisplayInfoUpdated() => AssignTextBasedOnStage();

    private void AssignTextBasedOnStage()
    {
        var stage = infoProvider.Info;

        textElement.text = stageTextsDict[stage];
    }
}