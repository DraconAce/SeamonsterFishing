using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BlinkBehaviour requires the GameObject to have a Animator component
[RequireComponent(typeof(Animator))]
public class BlinkBehaviour : MonoBehaviour
{
    private Animator mAnimator;
    
    private bool blinkingIsActive = true;
    private IEnumerator BlinkingCoroutine;
    
    [SerializeField] private float minCooldown = 2;
    [SerializeField] private float maxCooldown = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        mAnimator = GetComponent<Animator>();
        //Debug.Log(GetComponent<Animator>());
        BlinkingCoroutine = DoBlink();
        StartCoroutine(BlinkingCoroutine);
    }
    
    private IEnumerator DoBlink() 
    { 
        while (blinkingIsActive)
        {
            float waitTime = Random.Range(minCooldown, maxCooldown);
            //Debug.Log("Blink wait:" + waitTime);
            yield return new WaitForSeconds(waitTime);
            BlinkOnce();
        }
    }
    
    private void BlinkOnce()
    {
        mAnimator.SetTrigger("TrBlink");
    }
}
