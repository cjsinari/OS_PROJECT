using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Scheduler : MonoBehaviour
{
    public TMP_Text cpuExecutionText;

    [SerializeField] private GameObject timelineContainer;
    [SerializeField] private GameObject timelineSegmentPrefab;
    [SerializeField] private Button startButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Toggle algorithmToggle;

    private List<Process> processList = new List<Process>();
    private List<(int processId, float startTime, float duration)> timelineHistory = new List<(int, float, float)>();
    private bool useSRTF = false;

    void Start()
    {
        // Initialize processes
        processList.Add(new Process(1, 0, 5, Color.red));
        processList.Add(new Process(2, 2, 3, Color.green));
        processList.Add(new Process(3, 4, 1, Color.blue));

        startButton.onClick.AddListener(ExecuteScheduling);
        resetButton.onClick.AddListener(ResetSimulation);
        algorithmToggle.onValueChanged.AddListener(OnAlgorithmToggleChanged);
    }

    private void OnAlgorithmToggleChanged(bool isOn)
    {
        useSRTF = isOn;
        UnityEngine.Debug.Log(useSRTF ? "SRTF Mode Enabled" : "FCFS Mode Enabled");
    }

    private void ExecuteScheduling()
    {
        if (useSRTF)
        {
            ExecuteSRTF();
        }
        else
        {
            ExecuteFCFS();
        }
    }

    public void ExecuteFCFS()
    {
        StartCoroutine(ExecuteProcessesFCFS());
    }

    private IEnumerator ExecuteProcessesFCFS()
    {
        float currentTime = 0;

        foreach (var process in processList)
        {
            cpuExecutionText.text = "Executing Process " + process.Id;
            UnityEngine.Debug.Log($"Executing Process {process.Id} | Start: {currentTime}, Duration: {process.BurstTime}");
            yield return new WaitForSeconds(process.BurstTime);

            UpdateTimeline(process, currentTime, process.BurstTime);
            currentTime += process.BurstTime;
        }

        cpuExecutionText.text = "All Processes Completed";
        UnityEngine.Debug.Log("All Processes Completed in FCFS Mode.");
    }

    public void ExecuteSRTF()
    {
        StartCoroutine(ExecuteProcessesSRTF());
    }

    private IEnumerator ExecuteProcessesSRTF()
    {
        float currentTime = 0;
        List<Process> readyQueue = new List<Process>();
        List<Process> completedProcesses = new List<Process>();

        Process executingProcess = null;

        while (completedProcesses.Count < processList.Count)
        {
            // Update ready queue with eligible processes
            readyQueue = processList
                .Where(p => p.ArrivalTime <= currentTime && !completedProcesses.Contains(p))
                .OrderBy(p => p.RemainingTime)
                .ThenBy(p => p.ArrivalTime)
                .ToList();

            if (readyQueue.Count > 0)
            {
                // Preempt if a new process with shorter remaining time arrives or if no process is currently executing
                if (executingProcess == null || readyQueue[0].RemainingTime < executingProcess.RemainingTime)
                {
                    if (executingProcess != null)
                    {
                        // Update timeline for the preempted process
                        UpdateTimeline(executingProcess, currentTime, currentTime - executingProcess.RemainingTime);
                    }

                    executingProcess = readyQueue[0];
                }

                // Execute the current process
                cpuExecutionText.text = "Executing Process " + executingProcess.Id;

                float executionTime = Mathf.Min(1f, executingProcess.RemainingTime);
                executingProcess.RemainingTime -= executionTime;
                currentTime += executionTime;

                UnityEngine.Debug.Log($"Executing Process {executingProcess.Id} | Start: {currentTime - executionTime}, Execution Time: {executionTime}, Remaining Time: {executingProcess.RemainingTime}");

                yield return new WaitForSeconds(executionTime);

                UpdateTimeline(executingProcess, currentTime - executionTime, executionTime);

                if (executingProcess.RemainingTime <= 0)
                {
                    completedProcesses.Add(executingProcess);
                    UnityEngine.Debug.Log($"Process {executingProcess.Id} Completed.");
                    executingProcess = null; // Reset executingProcess for the next iteration
                }
            }
            else
            {
                UnityEngine.Debug.Log("No processes ready. Advancing time by 1 unit.");
                currentTime += 1f;
                yield return new WaitForSeconds(1f);
            }
        }

        cpuExecutionText.text = "All Processes Completed";
        UnityEngine.Debug.Log("All Processes Completed in SRTF Mode.");
    }

    private void UpdateTimeline(Process process, float startTime, float duration)
    {
        // Append the new execution block to the timeline history
        timelineHistory.Add((process.Id, startTime, duration));

        // Clear the timeline container (only for re-rendering from history)
        foreach (Transform child in timelineContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Render all blocks from the history list
        foreach (var (id, start, length) in timelineHistory)
        {
            for (float t = 0; t < length; t += 1f)
            {
                GameObject segment = Instantiate(timelineSegmentPrefab, timelineContainer.transform);
                Image image = segment.GetComponent<Image>();
                Process p = processList.First(p => p.Id == id);
                image.color = p.ProcessColor;

                RectTransform rect = segment.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2((start + t) * 50, 0);
                rect.sizeDelta = new Vector2(40, rect.sizeDelta.y);

                TextMeshProUGUI label = segment.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = $"P{id}";
                    label.color = Color.black;
                }
            }
        }
    }

    private void ResetSimulation()
    {
        cpuExecutionText.text = "";
        timelineHistory.Clear();

        foreach (Transform child in timelineContainer.transform)
        {
            Destroy(child.gameObject);
        }

        processList = new List<Process>
        {
            new Process(1, 0, 5, Color.red),
            new Process(2, 2, 3, Color.green),
            new Process(3, 4, 1, Color.blue)
        };

        UnityEngine.Debug.Log("Simulation Reset.");
    }
}

[System.Serializable]
public class Process
{
    public int Id;
    public float ArrivalTime;
    public float BurstTime;
    public float RemainingTime;
    public Color ProcessColor;
    public bool IsCompleted;

    public Process(int id, float arrivalTime, float burstTime, Color color)
    {
        Id = id;
        ArrivalTime = arrivalTime;
        BurstTime = burstTime;
        RemainingTime = burstTime;
        ProcessColor = color;
        IsCompleted = false;
    }

    public void Reset()
    {
        RemainingTime = BurstTime;
        IsCompleted = false;
    }
}