using System.Collections.Generic;
using UnityEngine;

public class NotebookTabPage : MonoBehaviour
{
    [SerializeField] private RectTransform nodesParent;
    [SerializeField] private RectTransform linesParent;
    [SerializeField] private RectTransform linePrefab;
    [SerializeField] private float lineThickness = 4f;

    private ClueNode[] nodes;
    private readonly Dictionary<NotebookEntryDataSO, ClueNode> nodeByClue = new Dictionary<NotebookEntryDataSO, ClueNode>();

    void Awake()
    {
        // true = also find inactive nodes
        nodes = nodesParent.GetComponentsInChildren<ClueNode>(true);
        foreach (ClueNode node in nodes)
        {
            if (node.clue == null)
            {
                Debug.LogWarning($"ClueNode '{node.name}' has no clue assigned.");
                continue;
            }
            nodeByClue[node.clue] = node;
        }

        testNode();
    }
    
    void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        foreach (ClueNode node in nodes)
            node.Refresh();

        foreach (Transform child in linesParent)
            Destroy(child.gameObject);

        foreach (ClueNode node in nodes)
        {
            if (!GameManager.Instance.IsDiscovered(node.clue)) continue;

            foreach (NotebookEntryDataSO target in node.clue.leadsTo)
            {
                if (GameManager.Instance.IsDiscovered(target) &&
                    nodeByClue.TryGetValue(target, out ClueNode targetNode))
                {
                    DrawLine(node.Rect, targetNode.Rect);
                }
            }
        }
    }

    private void DrawLine(RectTransform from, RectTransform to)
    {
        if (linePrefab == null)
        {
            // Debug.LogWarning("Line prefab is not assigned.");
            return;
        }

        // Convert both node positions into the lines container's space
        Vector2 start = linesParent.InverseTransformPoint(from.position);
        Vector2 end = linesParent.InverseTransformPoint(to.position);
        Vector2 dir = end - start;

        RectTransform line = Instantiate(linePrefab, linesParent);
        line.localPosition = start;
        line.sizeDelta = new Vector2(dir.magnitude, lineThickness);
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void testNode()
    {
        GameManager.Instance.Discover(nodes[0].clue);
    }
}
