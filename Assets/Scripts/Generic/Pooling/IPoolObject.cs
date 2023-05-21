public interface IPoolObject
{
    public PoolObjectContainer ContainerOfObject { get; set; }
    public void ResetInstance();
    public void OnInstantiation(PoolObjectContainer container) => ContainerOfObject = container;
    public void ReturnInstanceToPool() => ContainerOfObject.SourcePool.ReturnInstance(ContainerOfObject);
    public void OnReturnInstance(){}
}