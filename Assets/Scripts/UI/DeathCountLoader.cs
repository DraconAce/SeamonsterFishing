using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCountLoader : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("Load OnEnable");
        GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("Deaths", 0).ToString();
    }
    
}
