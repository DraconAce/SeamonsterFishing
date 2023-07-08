using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBehaviourTranslator<U> : Singleton<AbstractBehaviourTranslator<U>> where U : IDecisionRuntimeRep
{
    protected readonly Dictionary<string, U> allBehaviours = new();

    public override bool AddToDontDestroy => false;

    public void RegisterBehaviour(U behaviourToRegister)
    {
        if (allBehaviours.ContainsValue(behaviourToRegister)) return;
        
        allBehaviours.Add(behaviourToRegister.GetBehaviourID, behaviourToRegister);
    }
}