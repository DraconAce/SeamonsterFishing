using System;

public interface IFloatInfoProvider
{
    public event Action InfoChanged;

    public float Info { get; set; }
}