public interface IMonsterBehaviourDecisionState
{
    public int Priority { get; }
    bool CanBeExecuted();

    void Execute();
}