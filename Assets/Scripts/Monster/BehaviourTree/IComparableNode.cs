public interface IComparableNode
{
    public int Priority { get; } //doesn't change

    bool IsNodeExecutable { get; set; }
    float GetExecutability(); //1-100, changes with game situation
}