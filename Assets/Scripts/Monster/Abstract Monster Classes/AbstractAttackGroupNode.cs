using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractAttackGroupNode : AbstractDecisionNodeImpl
{
    [SerializeField] private float executability = 75f;

    protected abstract MonsterAttackType AttackType { get; }
    
    protected bool isExecutable;
    public override bool IsNodeExecutable
    {
        get => isExecutable; 
        set{}
    }

    protected override void Start()
    {
        base.Start();

        if (!AttacksOfThisTypeExist()) return;
        
        isExecutable = true;
    }

    private bool AttacksOfThisTypeExist()
    {
        var attacksOfThisType = FindObjectsByType<AbstractAttackNode>
            (FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(attack => attack.AttackType == AttackType);
        
        return attacksOfThisType.Any();
    }

    public override float GetExecutability() => executability;
}