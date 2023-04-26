using UnityEngine;

public class SingletonMonoBehaviour : MonoBehaviour
{
    public bool AddedToDontDestroy { get; set; }
    
    public virtual void OnCreated(){}
}
