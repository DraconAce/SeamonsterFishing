using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuckToFire : MonoBehaviour
{
    
    [SerializeField] private bool isburning = false;
    [SerializeField] private bool isAlternativeFireLook = false;
    private Material mat_Muck;
    
    // Start is called before the first frame update
    void Start()
    {
        //Fetch the Material from the Renderer of the GameObject
        mat_Muck = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (isburning)
        {
            //set emission to "On"
            mat_Muck.EnableKeyword("_EMISSION");
        }
        else
        {
            //set emission to "Off"
            mat_Muck.DisableKeyword("_EMISSION");
        }
        
        if (isAlternativeFireLook)
        {
            //set main maps Y-tiling to 0.16
            mat_Muck.SetTextureScale("_MainTex", new Vector2(1f, 0.16f));
        }
        else
        {
            //set main maps Y-tiling back to 1
            mat_Muck.SetTextureScale("_MainTex", new Vector2(1f, 1f));    
        }
        
    }
}
