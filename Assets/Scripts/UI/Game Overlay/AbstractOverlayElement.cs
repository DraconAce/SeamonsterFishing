using UnityEngine;

public abstract class AbstractOverlayElement : MonoBehaviour
{
    [SerializeField] protected GameObject infoProviderOb;
    
    protected abstract void OnDisplayInfoUpdated();
}