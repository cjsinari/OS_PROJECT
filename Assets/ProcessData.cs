using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessData : MonoBehaviour
{
    public Vector3 originalPosition;
    // Start is called before the first frame update
    void Start()
    {
        //stores original position of the processes
        originalPosition = transform.position; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
