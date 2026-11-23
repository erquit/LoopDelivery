using TMPro;
using UnityEngine;

public class ClueNode : MonoBehaviour
{
    public NotebookEntryDataSO clue;
    public TextMeshProUGUI titleText;

    public RectTransform Rect => (RectTransform)transform;
    public void Refresh()
    {
        bool known = GameManager.Instance.IsDiscovered(clue);
        gameObject.SetActive(known);
        if (known) titleText.text = clue.entryTitle;
    }
}
