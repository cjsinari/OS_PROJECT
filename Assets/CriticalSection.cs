using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalSection : MonoBehaviour
{
    //indicates if critical section is occupied. Initaially set to false
    private bool isOccupied = false;

    public bool RequestAccess()
    {
        //Checks if critical section is available
        if (!isOccupied)
        {
            isOccupied = true;
            Debug.Log("Critical section access granted");
            return true;
        }
        Debug.LogWarning("Critical section access has been denied since it is occupied");
        return false;
    }
    public void ReleaseAccess()
    {
        if (isOccupied)
        {
            isOccupied = false;
            Debug.Log("Critical section has been released and is now available");

        }
        else
        {
            Debug.LogWarning("Attempted to release an unoccupied critical section.");
        }
    }
    public bool IsOccupied()
    {
        return isOccupied;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
