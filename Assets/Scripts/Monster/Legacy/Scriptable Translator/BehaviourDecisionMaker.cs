using System;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourDecisionMaker : AbstractBehaviourTranslator<BehaviourChoice>
{
    public string DecideOnNextBehaviour()
    {
        var choicesWhichCanBeExecuted = SearchForSuitableBehaviours();

        var choiceCount = choicesWhichCanBeExecuted.Count;

        if (choiceCount <= 0) return "";
        
        if (choicesWhichCanBeExecuted.Count == 1)
            return ExecuteChoiceAndReturnID(choicesWhichCanBeExecuted[0]);

        choicesWhichCanBeExecuted.Sort((a, b) 
            => a.MonsterBehaviour.Priority.CompareTo(b.MonsterBehaviour.Priority));

        return ExecuteChoiceAndReturnID(choicesWhichCanBeExecuted[0]);
    }

    private List<BehaviourChoice> SearchForSuitableBehaviours()
    {
        var choicesWhichCanBeExecuted = new List<BehaviourChoice>();
        
        foreach (var choice in allBehaviours.Values)
        {
            if (!choice.IsChoiceSuitable()) continue;

            choicesWhichCanBeExecuted.Add(choice);
        }

        return choicesWhichCanBeExecuted;
    }

    private string ExecuteChoiceAndReturnID(BehaviourChoice choice)
    {
        var behaviour = choice.MonsterBehaviour;

        behaviour.Execute();

        return choice.NodeToRepresent.BehaviourName;
    }
}