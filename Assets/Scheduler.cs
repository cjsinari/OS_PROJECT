using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Scheduler : MonoBehaviour
{
    public TMP_Text cpuExecutionText;
    public TMP_Text resultText; // Add this to display waiting times, turnaround times, and averages

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

        // This will hold the process execution details for displaying results later
        foreach (var process in processList.OrderBy(p => p.ArrivalTime)) // Ensure processes are executed in order of arrival
        {
            // Wait for the process to arrive if the current time is less than the process arrival time
            if (currentTime < process.ArrivalTime)
            {
                cpuExecutionText.text = $"Waiting for Process {process.Id} to arrive...";
                yield return new WaitForSeconds(process.ArrivalTime - currentTime);
                currentTime = process.ArrivalTime; // Update current time to the arrival time
            }

            cpuExecutionText.text = "Executing Process " + process.Id;
            UnityEngine.Debug.Log($"Executing Process {process.Id} | Start: {currentTime}, Duration: {process.BurstTime}");

            // Calculate waiting time and turnaround time
            process.WaitingTime = currentTime - process.ArrivalTime;
            process.TurnaroundTime = process.WaitingTime + process.BurstTime;

            // Display process execution in timeline
            UpdateTimeline(process, currentTime, process.BurstTime);

            // Wait for the duration of the process
            yield return new WaitForSeconds(process.BurstTime);

            // Update current time after process execution
            currentTime += process.BurstTime;
        }

        cpuExecutionText.text = "All Processes Completed";
        DisplayResults(); // Display results including waiting and turnaround times
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
            readyQueue = processList
                .Where(p => p.ArrivalTime <= currentTime && !completedProcesses.Contains(p))
                .OrderBy(p => p.RemainingTime)
                .ThenBy(p => p.ArrivalTime)
                .ToList();

            if (readyQueue.Count > 0)
            {
                if (executingProcess == null || readyQueue[0].RemainingTime < executingProcess.RemainingTime)
                {
                    executingProcess = readyQueue[0];
                }

                cpuExecutionText.text = "Executing Process " + executingProcess.Id;

                float executionTime = Mathf.Min(1f, executingProcess.RemainingTime);
                executingProcess.RemainingTime -= executionTime;
                currentTime += executionTime;

                UnityEngine.Debug.Log($"Executing Process {executingProcess.Id} | Start: {currentTime - executionTime}, Execution Time: {executionTime}, Remaining Time: {executingProcess.RemainingTime}");

                yield return new WaitForSeconds(executionTime);

                UpdateTimeline(executingProcess, currentTime - executionTime, executionTime);

                if (executingProcess.RemainingTime <= 0)
                {
                    executingProcess.TurnaroundTime = currentTime - executingProcess.ArrivalTime;
                    executingProcess.WaitingTime = executingProcess.TurnaroundTime - executingProcess.BurstTime;

                    completedProcesses.Add(executingProcess);
                    UnityEngine.Debug.Log($"Process {executingProcess.Id} Completed.");
                    executingProcess = null;
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
        DisplayResults();
        UnityEngine.Debug.Log("All Processes Completed in SRTF Mode.");
    }

    private void UpdateTimeline(Process process, float startTime, float duration)
    {
        timelineHistory.Add((process.Id, startTime, duration));

        // Clear existing timeline segments
        foreach (Transform child in timelineContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new timeline segments based on the history
        foreach (var (id, start, length) in timelineHistory)
        {
            for (float t = 0; t < length; t += 1f)
            {
                GameObject segment = Instantiate(timelineSegmentPrefab, timelineContainer.transform);
                Image image = segment.GetComponent<Image>();
                Process p = processList.First(proc => proc.Id == id);
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
        resultText.text = ""; // Clear results
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

    private void DisplayResults()
    {
        float totalWaitingTime = 0;
        float totalTurnaroundTime = 0;

        // Create a StringBuilder for more efficient string building
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        result.AppendLine("FCFS Scheduling Results:\n");

        // Create a table-like header
        result.AppendLine(string.Format("{0,-10} {1,-20} {2,-20} {3,-20}",
            "Process", "Arrival Time", "Waiting Time", "Turnaround Time"));
        result.AppendLine(new string('-', 60));

        // Calculate and display individual process times
        foreach (var process in processList.OrderBy(p => p.Id))
        {
            // Calculate waiting and turnaround times for each process
            process.WaitingTime = CalculateFCFSWaitingTime(process);
            process.TurnaroundTime = process.WaitingTime + process.BurstTime;

            // Add process details to the result
            result.AppendLine(string.Format("P{0,-9} {1,-20:F2} {2,-20:F2} {3,-20:F2}",
                process.Id,
                process.ArrivalTime,
                process.WaitingTime,
                process.TurnaroundTime));

            // Accumulate total times
            totalWaitingTime += process.WaitingTime;
            totalTurnaroundTime += process.TurnaroundTime;
        }

        // Add separator
        result.AppendLine(new string('-', 60));

        // Calculate and display average times
        float avgWaitingTime = totalWaitingTime / processList.Count;
        float avgTurnaroundTime = totalTurnaroundTime / processList.Count;

        result.AppendLine(string.Format("{0,-30} {1:F2}", "Total Average Waiting Time:", avgWaitingTime));
        result.AppendLine(string.Format("{0,-30} {1:F2}", "Total Average Turnaround Time:", avgTurnaroundTime));

        // Update UI Text
        resultText.text = result.ToString();

        // Log to console for debugging
        UnityEngine.Debug.Log(result.ToString());
    }

    // Helper method to calculate waiting time in FCFS
    private float CalculateFCFSWaitingTime(Process currentProcess)
    {
        // Find processes that arrived before the current process
        var previousProcesses = processList
            .Where(p => p.Id != currentProcess.Id && p.ArrivalTime < currentProcess.ArrivalTime)
            .OrderBy(p => p.ArrivalTime);

        float waitingTime = 0;
        float lastCompletionTime = 0;

        // Calculate waiting time based on previous processes
        foreach (var prevProcess in previousProcesses)
        {
            lastCompletionTime += prevProcess.BurstTime;
        }

        // Waiting time is the difference between last completion time and current process arrival
        waitingTime = Mathf.Max(0, lastCompletionTime - currentProcess.ArrivalTime);

        return waitingTime;
    }
}

[System.Serializable]
public class Process
{
    public int Id;
    public float ArrivalTime;
    public float BurstTime;
    public float RemainingTime;
    public float WaitingTime;
    public float TurnaroundTime;
    public Color ProcessColor;

    public Process(int id, float arrivalTime, float burstTime, Color color)
    {
        Id = id;
        ArrivalTime = arrivalTime;
        BurstTime = burstTime;
        RemainingTime = burstTime;
        WaitingTime = 0;
        TurnaroundTime = 0;
        ProcessColor = color;
    }
}