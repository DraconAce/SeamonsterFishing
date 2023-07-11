using System;

public interface IBoolInfoProvider
{
    public event Action InfoChanged;

    public bool Info { get; set; }
}