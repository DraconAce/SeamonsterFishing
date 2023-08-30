using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCountLoader : MonoBehaviour
{
    void OnEnable()
    {
        var numDeaths = PlayerPrefs.GetInt("Deaths", 0) + 1;
        
        GetComponent<TMP_Text>().text = numDeaths.ToString();
        
        PlayerPrefs.SetInt("Deaths", numDeaths);
        PlayerPrefs.Save();
    }
    
}
