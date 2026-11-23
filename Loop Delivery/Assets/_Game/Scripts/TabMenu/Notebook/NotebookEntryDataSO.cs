using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NotebookEntryDataSO", menuName = "Scriptable Objects/NotebookEntryDataSO")]
public class NotebookEntryDataSO : ScriptableObject
{
    public int entryID;
    public string entryTitle;
    public string entryDescription;

    public Clue[] clues; // Array of Clue structs associated with this notebook entry

    public List<NotebookEntryDataSO> leadsTo = new List<NotebookEntryDataSO>();
}
