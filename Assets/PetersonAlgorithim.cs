using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetersonAlgorithim : MonoBehaviour
{
    public GameObject[] processes; // Array of processes
    public GameObject resources; // The shared resource
    public float delayAfterExit = 0.5f; // Delay enforced after a process leaves the critical section
    private int turn = 0; // Shared turn variable
    private bool[] flag = new bool[3]; // Flags for each process

    // Color for each state
    private Color idleColor = Color.gray;
    private Color waitingColor = Color.yellow;
    private Color requestingColor = Color.blue;
    private Color criticalSectionColor = Color.green;

    // Speed for process movement to critical section
    public float moveSpeed = 2f;

    void Start()
    {
        // Initialize all the flags to false and set processes to idle
        for (int i = 0; i < 3; i++)
        {
            flag[i] = false;
            SetColor(processes[i], idleColor);
        }

        // Start the routines for all processes
        StartCoroutine(ProcessRoutine(0));
        StartCoroutine(ProcessRoutine(1));
        StartCoroutine(ProcessRoutine(2));
    }

    private System.Collections.IEnumerator ProcessRoutine(int id)
    {
        while (true)
        {
            // Idle state
            SetColor(processes[id], idleColor);
            yield return new WaitForSeconds(Random.Range(1f, 3f));

            // Set the process as waiting
            turn = id;
            flag[id] = true;
            SetColor(processes[id], waitingColor);
            yield return new WaitForSeconds(1f);

            // Set the process as requesting
            turn = (id + 1) % 3;
            turn = (id + 2) % 3;
            SetColor(processes[id], requestingColor);
            yield return new WaitForSeconds(1f);

            // Wait until it is this process's turn and no other process is in the critical section
            while (flag[turn] && turn != id && flag[(id + 1) % 3] && flag[(id + 2) % 3])
                yield return null;

            // Move towards the resource
            yield return StartCoroutine(MoveToResource(processes[id]));

            // Critical section
            SetColor(processes[id], criticalSectionColor);
            yield return new WaitForSeconds(2f);

            // Move back to original position
            yield return StartCoroutine(MoveToOriginalPosition(processes[id]));

            // Delay before releasing the critical section
            yield return new WaitForSeconds(delayAfterExit);

            // Exit the critical section
            flag[id] = false;

            // Pass the turn to the next process
            turn = (id + 1) % 3;

            yield return null;
        }
    }

    private void SetColor(GameObject process, Color color)
    {
        SpriteRenderer renderer = process.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    private System.Collections.IEnumerator MoveToResource(GameObject process)
    {
        Vector3 resourcePosition = resources.transform.position;
        while (Vector3.Distance(process.transform.position, resourcePosition) > 0.1f)
        {
            process.transform.position = Vector3.MoveTowards(
                process.transform.position,
                resourcePosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private System.Collections.IEnumerator MoveToOriginalPosition(GameObject process)
    {
        Vector3 originalPosition = process.GetComponent<ProcessData>().originalPosition;
        while (Vector3.Distance(process.transform.position, originalPosition) > 0.1f)
        {
            process.transform.position = Vector3.MoveTowards(
                process.transform.position,
                originalPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
}

