using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCountSaver : MonoBehaviour
{   
    private int localDeathcounter;

    /*
    private void OnEnable()
    {
        Debug.Log("Death OnEnable");
        countDeath();
    }
    */
    public void countDeath()
    {
        Debug.Log("counting Death");
        localDeathcounter = PlayerPrefs.GetInt("Deaths", 0);
        localDeathcounter++;
        PlayerPrefs.SetInt("Deaths", localDeathcounter);
    }
}
