using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class TabMenu : MonoBehaviour
{
    [System.Serializable]
    public struct Tab
    {
        public Button tabButton;
        public GameObject tabContent;
    }

    public GameObject NotebookPanel;

    [SerializeField] 
    private Tab[] tabs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NotebookPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // used by the Input System to toggle the notebook panel on and off
    private void OnToggleNotebook(InputValue value)
    {
        bool isActive = NotebookPanel.activeSelf;
        NotebookPanel.SetActive(!isActive);
        isActive = NotebookPanel.activeSelf;

        if (isActive)
        {
            GlobalClockManager.Instance.StopClock();
            GameManager.Instance.PauseGame();
            // Initialize the tabs
            for (int i = 0; i < tabs.Length; i++)
            {
                var tab = tabs[i];
                tab.tabButton.onClick.AddListener(() => OnTabSelected(tab));
                if (i == 0)
                {
                    tab.tabContent.SetActive(true);
                }
                else
                {   
                    tab.tabContent.SetActive(false);
                }
            }
        } else
        {
            GameManager.Instance.ResumeGame();
            GlobalClockManager.Instance.StartClock();
        }
    }

    private void OnTabSelected(Tab selectedTab)
    {
        foreach (var tab in tabs)
        {
            bool isSelected = tab.Equals(selectedTab);
            tab.tabContent.SetActive(isSelected);
        }
    }
}
