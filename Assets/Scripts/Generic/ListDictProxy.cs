using System.Collections.Generic;

public class ListDictProxy <K, V>
{
    public virtual K Key { get; }
    public virtual V Value { get; }

    public static Dictionary<K, V> CreateDictionaryFromList(List<ListDictProxy<K,V>> listToCreateDictFrom)
    {
        var createdDict = new Dictionary<K, V>();
        
        foreach(var proxy in listToCreateDictFrom)
            createdDict.Add(proxy.Key, proxy.Value);

        return createdDict;
    }
}