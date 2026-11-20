using UnityEngine.UI;
using UnityEngine;

public class TabMenu : MonoBehaviour
{
    [System.Serializable]
    public struct Tab
    {
        public Button tabButton;
        public GameObject tabContent;
    }

    [SerializeField] 
    private Tab[] tabs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
