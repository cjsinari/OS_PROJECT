using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LRUManager : MonoBehaviour
{
    //References
    public GameObject notePrefab; //Prefab for page UI elements
    public Transform memoryPanel; //panel to display sticky notes
    public InputField noteInputField; //input field to enter note ID
    public int maxNotes = 4;  //max no. of sticky notes in memory
    public Text hitCounterText; //text element to show page hits
    public Text missCounterText; //text element to show page misses


    //List of note GameObjects in memory   
    private List<GameObject> noteObjects = new List<GameObject>();

    //Lookup for quick sticky note access
    private Dictionary<int, GameObject> noteLookup = new Dictionary<int, GameObject>();

    //Page hit and miss counters
    private int pageHits = 0;
    private int pageMisses = 0;


    //Method to access a sticky note
    public void AccessNote()
    {
        //validate input
        if (noteInputField != null)
        {
            string inputText = noteInputField.text;

            //validate input
            if (int.TryParse(inputText, out int noteID))
            {
                Debug.Log("Accessing sticky note: " + noteID);
                HandleNoteAccess(noteID);
            }
            else
            {
                Debug.LogWarning("Invalid Note ID entered.");
            }
        }
    }

    //Code logic for accessing & replacing a page
    private void HandleNoteAccess(int noteID)
    {
        //If the note is already in memory,
        //move it to the most recently used position
        if (noteLookup.ContainsKey(noteID))
        {
            Debug.Log($"Sticky note {noteID} is already in memory! (Page hit)");
            pageHits++;
            UpdateCounters();

            GameObject noteObject = noteLookup[noteID];

            //Move to end (most recently used)
            noteObjects.Remove(noteObject);
            noteObjects.Add(noteObject);
            UpdateNoteDisplay();
            return;

        }

        //If note is not in memory
        Debug.Log($"Note {noteID} is not in memory. Loading page... (Page miss)");
        pageMisses++;
        UpdateCounters();
        AddNoteToMemory(noteID);

    }

    //Adding new note to memory
    //and removing the least recently used when necessary
    private void AddNoteToMemory(int noteID)
    {
        if (noteObjects.Count >= maxNotes)
        {
            //Evict the least recently used note
            GameObject lruNote = noteObjects[0];
            noteObjects.RemoveAt(0);

            int lruNoteID = lruNote.GetComponent<Note>().noteID;
            noteLookup.Remove(lruNoteID);

            Destroy(lruNote); //Remove from the UI
            Debug.Log($"Removed sticky note {lruNoteID}.");

        }

        //Create a new page and add it to memory
        GameObject newNote = Instantiate(notePrefab, memoryPanel);
        Note noteComponent = newNote.GetComponent<Note>();
        noteComponent.SetNoteID(noteID);

        noteObjects.Add(newNote);
        noteLookup[noteID] = newNote;

        UpdateNoteDisplay();
   
    }

    //Updating visual order of notes in the memory panel
    private void UpdateNoteDisplay()
    {
        for (int i = 0; i < noteObjects.Count; i++)
        {
            RectTransform rect = noteObjects[i].GetComponent<RectTransform>();
            rect.SetSiblingIndex(i); // Ensure the visual order matches the logical order
        }
    }

    //Update the hit and miss counters on UI
    private void UpdateCounters()
    {
        if (hitCounterText != null)
        {
            hitCounterText.text = $"Hits: {pageHits}";
        }

        if(missCounterText != null)
        {
            missCounterText.text = $"Misses: {pageMisses}";
        }

    }


}