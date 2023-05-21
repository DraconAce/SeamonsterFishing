public interface IPoolObject
{
    public PoolObjectContainer ContainerOfObject { get; set; }
    public void ResetInstance();
    public void OnReturnInstance(){}
    public void ReturnInstanceToPool() => ContainerOfObject.SourcePool.ReturnInstance(ContainerOfObject);
    public void OnInitialisation(PoolObjectContainer container) => ContainerOfObject = container;
}