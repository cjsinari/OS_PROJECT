using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class Note : MonoBehaviour
{
    public int noteID;  // Sticky note ID
    public Text noteText; //Reference to the text UI element

    public void SetNoteID(int id)
    {
        noteID = id;
        if (noteText != null)
        {
            noteText.text = "Sticky " + noteID;
        }
    }
}

