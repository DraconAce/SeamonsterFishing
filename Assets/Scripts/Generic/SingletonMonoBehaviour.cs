using UnityEngine;

public class SingletonMonoBehaviour : MonoBehaviour
{
    public virtual bool AddToDontDestroy => true;
    
    public virtual void OnCreated(){}
}
