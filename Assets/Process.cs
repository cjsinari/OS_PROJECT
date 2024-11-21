using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Process : MonoBehaviour
{
    public CriticalSection criticalSection;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ProcessRoutine());
    }
    private System.Collections.IEnumerator ProcessRoutine()
    { 
        while(true)
        {
            Debug.Log("Requesting access to critical section");
            if (criticalSection.RequestAccess()) {
                yield return new WaitForSeconds(2f);
                criticalSection.RequestAccess();
                Debug.Log("Exited critical section");
            }
            

            else
            {
                Debug.Log("Critical section is in hold. Waiting...");
                yield return new WaitForSeconds(1f);
            }
        }
    }
        
    // Update is called once per frame
    void Update()
    {
        
    }
}
