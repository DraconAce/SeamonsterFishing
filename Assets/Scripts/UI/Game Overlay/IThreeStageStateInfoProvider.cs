using System;

public interface IThreeStageStateInfoProvider
{
    public event Action InfoChanged;

    public ThreeStageProcessState Info { get; set; }
}