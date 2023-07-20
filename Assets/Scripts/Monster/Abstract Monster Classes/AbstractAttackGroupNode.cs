using System.Linq;
using UnityEngine;

public abstract class AbstractAttackGroupNode : AbstractDecisionNodeImpl
{
    [SerializeField] private float executability = 75f;

    protected abstract MonsterAttackType AttackType { get; }
    
    private bool isExecutable;
    public override bool IsNodeExecutable
    {
        get => isExecutable; 
        set{}
    }

    //Todo: Override IsNodeExecutable in child classes
    
    //short range: whether in cannon station but also small possibility of still activating
    //swipe: weakpoint hit/ hit miss
    //longrange: works with timeout

    protected override void Start()
    {
        base.Start();
        
        var attacksOfThisType = FindObjectsByType<AbstractAttackNode>
            (FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(attack => attack.AttackType == AttackType);

        if (!attacksOfThisType.Any()) return;
        
        isExecutable = true;
    }

    public override float GetExecutability() => executability;
}